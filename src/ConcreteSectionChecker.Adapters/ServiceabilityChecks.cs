using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Cracking;
using GPC.Checkers.Concrete.Results;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Serviceability;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;

namespace ConcreteSectionChecker.Adapters
{
    /// <summary>
    /// Auxiliary linear checkers with tensile concrete for the decompression criterion, one per section, standard, axes and ψ.
    /// Use one instance per batch on one thread; building the checker is the dominant cost of an uncracked analysis.
    /// </summary>
    public sealed class UncrackedCheckers
    {
        // Sections and standards are compared by reference: a batch normally holds one entry.
        private readonly List<(object Section, object Standard, string Axes, double PsiRebar, double PsiTendon, SectionCheckerModelCode2010 Checker)> items = new();
        public int Count { get { lock (items) return items.Count; } }
        internal SectionCheckerModelCode2010 Get(ReinforcedConcreteSection section, StandardModelCode2010 standard, CoordinateSystem axes,
            double psiRebar, double psiTendon)
        {
            string Vector(double x, double y, double z) => string.Join(",", new[] { x, y, z }.Select(v => v.ToString("R", CultureInfo.InvariantCulture)));
            string key = Vector(axes.Origin.X, axes.Origin.Y, axes.Origin.Z) + "|" + Vector(axes.V1.X, axes.V1.Y, axes.V1.Z) + "|" + Vector(axes.V2.X, axes.V2.Y, axes.V2.Z);
            lock (items)
            {
                foreach (var item in items)
                    if (ReferenceEquals(item.Section, section) && ReferenceEquals(item.Standard, standard) && item.Axes == key
                        && item.PsiRebar.Equals(psiRebar) && item.PsiTendon.Equals(psiTendon)) return item.Checker;
                var checker = ServiceabilityChecks.UncrackedChecker(section, standard, axes, psiRebar, psiTendon);
                items.Add((section, standard, key, psiRebar, psiTendon, checker));
                return checker;
            }
        }
    }

    public static class ServiceabilityChecks
    {
        // Historical value-list IDs must not be cast to the differently ordered GPC enum.
        public static ServiceabilityCombination Combination(int historicalId) => historicalId switch
        {
            0 => ServiceabilityCombination.QuasiPermanent,
            1 => ServiceabilityCombination.Frequent,
            2 => ServiceabilityCombination.Characteristic,
            _ => throw new ArgumentOutOfRangeException(nameof(historicalId), "Combinazione SLE non valida.")
        };

        public static StressLimitResult Stress(StressAnalysisResult stress, int combination)
        {
            if (stress is null) throw new ArgumentNullException(nameof(stress));
            var reason = StressLimitCheck.NotApplicableReason(stress.Standard);
            if (reason is not null) throw new NotSupportedException(reason);
            return StressLimitCheck.Evaluate(stress, Combination(combination));
        }

        internal static SectionCheckerModelCode2010 UncrackedChecker(ReinforcedConcreteSection section, StandardModelCode2010 standard,
            CoordinateSystem axes, double psiRebar, double psiTendon)
        {
            var options = new SectionCheckerModelCode2010.SectionOptionsModelCode2010
            {
                ForceReferenceCoordinateSystem = axes,
                StressAnalysisType = SectionSolver.StressAnalysisTypes.Linear,
                ConsiderTensileConcrete = true,
                PsiCoefficientRebar = psiRebar,
                PsiCoefficientTendon = psiTendon
            };
            return new SectionCheckerModelCode2010(new SectionCheckerAttribute(section), options, standard);
        }

        public static SectionCrackResult Cracking(StressAnalysisResult stress, double barCover, int combination,
            int exposure, int sensitivity, int barType, int duration,
            double? designLimit = null, double? spacingOverride = null, double? coverOverride = null, bool concentricRings = false,
            UncrackedCheckers cache = null)
        {
            if (stress is null) throw new ArgumentNullException(nameof(stress));
            if (exposure < 0 || exposure >= CrackRequirements.ExposureClasses.Count)
                throw new ArgumentOutOfRangeException(nameof(exposure));
            if (sensitivity < 0 || sensitivity > 1 || barType < 0 || barType > 1 || duration < 0 || duration > 1)
                throw new ArgumentException("Sensibilità, aderenza e durata devono essere 0 oppure 1.");
            if (!double.IsFinite(barCover) || barCover < 0) throw new ArgumentOutOfRangeException(nameof(barCover));
            if (stress.ConcreteSection is not ReinforcedConcreteSection section
                || section.ConcreteMaterial is not ConcreteMaterialEuropeanCommon concrete
                || stress.Standard is not StandardModelCode2010 standard)
                throw new NotSupportedException("Fessurazione: tipo di sezione, materiale o norma non supportato.");
            var captured = LegacySectionCalculation.Capture(stress);
            if (captured.Diagnostics.Status != CalculationStatus.Completed)
                throw new InvalidOperationException(captured.Diagnostics.Message);
            var ordinary = section.Rebars.Where(b => b.EpsilonP == 0
                && b.RebarMaterial.SteelType != SteelMaterial.SteelTypes.Tendon).ToArray();
            if (ordinary.Length == 0) throw new ArgumentException("Fessurazione: occorre almeno una barra ordinaria.");
            double es = ordinary[0].RebarMaterial.E;
            if (ordinary.Any(b => b.RebarMaterial.E != es))
                throw new NotSupportedException("Fessurazione: la formula GPC richiede un unico modulo elastico delle barre.");

            double UncrackedMaximumStress()
            {
                double psiRebar = stress.PsiRebar ?? 0, psiTendon = stress.PsiTendon ?? 0;
                var checker = cache?.Get(section, standard, stress.Force.CoordinateSystem, psiRebar, psiTendon)
                    ?? UncrackedChecker(section, standard, stress.Force.CoordinateSystem, psiRebar, psiTendon);
                var result = checker.GetTensionAnalysisResult(stress.Force);
                var response = LegacySectionCalculation.Capture(result);
                if (response.Diagnostics.Status != CalculationStatus.Completed)
                    throw new InvalidOperationException(response.Diagnostics.Message);
                return response.Concrete.Max(p => p.Stress);
            }

            var input = new SectionCrackInput(standard, Combination(combination), CrackRequirements.ExposureClasses[exposure],
                sensitivity == 0, designLimit, CrackSectionGeometry.From(section, concentricRings), stress.StrainPlane,
                SectionCrackInput.OrdinaryBarStresses(stress, section), stress.LinearElasticAnalysis,
                stress.SectionSolver.ConsiderTensileConcrete,
                section.Rebars.Any(b => b.EpsilonP != 0 || b.RebarMaterial.SteelType == SteelMaterial.SteelTypes.Tendon),
                es, concrete.Ecm, concrete.Fctm, duration == 0, barType == 0, barCover,
                coverOverride: coverOverride, spacingOverride: spacingOverride,
                uncrackedMaximumConcreteStress: UncrackedMaximumStress);
            return SectionCrackCheck.Evaluate(input);
        }
    }
}
