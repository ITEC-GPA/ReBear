using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Response;
using GPC.Checkers.Concrete.Results;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;

namespace ConcreteSectionChecker.Adapters
{
    public static class SectionAnalyses
    {
        /// <summary>Independent copy of the options: the GPC solver keeps a reference to the options object it was built with.</summary>
        public static SectionCheckerModelCode2010.SectionOptionsModelCode2010 Copy(SectionCheckerModelCode2010.SectionOptionsModelCode2010 p,
            SectionSolver.FailureAnalysisTypes? criterion = null, int? angles = null)
        {
            if (p is null) throw new ArgumentNullException(nameof(p));
            return new SectionCheckerModelCode2010.SectionOptionsModelCode2010(
                new CoordinateSystem(p.ForceReferenceCoordinateSystem), criterion ?? p.FailureAnalysisType, p.FailureDomainType,
                p.StressAnalysisType, p.PsiCoefficientRebar, p.PsiCoefficientTendon, p.ConsiderTensileConcrete, angles ?? p.TetaDiscretization)
            { DomainPointStrategy = p.DomainPointStrategy, StressAnalysisConcreteTension = p.StressAnalysisConcreteTension };
        }

        /// <summary>
        /// A new checker with its own solver and options. Use it to change options, and to isolate a background calculation:
        /// the legacy checker is not thread-safe and a checker received from Grasshopper may be used by other components.
        /// </summary>
        public static SectionCheckerModelCode2010 Detached(SectionCheckerModelCode2010 source,
            SectionSolver.FailureAnalysisTypes? criterion = null, int? angles = null)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            return new SectionCheckerModelCode2010(new SectionCheckerAttribute(source.SectionCheckerAttribute.Section),
                Copy(source.SectionCheckerOptionsModelCode2010, criterion, angles), source.StandardModelCode2010);
        }

