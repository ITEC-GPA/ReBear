using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Cracking;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Results;

namespace ConcreteSectionChecker.Adapters
{
    public sealed record SleOptions(int Combination, string Exposure, bool Sensitive, bool ShortTerm,
        bool Ribbed, double BarCover, double? DesignLimit = null, double? Spacing = null,
        double? FormulaCover = null, bool ConcentricRings = false);

    public static class SectionChecks
    {
        public static CheckResult Resistance(SectionCheckerModelCode2010 checker, ResultBeamForces force, string section, int index = 0,
            CancellationToken token = default)
        {
            LoadEnvelopes.Validate(force);
            return Resistance(ResistanceChecks.Evaluate(checker, force, token), checker, force, section, index);
        }

        /// <summary>Structured result of an already solved resistance point.</summary>
        public static CheckResult Resistance(SectionResistanceResponse r, SectionCheckerModelCode2010 checker, ResultBeamForces force, string section, int index = 0)
        {
            var status = r.IsAxialForceOutsideDomain ? CheckStatus.NotSatisfied : !r.Utilization.HasValue
                ? CheckStatus.NotEvaluated : r.Utilization.Value <= 1 ? CheckStatus.Satisfied : CheckStatus.NotSatisfied;
            return new(section, LoadEnvelopes.Name(force, index), "N-Mx-My", status, r.Utilization,
                Message: r.Diagnostics.Message, Reference: checker.StandardModelCode2010.Name,
                Details: FormattableString.Invariant($"Criterion={r.Criterion}; Mode={r.FailureMode}; NRd={r.N / 1000} kN; MxRd={r.M1 / 1e6} kNm; MyRd={r.M2 / 1e6} kNm"));
        }

        public static CheckResult Stress(StressAnalysisResult stress, SleOptions options, string section, int index = 0)
        {
            var r = ServiceabilityChecks.Stress(stress, options.Combination);
            return new(section, LoadEnvelopes.Name(stress.Force, index), "SLE stress", r.Satisfied.HasValue ? CheckResults.Status(r.Satisfied) : CheckStatus.NotRequired,
                r.Ratio, Message: r.Satisfied.HasValue ? r.Combination.ToString() : "Nessun limite tensionale per questa combinazione.",
                Reference: stress.Standard.Name, Details: FormattableString.Invariant(
                    $"σc,min={r.ConcreteMinStress} MPa; σs,max={r.SteelMaxStress} MPa; ηc={r.ConcreteRatio}; ηs={r.SteelRatio}; ψs={r.PsiRebar}"));
        }

        public static (CheckResult Result, SectionCrackResult Native) Crack(StressAnalysisResult stress, SleOptions options, string section,
            int index = 0, UncrackedCheckers cache = null)
        {
            int exposure = Array.FindIndex(CrackRequirements.ExposureClasses.ToArray(), e => e.Equals(options.Exposure, StringComparison.OrdinalIgnoreCase));
            if (exposure < 0) throw new ArgumentException("Esposizione non riconosciuta: " + options.Exposure);
            var r = ServiceabilityChecks.Cracking(stress, options.BarCover, options.Combination, exposure,
                options.Sensitive ? 0 : 1, options.Ribbed ? 0 : 1, options.ShortTerm ? 0 : 1,
                options.DesignLimit, options.Spacing, options.FormulaCover, options.ConcentricRings, cache);
            var status = r.Verdict == CrackVerdict.NotRequired ? CheckStatus.NotRequired : CheckResults.Status(r.Passed);
            bool width = r.Width.HasValue;
            return (new(section, LoadEnvelopes.Name(stress.Force, index), "SLE cracking", status, r.Ratio,
                width ? r.Width : r.UncrackedMaximumStress, width ? r.Limit : r.StressLimit, width ? "mm" : "MPa",
                $"{r.Requirement?.Criterion}: {r.Status}", r.Reference, AdvancedShearChecks.Details(r.Details)), r);
        }

        public static CheckResult Try(string section, string name, string check, Func<CheckResult> operation)
        {
            try { return operation(); }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or ArithmeticException)
            { return CheckResult.Pending(section, name, check, e); }
        }

        /// <summary>
        /// Serial execution on one checker: the legacy GPC checker is mutable and is never shared between workers.
        /// The uncracked analyses of the decompression criterion share one auxiliary checker per section and axes.
        /// </summary>
        public static IReadOnlyList<CheckResult> Batch(SectionCheckerModelCode2010 checker, IReadOnlyList<ResultBeamForces> forces,
            string section, bool resistance, ShearOptions x = null, ShearOptions y = null, TorsionOptions torsion = null, SleOptions sle = null,
            CancellationToken token = default)
        {
            if (checker is null || forces is null || forces.Count == 0) throw new ArgumentException("Checker e combinazioni richiesti.");
            if (!resistance && x is null && y is null && torsion is null && sle is null) throw new ArgumentException("Selezionare almeno una verifica.");
            var results = new List<CheckResult>();
            var cache = sle is null ? null : new UncrackedCheckers();
            for (int i = 0; i < forces.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var f = forces[i] ?? throw new ArgumentException("Combinazione vuota.");
                string name = LoadEnvelopes.Name(f, i); int index = i;
                void Add(string check, Func<CheckResult> operation) => results.Add(Try(section, name, check, operation) with { Case = name });
                if (resistance) Add("N-Mx-My", () => Resistance(checker, f, section, index, token));
                if (x is not null) Add("Shear X", () => AdvancedShearChecks.Shear(checker, f, x, ShearDirections.X, section, index));
                if (y is not null) Add("Shear Y", () => AdvancedShearChecks.Shear(checker, f, y, ShearDirections.Y, section, index));
                if (torsion is not null) Add("Torsion + shear", () => AdvancedShearChecks.Torsion(checker, f, torsion, x, y, section, index));
                if (sle is not null)
                {
                    StressAnalysisResult stress = null;
                    // Both records must survive a failed analysis.
                    Add("SLE stress", () => { LoadEnvelopes.Validate(f); stress = checker.GetTensionAnalysisResult(f); return Stress(stress, sle, section, index); });
                    Add("SLE cracking", () => Crack(stress ?? throw new InvalidOperationException("Analisi tensionale non disponibile."), sle, section, index, cache).Result);
                }
            }
            return results;
        }
    }
}
