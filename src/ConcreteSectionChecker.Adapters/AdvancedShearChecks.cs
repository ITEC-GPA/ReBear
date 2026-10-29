using System;
using System.Globalization;
using System.Linq;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Shear;
using GPC.Checkers.Concrete.Torsion;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Standards;

namespace ConcreteSectionChecker.Adapters
{
    /// <summary>One direction; lengths mm, areas mm². z/d and d are explicit, no cover convention is inferred.</summary>
    public sealed record ShearOptions(SteelMaterial LinkSteel, SteelMaterial LongitudinalSteel, double Width, double Depth, double LongitudinalArea,
        double LinkArea, double Spacing, double Alpha = 90, double? CotTheta = null,
        double LeverFactor = .9, double Aggregate = 20, double Eccentricity = 0);

    public sealed record TorsionOptions(TorsionGeometry Geometry, SteelMaterial LinkSteel, SteelMaterial LongitudinalSteel,
        double LinkLegArea, double Spacing, double LongitudinalArea, double CotTheta, bool HollowBothFaces = false);

    /// <summary>Shear directions in the checker axes: 0 = X (V1/M2), 1 = Y (V2/M1), 2 = resultant (circular sections).</summary>
    public static class ShearDirections
    {
        public const int X = 0, Y = 1, Resultant = 2;
        public static string Name(int axis) => axis switch
        {
            X => "Shear X", Y => "Shear Y", Resultant => "Shear R",
            _ => throw new ArgumentException("Direzione: 0 = X, 1 = Y, 2 = risultante.")
        };
    }

    public static class AdvancedShearChecks
    {
        /// <summary>
        /// Rotation and transport of the actions leave round-off on components that are zero in the source axes
        /// (5.5e-12 N for a 90° rotation). A shear smaller than this relative tolerance of the shear resultant,
        /// or than 1e-6 N, is treated as absent.
        /// </summary>
        public static bool IsShearPresent(double component, double other)
        {
            double scale = Math.Sqrt(component * component + other * other);
            return Math.Abs(component) > Math.Max(1e-6, 1e-9 * scale);
        }

        internal static void Ordinary(SectionCheckerModelCode2010 checker)
        {
            if (checker?.SectionCheckerAttribute?.Section is null) throw new ArgumentException("Checker non valido.");
            if (checker.SectionCheckerAttribute.Section.Rebars.Any(b => b.EpsilonP != 0 ||
                b.RebarMaterial.SteelType == SteelMaterial.SteelTypes.Tendon))
                throw new NotSupportedException("Questo modello GPC di taglio/torsione non supporta la precompressione.");
        }

        public static SectionShearInput Input(SectionCheckerModelCode2010 checker, ResultBeamForces force, ShearOptions p, int axis)
        {
            Ordinary(checker); LoadEnvelopes.Validate(force);
            if (p?.LinkSteel is null || p.LongitudinalSteel is null) throw new ArgumentException("Specificare acciaio delle staffe e delle barre longitudinali.");
            RequireOrdinarySteel(p.LinkSteel, p.LongitudinalSteel);
            _ = ShearDirections.Name(axis);
            var section = checker.SectionCheckerAttribute.Section;
            var concrete = section.ConcreteMaterial as ConcreteMaterialEuropeanCommon
                ?? throw new NotSupportedException("Calcestruzzo europeo richiesto.");
            var standard = checker.StandardModelCode2010;
            // Geometry is explicit in the checker's force axes; transport actions with eccentricity.
            var f = force.ToCoordinateSystemWithEccentricity(checker.SectionCheckerOptions.ForceReferenceCoordinateSystem);
            double v = axis switch { ShearDirections.X => f.V1, ShearDirections.Y => f.V2, _ => Math.Sqrt(f.V1 * f.V1 + f.V2 * f.V2) };
            double m = axis switch { ShearDirections.X => f.M2, ShearDirections.Y => f.M1, _ => Math.Sqrt(f.M1 * f.M1 + f.M2 * f.M2) };
            return new SectionShearInput(standard, f.N, v, m,
                section.Area, p.Width, p.Depth, p.LongitudinalArea,
                Math.Abs(concrete.Fck), Math.Abs(concrete.CalculateFcd(standard)), Math.Abs(p.LinkSteel.CalculateFyd(standard)),
                standard.GammaC, p.LongitudinalSteel.E, p.LinkArea, p.Spacing, p.Alpha, p.CotTheta, p.LeverFactor, p.Aggregate, p.Eccentricity);
        }