        /// <summary>The source itself when its options already match, otherwise a detached copy with the requested options.</summary>
        public static SectionCheckerModelCode2010 Prepared(SectionCheckerModelCode2010 source,
            SectionSolver.FailureAnalysisTypes? criterion = null, int? angles = null)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var p = source.SectionCheckerOptions;
            bool same = (!criterion.HasValue || p.FailureAnalysisType == criterion.Value) && (!angles.HasValue || p.TetaDiscretization == angles.Value);
            return same ? source : Detached(source, criterion, angles);
        }

        /// <summary>Stress analyses of the checker for every force, without rebuilding the checker (GPC solves the list in parallel).</summary>
        public static StressAnalysisResult[] Stress(SectionCheckerModelCode2010 checker, IReadOnlyList<ResultBeamForces> forces)
        {
            if (checker is null || forces is null || forces.Any(f => f is null)) throw new ArgumentException("Checker e azioni richiesti.");
            foreach (var f in forces) LoadEnvelopes.Validate(f);
            var o = checker.SectionCheckerOptions;
            var array = forces.ToArray();
            return o.StressAnalysisType == SectionSolver.StressAnalysisTypes.NonLinear
                ? checker.SectionSolver.GetStressAnalysisResults(array)
                : checker.SectionSolver.GetLinearStressAnalysisResults(array, o.PsiCoefficientRebar, o.PsiCoefficientTendon, o);
        }

        public static MomentCurvatureResult MomentCurvature(SectionCheckerModelCode2010 source, double n, double direction,
            int steps, double endFraction, double axialTolerance, CancellationToken token = default)
        {
            var checker = Prepared(source, SectionSolver.FailureAnalysisTypes.ConstantN);
            var section = checker.SectionCheckerAttribute.Section as ReinforcedConcreteSection
                ?? throw new NotSupportedException("Sezione in calcestruzzo armato richiesta.");
            var bars = section.Rebars.ToArray();
            if (bars.Length == 0 || bars.Any(b => b.EpsilonP != 0))
                throw new NotSupportedException("Momento-curvatura: barre ordinarie senza predeformazioni richieste.");
            double yield = Math.Abs(bars[0].RebarMaterial.CalculateFyd(checker.StandardModelCode2010)) / bars[0].RebarMaterial.E;
            if (bars.Any(b => Math.Abs(Math.Abs(b.RebarMaterial.CalculateFyd(checker.StandardModelCode2010)) / b.RebarMaterial.E - yield) > 1e-12))
                throw new NotSupportedException("Momento-curvatura: la soglia di snervamento GPC richiede barre con uguale deformazione di snervamento.");
            return MomentCurvatureAnalysis.Calculate(new MomentCurvatureRequest(n, direction, steps, endFraction,
                axialTolerance: axialTolerance), checker, section, checker.SectionCheckerOptions.ForceReferenceCoordinateSystem, yield, token);
        }

        public static IReadOnlyList<(double Mx, double My, double N)> Slice(SectionCheckerModelCode2010 source, double n, int samples,
            CancellationToken token = default)
        {
            if (!double.IsFinite(n) || samples < 8 || samples > 360) throw new ArgumentException("N finito e 8–360 direzioni richiesti.");
            var checker = Prepared(source, SectionSolver.FailureAnalysisTypes.ConstantN, samples);
            var result = new List<(double, double, double)>();
            for (int i = 0; i < samples; i++)
            {
                token.ThrowIfCancellationRequested();
                double theta = 2 * Math.PI * i / samples;
                var f = new ResultBeamForces(n, 0, 0, 0, 1e6 * Math.Cos(theta), 1e6 * Math.Sin(theta),
                    checker.SectionCheckerOptions.ForceReferenceCoordinateSystem);
                var r = ResistanceChecks.RequirePoint(checker, f, token);
                result.Add((r.M1.Value / 1e6, r.M2.Value / 1e6, r.N.Value / 1000));
            }
            return result;
        }

        /// <summary>
        /// Minimum steel area among the candidates that satisfy every N-Mx-My case with utilization ≤ target; ties keep the first.
        /// Candidates are checked in parallel: distinct checker objects run on different threads, a checker passed more than once
        /// is used by one thread only. <paramref name="isolate"/> gives every candidate a private copy (for checkers shared with
        /// other Grasshopper components while the search runs in the background).
        /// </summary>
        public static (int Index, IReadOnlyList<CheckResult> Results) Search(IReadOnlyList<SectionCheckerModelCode2010> candidates,
            IReadOnlyList<ResultBeamForces> forces, double target, CancellationToken token = default, bool isolate = false)
        {
            if (candidates is null || candidates.Count == 0 || candidates.Count > 100 ||
                forces is null || forces.Count == 0 || forces.Count > 1000 ||
                !double.IsFinite(target) || target <= 0 || target > 1)
                throw new ArgumentException("Ricerca: 1–100 candidati, 1–1000 casi e 0 < utilizzo obiettivo ≤ 1.");
            if (candidates.Any(c => c is null)) throw new ArgumentException("Checker candidato vuoto.");
            var perCandidate = new IReadOnlyList<CheckResult>[candidates.Count];
            var groups = Enumerable.Range(0, candidates.Count)
                .GroupBy(i => candidates[i], ReferenceEqualityComparer.Instance).Select(g => g.ToArray()).ToArray();
            try
            {
                Parallel.ForEach(groups, new ParallelOptions { CancellationToken = token }, indices =>
                {
                    var checker = isolate ? Detached(candidates[indices[0]]) : candidates[indices[0]];
                    foreach (int i in indices)
                        perCandidate[i] = SectionChecks.Batch(checker, forces, "Candidate " + i, true, token: token);
                });
            }
            catch (AggregateException e)
            {
                var inner = e.Flatten().InnerExceptions;
                if (inner.All(x => x is OperationCanceledException)) throw new OperationCanceledException(token);
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(inner.First(x => x is not OperationCanceledException)).Throw();
                throw;
            }
            int best = -1; double area = double.PositiveInfinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                double steelArea = candidates[i].SectionCheckerAttribute.Section.AreaRebars;
                if (perCandidate[i].All(r => r.Status == CheckStatus.Satisfied && r.Ratio.HasValue && r.Ratio.Value <= target)
                    && steelArea < area) { best = i; area = steelArea; }
            }
            return (best, perCandidate.SelectMany(r => r).ToArray());
        }
    }
}
