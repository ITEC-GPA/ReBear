using System;
using System.Collections.Generic;
using ConcreteSectionChecker.Grasshopper.Features;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper
{
    /// <summary>
    /// Shared workflow for recovered and new components. Panel prefixes order the workflow;
    /// native exposure sections order related operations without renaming components.
    /// Components in the same exposure section are alternative tools at the same workflow stage.
    /// </summary>
    internal static class ComponentPalette
    {
        internal const string Category = "ReBear";
        internal readonly record struct Placement(string SubCategory, GH_Exposure Exposure);
        private static readonly IReadOnlyDictionary<string, Placement> Items = Create();

        internal static Placement For(Type type) => Items.TryGetValue(type.Name, out var item)
            ? item : throw new InvalidOperationException("Missing ReBear palette placement: " + type.Name);

        private static IReadOnlyDictionary<string, Placement> Create()
        {
            var items = new Dictionary<string, Placement>(StringComparer.Ordinal);
            void Add(string panel, GH_Exposure exposure, params string[] types)
            {
                foreach (string type in types) items.Add(type, new Placement(panel, exposure));
            }

            const string materials = "1 Materials";
            Add(materials, GH_Exposure.primary, nameof(StandardComponent));
            Add(materials, GH_Exposure.secondary, nameof(ConcreteMaterialComponent));
            Add(materials, GH_Exposure.tertiary, nameof(RebarMaterialComponent));
            Add(materials, GH_Exposure.quarternary, nameof(BarMaterialComponent), nameof(TendonMaterialComponent));
            Add(materials, GH_Exposure.quinary, nameof(ConcreteDesignValue), nameof(SteelDesignValue));

            const string sections = "2 Sections";
            Add(sections, GH_Exposure.primary, nameof(RebarComponent), nameof(RebarLayoutComponent), nameof(TendonComponent));
            Add(sections, GH_Exposure.secondary, nameof(RectangularRcSectionComponent), nameof(CircularRcSectionComponent));
            Add(sections, GH_Exposure.tertiary, nameof(TeeRcSectionComponent), nameof(IRcSectionComponent), nameof(BoxRcSectionComponent));
            Add(sections, GH_Exposure.quarternary, nameof(ReinforcedConcreteSectionComponent));
            Add(sections, GH_Exposure.quinary, nameof(SectionGeometryCheckComponent));
            Add(sections, GH_Exposure.senary, nameof(ConcreteMechanicalPropertiesComponent), nameof(HomogenizedMechanicalPropertiesComponent));

            const string input = "3 Input";
            Add(input, GH_Exposure.primary, nameof(CoordinateSystemComponent), nameof(CoordinateSystemSectionComponent));
            Add(input, GH_Exposure.secondary, nameof(SectionForceComponent));
            Add(input, GH_Exposure.tertiary, nameof(LoadEnvelopeComponent));
            Add(input, GH_Exposure.quarternary, nameof(HomogenizationOptionsComponent), nameof(PsiFactorComponent));

            const string setup = "4 Setup";
            Add(setup, GH_Exposure.primary, nameof(CheckerOptionsComponent));
            Add(setup, GH_Exposure.secondary, nameof(CheckerComponent));
            Add(setup, GH_Exposure.tertiary, nameof(ShearOptionsComponent));
            Add(setup, GH_Exposure.quarternary, nameof(TorsionOptionsComponent));
            Add(setup, GH_Exposure.quinary, nameof(SleOptionsComponent));
            Add(setup, GH_Exposure.senary, nameof(PlateDetailingOptionsComponent));
            Add(setup, GH_Exposure.septenary, nameof(DetailingOptionsComponent));

            const string analysis = "5 Analysis";
            Add(analysis, GH_Exposure.primary, nameof(StressAnalysisComponent));
            Add(analysis, GH_Exposure.secondary, nameof(FailureDomain3DComponent));
            Add(analysis, GH_Exposure.tertiary, nameof(DomainSliceComponent));
            Add(analysis, GH_Exposure.quarternary, nameof(MomentCurvatureComponent));

            const string checks = "6 Checks";
            Add(checks, GH_Exposure.primary, nameof(ResistanceCheckComponent), nameof(PostProcessFailureDomain3DComponent));
            Add(checks, GH_Exposure.secondary, nameof(ShearCheckComponent));
            Add(checks, GH_Exposure.tertiary, nameof(RectangularSectionShearCheckNoStirrupComponent),
                nameof(RectangularSectionShearCheckComponent), nameof(CircularSectionShearCheckComponent));
            Add(checks, GH_Exposure.quarternary, nameof(TorsionShearCheckComponent));
            Add(checks, GH_Exposure.quinary, nameof(ServiceabilityCheckComponent));
            Add(checks, GH_Exposure.senary, nameof(ServiceabilityStressCheckComponent), nameof(ServiceabilityCrackWidthControlCheckComponent));
            Add(checks, GH_Exposure.septenary, nameof(BatchSectionChecksComponent));
            Add(checks, GH_Exposure.octonary, nameof(ReinforcementSearchComponent));

            const string detailing = "7 Detailing";
            Add(detailing, GH_Exposure.primary, nameof(ConcreteCoverComponent));
            Add(detailing, GH_Exposure.secondary, nameof(AnchorageLapComponent));
            Add(detailing, GH_Exposure.tertiary, nameof(ReinforcementDetailingComponent));

            const string results = "8 Results";
            Add(results, GH_Exposure.primary, nameof(CheckResultComponent));
            Add(results, GH_Exposure.secondary, nameof(PostProcessStrainResultComponent), nameof(EffectiveCrackedSectionComponent));
            Add(results, GH_Exposure.tertiary, nameof(FailureDomainSummaryComponent));
            Add(results, GH_Exposure.quarternary, nameof(CheckEnvelopeComponent));
            Add(results, GH_Exposure.quinary, nameof(CheckReportComponent));

            const string display = "9 Display";
            Add(display, GH_Exposure.primary, nameof(ViewStressAnalysisResultComponent));
            Add(display, GH_Exposure.secondary, nameof(ColorMeshComponent), nameof(ColorGradientMeshComponent));
            Add(display, GH_Exposure.tertiary, nameof(ViewStressAnalysisResultMeshComponent));
            return items;
        }
    }
}
