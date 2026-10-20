using System;
using System.Linq;
using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Cracking;
using GPC.Checkers.Concrete.Results;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Serviceability;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;

namespace ConcreteSectionChecker.Adapters
{
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
            if (stress == null) throw new ArgumentNullException(nameof(stress));
            var reason = StressLimitCheck.NotApplicableReason(stress.Standard);
            if (reason != null) throw new NotSupportedException(reason);
            return StressLimitCheck.Evaluate(stress, Combination(combination));
        }

        public static SectionCrackResult Cracking(StressAnalysisResult stress, double barCover, int combination,
            int exposure, int sensitivity, int barType, int duration,
            double? designLimit = null, double? spacingOverride = null, double? coverOverride = null, bool concentricRings = false)
        {
            if (stress == null) throw new ArgumentNullException(nameof(stress));
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
                var options = new SectionCheckerModelCode2010.SectionOptionsModelCode2010
                {
                    ForceReferenceCoordinateSystem = stress.Force.CoordinateSystem,
                    StressAnalysisType = SectionSolver.StressAnalysisTypes.Linear,
                    ConsiderTensileConcrete = true,
                    PsiCoefficientRebar = stress.PsiRebar ?? 0,
                    PsiCoefficientTendon = stress.PsiTendon ?? 0
                };
                var checker = new SectionCheckerModelCode2010(new SectionCheckerAttribute(section), options, standard);
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
