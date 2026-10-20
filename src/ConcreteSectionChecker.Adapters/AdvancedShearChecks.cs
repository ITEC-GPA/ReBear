using System;
using System.Globalization;
using System.Linq;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Shear;
using GPC.Checkers.Concrete.Torsion;
using GPC.Model.Materials;
using GPC.Model.Results;

namespace ConcreteSectionChecker.Adapters
{
    /// <summary>One direction; lengths mm, areas mm². z/d and d are explicit, no cover convention is inferred.</summary>
    public sealed record ShearOptions(SteelMaterial LinkSteel, SteelMaterial LongitudinalSteel, double Width, double Depth, double LongitudinalArea,
        double LinkArea, double Spacing, double Alpha = 90, double? CotTheta = null,
        double LeverFactor = .9, double Aggregate = 20, double Eccentricity = 0);

    public sealed record TorsionOptions(TorsionGeometry Geometry, SteelMaterial LinkSteel, SteelMaterial LongitudinalSteel,
        double LinkLegArea, double Spacing, double LongitudinalArea, double CotTheta, bool HollowBothFaces = false);

    public static class AdvancedShearChecks
    {
        internal static void Ordinary(SectionCheckerModelCode2010 checker)
        {
            if (checker?.SectionCheckerAttribute?.Section == null) throw new ArgumentException("Checker non valido.");
            if (checker.SectionCheckerAttribute.Section.Rebars.Any(b => b.EpsilonP != 0 ||
                b.RebarMaterial.SteelType == SteelMaterial.SteelTypes.Tendon))
                throw new NotSupportedException("Questo modello GPC di taglio/torsione non supporta la precompressione.");
        }

        public static SectionShearInput Input(SectionCheckerModelCode2010 checker, ResultBeamForces force, ShearOptions p, int axis)
        {
            Ordinary(checker); LoadEnvelopes.Validate(force);
            if (p?.LinkSteel == null || p.LongitudinalSteel == null) throw new ArgumentException("Specificare acciaio delle staffe e delle barre longitudinali.");
            RequireOrdinarySteel(p.LinkSteel, p.LongitudinalSteel);
            if (axis != 0 && axis != 1) throw new ArgumentException("Direzione: 0 = X, 1 = Y.");
            var section = checker.SectionCheckerAttribute.Section;
            var concrete = section.ConcreteMaterial as ConcreteMaterialEuropeanCommon
                ?? throw new NotSupportedException("Calcestruzzo europeo richiesto.");
            var standard = checker.StandardModelCode2010;
            // Geometry is explicit in the checker's force axes; transport actions with eccentricity.
            var f = force.ToCoordinateSystemWithEccentricity(checker.SectionCheckerOptions.ForceReferenceCoordinateSystem);
            return new SectionShearInput(standard, f.N, axis == 0 ? f.V1 : f.V2, axis == 0 ? f.M2 : f.M1,
                section.Area, p.Width, p.Depth, p.LongitudinalArea,
                Math.Abs(concrete.Fck), Math.Abs(concrete.CalculateFcd(standard)), Math.Abs(p.LinkSteel.CalculateFyd(standard)),
                standard.GammaC, p.LongitudinalSteel.E, p.LinkArea, p.Spacing, p.Alpha, p.CotTheta, p.LeverFactor, p.Aggregate, p.Eccentricity);
        }

        public static CheckResult Shear(SectionCheckerModelCode2010 checker, ResultBeamForces force, ShearOptions p, int axis, string sectionId)
        {
            var r = SectionShearCalculator.Calculate(Input(checker, force, p, axis));
            var status = r.Verdict == ShearVerdict.NotEvaluated ? CheckStatus.NotEvaluated :
                r.Verdict == ShearVerdict.Satisfied ? CheckStatus.Satisfied : CheckStatus.NotSatisfied;
            return new(sectionId, LoadEnvelopes.Name(force), axis == 0 ? "Shear X" : "Shear Y", status,
                r.Ratio, r.Demand / 1000, status == CheckStatus.NotEvaluated ? null : r.VRd / 1000, "kN",
                r.Status, r.Reference, Details(r.Details));
        }

        public static CheckResult Torsion(SectionCheckerModelCode2010 checker, ResultBeamForces force, TorsionOptions p,
            ShearOptions x, ShearOptions y, string sectionId)
        {
            Ordinary(checker); LoadEnvelopes.Validate(force);
            if (p?.LinkSteel == null || p.LongitudinalSteel == null) throw new ArgumentException("Opzioni di torsione non valide.");
            RequireOrdinarySteel(p.LinkSteel, p.LongitudinalSteel);
            var c = checker.SectionCheckerAttribute.Section.ConcreteMaterial as ConcreteMaterialEuropeanCommon
                ?? throw new NotSupportedException("Calcestruzzo europeo richiesto.");
            var s = checker.StandardModelCode2010;
            var f = force.ToCoordinateSystemWithEccentricity(checker.SectionCheckerOptions.ForceReferenceCoordinateSystem);
            if ((x == null && f.V1 != 0) || (y == null && f.V2 != 0))
                throw new ArgumentException("Le azioni di taglio richiedono le opzioni di entrambe le direzioni attive.");
            foreach (var direction in new[] { x, y }.Where(a => a != null))
                if (direction.Alpha != 90 || direction.Spacing != p.Spacing ||
                    direction.LinkSteel.CalculateFyd(s) != p.LinkSteel.CalculateFyd(s))
                    throw new ArgumentException("Taglio/torsione: staffe a 90°, stesso passo e stesso acciaio richiesti.");
            var input = new SectionTorsionInput(s, f.T, p.Geometry, Math.Abs(c.Fck), Math.Abs(c.CalculateFcd(s)),
                s.GammaC, Math.Abs(p.LinkSteel.CalculateFyd(s)), Math.Abs(p.LongitudinalSteel.CalculateFyd(s)),
                p.LinkLegArea, p.Spacing, p.LongitudinalArea, p.CotTheta, hollowWithReinforcementOnBothFaces: p.HollowBothFaces);
            var r = SectionTorsionCalculator.Calculate(input, x == null ? null : Input(checker, force, x, 0),
                y == null ? null : Input(checker, force, y, 1));
            return new(sectionId, LoadEnvelopes.Name(force), "Torsion + shear",
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
}
