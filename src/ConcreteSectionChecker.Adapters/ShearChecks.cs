using System;
using System.Linq;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Shear;
using GPC.Model.Materials;
using GPC.Model.Results;

namespace ConcreteSectionChecker.Adapters
{
    /// <summary>
    /// Maps the historical Grasshopper inputs to GPC. Geometry: mm, mm²; actions: N, Nmm;
    /// compression negative. No shear resistance formula is implemented here.
    /// </summary>
    public static class ShearChecks
    {
        public static SectionShearResult WithoutStirrups(SectionCheckerModelCode2010 checker, ResultBeamForces forces,
            double height, double width, double barDiameter, double barCount, double cover, double concreteArea)
        {
            RequireModels(checker, forces);
            Positive(height, nameof(height)); Positive(width, nameof(width)); NonNegative(cover, nameof(cover));
            double asl = BarArea(barDiameter, barCount);
            return Calculate(checker, forces, forces.V2, forces.M1, concreteArea, width,
                height - cover - barDiameter / 2, asl, 0, 1, 90, cover, 0);
        }

        /// <returns>Unreinforced X, unreinforced Y, reinforced X, reinforced Y. Reinforced entries null if the diameter is zero.</returns>
        public static SectionShearResult[] Rectangular(SectionCheckerModelCode2010 checker, ResultBeamForces forces,
            double height, double width, double linkDiameter, int legsX, int legsY, double spacing,
            double alphaX, double alphaY, double barDiameterX, double barDiameterY,
            int barCountX, int barCountY, double cover, double concreteArea)
        {
            RequireModels(checker, forces);
            Positive(height, nameof(height)); Positive(width, nameof(width)); NonNegative(cover, nameof(cover));
            NonNegative(linkDiameter, nameof(linkDiameter));
            // Historical input CC is the cover to the longitudinal bar surface, not to the link.
            double dx = width - cover - barDiameterX / 2, dy = height - cover - barDiameterY / 2;
            double aslX = BarArea(barDiameterX, barCountX), aslY = BarArea(barDiameterY, barCountY);
            var x0 = Calculate(checker, forces, forces.V1, forces.M2, concreteArea, height, dx, aslX, 0, 1, 90, cover, 0);
            var y0 = Calculate(checker, forces, forces.V2, forces.M1, concreteArea, width, dy, aslY, 0, 1, 90, cover, 0);
            if (linkDiameter == 0) return new[] { x0, y0, null, null };
            Positive(legsX, nameof(legsX)); Positive(legsY, nameof(legsY));
            var x = Calculate(checker, forces, forces.V1, forces.M2, concreteArea, height, dx, aslX,
                BarArea(linkDiameter, legsX), spacing, alphaX, cover, linkDiameter);
            var y = Calculate(checker, forces, forces.V2, forces.M1, concreteArea, width, dy, aslY,
                BarArea(linkDiameter, legsY), spacing, alphaY, cover, linkDiameter);
            return new[] { x0, y0, x, y };
        }

        public static SectionShearResult Circular(SectionCheckerModelCode2010 checker, ResultBeamForces forces,
            double diameter, double linkDiameter, int legs, double spacing, double alpha, double barDiameter, double cover)
        {
            RequireModels(checker, forces);
            Positive(diameter, nameof(diameter)); Positive(linkDiameter, nameof(linkDiameter));
            Positive(legs, nameof(legs)); Positive(barDiameter, nameof(barDiameter)); NonNegative(cover, nameof(cover));
            // Preserve the explicit historical circular-pier model (z/d from the GPC NTC API).
            // The old component has no input for a different circular lever-arm model.
            var profile = ShearProfiles.Resolve(checker.StandardModelCode2010);
            double d = diameter - cover - linkDiameter - barDiameter / 2;
            double lever = ShearLeverArm.Ratio(profile, d, cover, linkDiameter, circular: true, ntcPier: true);
            return Calculate(checker, forces, Math.Sqrt(forces.V1 * forces.V1 + forces.V2 * forces.V2), 0,
                Math.PI * diameter * diameter / 4, diameter, d, 0, BarArea(linkDiameter, legs), spacing, alpha,
                cover, linkDiameter, lever);
        }

        private static SectionShearResult Calculate(SectionCheckerModelCode2010 checker, ResultBeamForces forces,
            double v, double m, double area, double bw, double d, double asl, double asw, double spacing,
            double alpha, double cover, double linkDiameter, double? assignedLever = null)
        {
            RequireModels(checker, forces);
            // The old ports have neither a separate link grade nor effective Asl for prestress.
            var section = checker.SectionCheckerAttribute.Section;
            var bars = section.Rebars.ToArray();
            if (bars.Any(b => b.EpsilonP != 0 || b.RebarMaterial.SteelType == SteelMaterial.SteelTypes.Tendon))
                throw new NotSupportedException("Taglio: la verifica GPC richiamata non supporta la precompressione.");
            if (bars.Length == 0) throw new ArgumentException("Taglio: specificare il materiale delle barre nella sezione.");
            var steel = bars[0].RebarMaterial;
            var standard = checker.StandardModelCode2010;
            if (bars.Any(b => b.RebarMaterial.CalculateFyd(standard) != steel.CalculateFyd(standard)
                || b.RebarMaterial.E != steel.E))
                throw new NotSupportedException("Taglio: il componente storico richiede un unico materiale per barre e staffe.");
            var concrete = section.ConcreteMaterial as ConcreteMaterialEuropeanCommon
                ?? throw new NotSupportedException("Taglio: materiale di calcestruzzo non supportato.");
            var profile = ShearProfiles.Resolve(standard);
            var lever = assignedLever ?? ShearLeverArm.Ratio(profile, d, cover, linkDiameter);
            return SectionShearCalculator.Calculate(new SectionShearInput(standard, forces.N, v, m, area, bw, d, asl,
                Math.Abs(concrete.Fck), Math.Abs(concrete.CalculateFcd(standard)), Math.Abs(steel.CalculateFyd(standard)),
                standard.GammaC, steel.E, asw, spacing, alpha, leverFactor: lever));
        }

        public static double BarArea(double diameter, double count)
        {
            Positive(diameter, nameof(diameter)); NonNegative(count, nameof(count));
            if (count != Math.Truncate(count)) throw new ArgumentException("Il numero delle barre deve essere intero.");
            return Math.PI * diameter * diameter / 4 * count;
        }

        private static void RequireModels(SectionCheckerModelCode2010 checker, ResultBeamForces forces)
        {
            if (checker is null) throw new ArgumentNullException(nameof(checker));
            if (forces is null) throw new ArgumentNullException(nameof(forces));
        }

        private static void Positive(double v, string name)
        {
            if (!double.IsFinite(v) || v <= 0) throw new ArgumentOutOfRangeException(name, "Valore finito e positivo richiesto.");
        }
        private static void NonNegative(double v, string name)
        {
            if (!double.IsFinite(v) || v < 0) throw new ArgumentOutOfRangeException(name, "Valore finito non negativo richiesto.");
        }
    }
}