        public static CheckResult Shear(SectionCheckerModelCode2010 checker, ResultBeamForces force, ShearOptions p, int axis, string sectionId, int index = 0)
        {
            var r = SectionShearCalculator.Calculate(Input(checker, force, p, axis));
            var status = r.Verdict == ShearVerdict.NotEvaluated ? CheckStatus.NotEvaluated :
                r.Verdict == ShearVerdict.Satisfied ? CheckStatus.Satisfied : CheckStatus.NotSatisfied;
            return new(sectionId, LoadEnvelopes.Name(force, index), ShearDirections.Name(axis), status,
                r.Ratio, r.Demand / 1000, status == CheckStatus.NotEvaluated ? null : r.VRd / 1000, "kN",
                r.Status, r.Reference, Details(r.Details) + FormattableString.Invariant(
                    $"\nModel={r.Model}; VRsd={Kilo(r, r.VRsd)} kN; VRcd={Kilo(r, r.VRcd)} kN; cotθ={r.CotTheta}"));
        }
        private static double? Kilo(SectionShearResult r, double value) => r.Verdict == ShearVerdict.NotEvaluated ? null : value / 1000;

        public static CheckResult Torsion(SectionCheckerModelCode2010 checker, ResultBeamForces force, TorsionOptions p,
            ShearOptions x, ShearOptions y, string sectionId, int index = 0)
        {
            Ordinary(checker); LoadEnvelopes.Validate(force);
            if (p?.LinkSteel is null || p.LongitudinalSteel is null) throw new ArgumentException("Opzioni di torsione non valide.");
            RequireOrdinarySteel(p.LinkSteel, p.LongitudinalSteel);
            var c = checker.SectionCheckerAttribute.Section.ConcreteMaterial as ConcreteMaterialEuropeanCommon
                ?? throw new NotSupportedException("Calcestruzzo europeo richiesto.");
            var s = checker.StandardModelCode2010;
            var f = force.ToCoordinateSystemWithEccentricity(checker.SectionCheckerOptions.ForceReferenceCoordinateSystem);
            if ((x is null && IsShearPresent(f.V1, f.V2)) || (y is null && IsShearPresent(f.V2, f.V1)))
                throw new ArgumentException("Le azioni di taglio richiedono le opzioni di entrambe le direzioni attive.");
            foreach (var direction in new[] { x, y }.Where(a => a is not null))
                if (direction.Alpha != 90 || direction.Spacing != p.Spacing ||
                    direction.LinkSteel.CalculateFyd(s) != p.LinkSteel.CalculateFyd(s))
                    throw new ArgumentException("Taglio/torsione: staffe a 90°, stesso passo e stesso acciaio richiesti.");
            var input = new SectionTorsionInput(s, f.T, p.Geometry, Math.Abs(c.Fck), Math.Abs(c.CalculateFcd(s)),
                s.GammaC, Math.Abs(p.LinkSteel.CalculateFyd(s)), Math.Abs(p.LongitudinalSteel.CalculateFyd(s)),
                p.LinkLegArea, p.Spacing, p.LongitudinalArea, p.CotTheta, hollowWithReinforcementOnBothFaces: p.HollowBothFaces);
            var r = SectionTorsionCalculator.Calculate(input, x is null ? null : Input(checker, force, x, 0),
                y is null ? null : Input(checker, force, y, 1));
            return new(sectionId, LoadEnvelopes.Name(force, index), "Torsion + shear",
                r.Verdict == TorsionVerdict.Satisfied ? CheckStatus.Satisfied : CheckStatus.NotSatisfied,
                r.Ratio, r.Demand / 1e6, r.TRd / 1e6, "kNm", r.Status + " " + string.Join("; ", r.Limitations),
                r.Reference, Details(r.Details) + FormattableString.Invariant($"\nTorsionRatio={r.TorsionRatio}; ConcreteInteraction={r.ConcreteInteraction}; LinkInteraction={r.LinkInteraction}"));
        }
        private static void RequireOrdinarySteel(params SteelMaterial[] materials)
        {
            if (materials.Any(m => m.SteelType == SteelMaterial.SteelTypes.Tendon))
                throw new NotSupportedException("Taglio/torsione: acciai per barre e staffe ordinarie richiesti.");
        }
        internal static string Details(System.Collections.Generic.IEnumerable<ShearCalculationDetail> details) =>
            string.Join("\n", details.Select(d => $"{d.Symbol}={d.Value.ToString("G17", CultureInfo.InvariantCulture)} {d.Unit}: {d.Expression}"));
    }

