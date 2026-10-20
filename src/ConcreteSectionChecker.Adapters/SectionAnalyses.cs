using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Response;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;

namespace ConcreteSectionChecker.Adapters
{
    public static class SectionAnalyses
    {
        public static SectionCheckerModelCode2010 Detached(SectionCheckerModelCode2010 source,
            SectionSolver.FailureAnalysisTypes? criterion = null, int? angles = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var p = source.SectionCheckerOptions;
            var options = new SectionCheckerModelCode2010.SectionOptionsModelCode2010(
                new CoordinateSystem(p.ForceReferenceCoordinateSystem), criterion ?? p.FailureAnalysisType, p.FailureDomainType,
                p.StressAnalysisType, p.PsiCoefficientRebar, p.PsiCoefficientTendon, p.ConsiderTensileConcrete, angles ?? p.TetaDiscretization)
            { DomainPointStrategy = p.DomainPointStrategy, StressAnalysisConcreteTension = p.StressAnalysisConcreteTension };
            return new SectionCheckerModelCode2010(new SectionCheckerAttribute(source.SectionCheckerAttribute.Section), options, source.StandardModelCode2010);
        }

        public static MomentCurvatureResult MomentCurvature(SectionCheckerModelCode2010 source, double n, double direction,
            int steps, double endFraction, double axialTolerance)
        {
            var checker = Detached(source, SectionSolver.FailureAnalysisTypes.ConstantN);
            var section = checker.SectionCheckerAttribute.Section as ReinforcedConcreteSection
                ?? throw new NotSupportedException("Sezione in calcestruzzo armato richiesta.");
            var bars = section.Rebars.ToArray();
            if (bars.Length == 0 || bars.Any(b => b.EpsilonP != 0))
                throw new NotSupportedException("Momento-curvatura: barre ordinarie senza predeformazioni richieste.");
            double yield = Math.Abs(bars[0].RebarMaterial.CalculateFyd(checker.StandardModelCode2010)) / bars[0].RebarMaterial.E;
            if (bars.Any(b => Math.Abs(Math.Abs(b.RebarMaterial.CalculateFyd(checker.StandardModelCode2010)) / b.RebarMaterial.E - yield) > 1e-12))
                throw new NotSupportedException("Momento-curvatura: la soglia di snervamento GPC richiede barre con uguale deformazione di snervamento.");
            return MomentCurvatureAnalysis.Calculate(new MomentCurvatureRequest(n, direction, steps, endFraction,
                axialTolerance: axialTolerance), checker, section, checker.SectionCheckerOptions.ForceReferenceCoordinateSystem, yield);
        }

        public static IReadOnlyList<(double Mx, double My, double N)> Slice(SectionCheckerModelCode2010 source, double n, int samples)
        {
            if (!double.IsFinite(n) || samples < 8 || samples > 360) throw new ArgumentException("N finito e 8–360 direzioni richiesti.");
            var checker = Detached(source, SectionSolver.FailureAnalysisTypes.ConstantN, samples);
            var result = new List<(double, double, double)>();
            for (int i = 0; i < samples; i++)
            {
                double theta = 2 * Math.PI * i / samples;
                var f = new ResultBeamForces(n, 0, 0, 0, 1e6 * Math.Cos(theta), 1e6 * Math.Sin(theta),
                    checker.SectionCheckerOptions.ForceReferenceCoordinateSystem);
                var r = ResistanceChecks.RequirePoint(checker, f);
                result.Add((r.M1.Value / 1e6, r.M2.Value / 1e6, r.N.Value / 1000));
            }
            return result;
        }

        public static (int Index, IReadOnlyList<CheckResult> Results) Search(IReadOnlyList<SectionCheckerModelCode2010> candidates,
            IReadOnlyList<ResultBeamForces> forces, double target)
        {
            if (candidates == null || candidates.Count == 0 || candidates.Count > 100 ||
                forces == null || forces.Count == 0 || forces.Count > 1000 ||
                !double.IsFinite(target) || target <= 0 || target > 1)
                throw new ArgumentException("Ricerca: 1–100 candidati, 1–1000 casi e 0 < utilizzo obiettivo ≤ 1.");
            int best = -1; double area = double.PositiveInfinity;
            var results = new List<CheckResult>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] == null) throw new ArgumentException("Checker candidato vuoto.");
                var current = SectionChecks.Batch(Detached(candidates[i]), forces, "Candidate " + i, true);
                results.AddRange(current);
                double steelArea = candidates[i].SectionCheckerAttribute.Section.AreaRebars;
                if (current.All(r => r.Status == CheckStatus.Satisfied && r.Ratio.HasValue && r.Ratio.Value <= target)
                    && steelArea < area) { best = i; area = steelArea; }
            }
            return (best, results);
        }
    }
}
