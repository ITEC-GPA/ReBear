using System;
using System.Diagnostics;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;

/// <summary>Timing of the long adapter operations (console only, not a pass/fail check).</summary>
internal static class Benchmark
{
    private static SectionCheckerModelCode2010 Checker(ReinforcedConcreteSection section, SectionSolver.DomainPointStrategyTypes strategy,
        bool nonlinear = false) => new(new SectionCheckerAttribute(section), new SectionCheckerModelCode2010.SectionOptionsModelCode2010
        {
            ForceReferenceCoordinateSystem = CoordinateSystem.Global,
            FailureDomainType = SectionSolver.FailureDomainTypes.Plastic,
            FailureAnalysisType = SectionSolver.FailureAnalysisTypes.ConstantN,
            DomainPointStrategy = strategy,
            StressAnalysisType = nonlinear ? SectionSolver.StressAnalysisTypes.NonLinear : SectionSolver.StressAnalysisTypes.Linear,
            PsiCoefficientRebar = 1.5
        }, new StandardNTC2018Concrete());

    private static ReinforcedConcreteSection Section(double bar = 20) =>
        SectionBuilders.Rectangle(ConcreteMaterialEN1992Data.C30_37, SteelMaterialEN1992Data.B450C, 300, 500, 30, 8, bar, 4, 4, 0);

    private static ResultBeamForces[] Cases(int count) => Enumerable.Range(0, count).Select(i => new ResultBeamForces(
        -100e3 - 700e3 * i / count, 0, 40e3 + 20e3 * (i % 5), 0, 20e6 + 100e6 * i / count, 5e6 * (i % 7),
        CoordinateSystem.Global, name: "C" + (i + 1))).ToArray();

    private static double Time(Action action, int repeat = 1)
    {
        action(); // warm-up: JIT and lazy section meshes
        var watch = Stopwatch.StartNew();
        for (int i = 0; i < repeat; i++) action();
        return watch.Elapsed.TotalSeconds / repeat;
    }

    internal static void Run()
    {
        var section = Section();
        var forces = Cases(100);
        var shear = new ShearOptions(SteelMaterialEN1992Data.B450C, SteelMaterialEN1992Data.B450C, 300, 452, 400 * Math.PI,
            2 * Math.PI * 16, 150, CotTheta: 1.5);
        var sle = new SleOptions(0, "XC4", false, false, true, 38);
        foreach (var strategy in new[] { SectionSolver.DomainPointStrategyTypes.Intersection, SectionSolver.DomainPointStrategyTypes.Iterative })
        {
            double build = Time(() => Checker(section, strategy), 5);
            var checker = Checker(section, strategy);
            double batch = Time(() => SectionChecks.Batch(checker, forces, "S", true, y: shear, sle: sle));
            var candidates = Enumerable.Range(0, 10).Select(i => Checker(Section(12 + 2 * i), strategy)).ToArray();
            double search = Time(() => SectionAnalyses.Search(candidates, forces, 1));
            double slice = Time(() => SectionAnalyses.Slice(checker, -300e3, 64));
            var nonlinear = Checker(section, strategy, true);
            double curvature = Time(() => SectionAnalyses.MomentCurvature(nonlinear, -300e3, 0, 60, 1, 1000));
            Console.WriteLine(FormattableString.Invariant(
                $"BENCH {strategy}: construction {build * 1000:F1} ms; Batch 100 NM+V+SLE {batch:F3} s; Search 10x100 {search:F3} s; Slice 64 {slice:F3} s; M-chi 60 {curvature:F3} s"));
        }
    }
}