    /// <summary>
    /// Shear options from the geometry of the 0.2 rectangular/circular shear components (replaced in 0.3 by Shear Check).
    /// Only geometry is mapped here; resistance formulas, lever arms and profiles stay in GPC.
    /// </summary>
    public static class ShearGeometry
    {
        public static double BarArea(double diameter, double count)
        {
            Positive(diameter, "diametro"); NonNegative(count, "numero barre");
            if (count != Math.Truncate(count)) throw new ArgumentException("Il numero delle barre deve essere intero.");
            return Math.PI * diameter * diameter / 4 * count;
        }

        /// <summary>
        /// Rectangle b (X) × h (Y). <paramref name="barCover"/> is measured to the surface of the longitudinal bars, as in 0.2.
        /// X: bw = h, d = b − c − ØX/2; Y: bw = b, d = h − c − ØY/2. Link diameter 0 = no links (members without shear reinforcement).
        /// </summary>
        public static (ShearOptions X, ShearOptions Y) Rectangular(StandardModelCode2010 standard, SteelMaterial linkSteel, SteelMaterial longitudinalSteel,
            double width, double height, double barCover, double linkDiameter, int legsX, int legsY, double spacing, double alphaX, double alphaY,
            double barDiameterX, double barDiameterY, int barCountX, int barCountY, double? cotTheta = null, double aggregate = 20)
        {
            Steel(standard, linkSteel, longitudinalSteel);
            Positive(width, "base"); Positive(height, "altezza"); NonNegative(barCover, "copriferro"); NonNegative(linkDiameter, "diametro staffa");
            var profile = ShearProfiles.Resolve(standard);
            double dx = width - barCover - barDiameterX / 2, dy = height - barCover - barDiameterY / 2;
            Positive(dx, "altezza utile X"); Positive(dy, "altezza utile Y");
            bool links = linkDiameter > 0;
            if (links) { Positive(legsX, "bracci X"); Positive(legsY, "bracci Y"); Positive(spacing, "passo"); }
            ShearOptions Direction(double bw, double d, double barDiameter, int count, int legs, double alpha) => new(linkSteel, longitudinalSteel,
                bw, d, BarArea(barDiameter, count), links ? BarArea(linkDiameter, legs) : 0, links ? spacing : 1, links ? alpha : 90, cotTheta,
                ShearLeverArm.Ratio(profile, d, barCover, linkDiameter), aggregate);
            return (Direction(height, dx, barDiameterX, barCountX, legsX, alphaX), Direction(width, dy, barDiameterY, barCountY, legsY, alphaY));
        }

        /// <summary>
        /// Circular pier of diameter D; <paramref name="linkCover"/> is measured to the outside of the link, as in 0.2.
        /// d = D − c − Øw − Ø/2 and z/d from the GPC NTC circular-pier model (NTC 2018 only); use direction 2 (resultant).
        /// </summary>
        public static ShearOptions Circular(StandardModelCode2010 standard, SteelMaterial linkSteel, SteelMaterial longitudinalSteel,
            double diameter, double linkCover, double linkDiameter, int legs, double spacing, double alpha, double barDiameter,
            double? cotTheta = null, double aggregate = 20)
        {
            Steel(standard, linkSteel, longitudinalSteel);
            Positive(diameter, "diametro"); Positive(linkDiameter, "diametro staffa"); Positive(legs, "bracci");
            Positive(spacing, "passo"); Positive(barDiameter, "diametro barre"); NonNegative(linkCover, "copriferro");
            var profile = ShearProfiles.Resolve(standard);
            double d = diameter - linkCover - linkDiameter - barDiameter / 2;
            Positive(d, "altezza utile");
            double lever = ShearLeverArm.Ratio(profile, d, linkCover, linkDiameter, circular: true, ntcPier: true);
            return new ShearOptions(linkSteel, longitudinalSteel, diameter, d, 0, BarArea(linkDiameter, legs), spacing, alpha, cotTheta, lever, aggregate);
        }

        private static void Steel(StandardModelCode2010 standard, SteelMaterial link, SteelMaterial longitudinal)
        {
            if (standard is null || link is null || longitudinal is null) throw new ArgumentException("Norma, acciaio delle staffe e acciaio longitudinale richiesti.");
        }
        private static void Positive(double v, string name)
        {
            if (!double.IsFinite(v) || v <= 0) throw new ArgumentException($"Taglio: {name} finito e positivo richiesto.");
        }
        private static void NonNegative(double v, string name)
        {
            if (!double.IsFinite(v) || v < 0) throw new ArgumentException($"Taglio: {name} finito e non negativo richiesto.");
        }
    }
}
