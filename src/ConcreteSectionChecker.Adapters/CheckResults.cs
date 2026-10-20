using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using GPC.Model.Results;

namespace ConcreteSectionChecker.Adapters
{
    public enum CheckStatus { Satisfied, NotSatisfied, NotEvaluated, NotRequired }

    /// <summary>Detached result for aggregation and reporting. A missing ratio is never zero.</summary>
    public sealed record CheckResult(
        string Section, string Case, string Check, CheckStatus Status, double? Ratio,
        double? Demand = null, double? Capacity = null, string Unit = "",
        string Message = "", string Reference = "", string Details = "")
    {
        public override string ToString() => $"{Section} / {Case}: {Check} = {Status}, η={Ratio?.ToString("G6", CultureInfo.InvariantCulture) ?? "n/a"}";
        public static CheckResult Pending(string section, string name, string check, Exception error)
            => new(section, name, check, CheckStatus.NotEvaluated, null, Message: error.Message);
    }

    public sealed record CheckEnvelopeResult(string Section, string Check, CheckStatus Status,
        CheckResult Governing, int Count, int Evaluated, int NotEvaluated, int NotRequired);

    public static class CheckResults
    {
        public static CheckStatus Status(bool? passed) => passed.HasValue
            ? passed.Value ? CheckStatus.Satisfied : CheckStatus.NotSatisfied : CheckStatus.NotEvaluated;

        public static IReadOnlyList<CheckEnvelopeResult> Envelope(IEnumerable<CheckResult> results)
        {
            var values = (results ?? throw new ArgumentNullException(nameof(results))).ToArray();
            if (values.Length == 0 || values.Any(v => v == null))
                throw new ArgumentException("Inviluppo: fornire almeno un risultato valido.");
            return values.GroupBy(v => (v.Section, v.Check)).Select(group =>
            {
                // Failed checks without a defined ratio must still govern; incomplete sets cannot pass.
                var failed = group.Where(v => v.Status == CheckStatus.NotSatisfied).ToArray();
                var pending = group.Where(v => v.Status == CheckStatus.NotEvaluated).ToArray();
                var evaluated = group.Where(v => v.Status is CheckStatus.Satisfied or CheckStatus.NotSatisfied).ToArray();
                CheckResult Worst(IEnumerable<CheckResult> rows) => rows
                    .OrderByDescending(v => !v.Ratio.HasValue || double.IsNaN(v.Ratio.Value))
                    .ThenByDescending(v => v.Ratio).FirstOrDefault();
                var status = failed.Length > 0 ? CheckStatus.NotSatisfied : pending.Length > 0 ? CheckStatus.NotEvaluated
                    : evaluated.Length > 0 ? CheckStatus.Satisfied : CheckStatus.NotRequired;
                var governing = failed.Length > 0 ? Worst(failed) : pending.Length > 0 ? pending[0] : Worst(evaluated);
                return new CheckEnvelopeResult(group.Key.Section, group.Key.Check, status, governing,
                    group.Count(), evaluated.Length, pending.Length, group.Count(v => v.Status == CheckStatus.NotRequired));
            }).ToArray();
        }

        public static string Csv(IEnumerable<CheckResult> results)
        {
            string Q(object value) => "\"" + (value is IFormattable f
                ? f.ToString(null, CultureInfo.InvariantCulture) : value?.ToString() ?? "").Replace("\"", "\"\"") + "\"";
            var rows = new List<string> { "Section;Case;Check;Status;Ratio;Demand;Capacity;Unit;Message;Reference;Details" };
            rows.AddRange(results.Select(r => string.Join(";", new object[] { r.Section, r.Case, r.Check,
                r.Status, r.Ratio, r.Demand, r.Capacity, r.Unit, r.Message, r.Reference, r.Details }.Select(Q))));
            return string.Join(Environment.NewLine, rows);
        }

        public static string Json(IEnumerable<CheckResult> results) => JsonSerializer.Serialize(new
        {
            schema = "ReBear.CheckReport", version = 1,
            engine = typeof(GPC.Checkers.Concrete.Shear.SectionShearCalculator).Assembly.GetName().FullName,
            results = results.ToArray()
        }, new JsonSerializerOptions { WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters = { new JsonStringEnumConverter() } });
    }

    public sealed record LoadExtremum(string Component, bool Maximum, int Index, double Value, ResultBeamForces Force);

    public static class LoadEnvelopes
    {
        /// <summary>Component extrema with concomitant full load cases, in the first case's axes and origin.</summary>
        public static IReadOnlyList<LoadExtremum> Calculate(IReadOnlyList<ResultBeamForces> cases)
        {
            if (cases == null || cases.Count == 0 || cases.Any(c => c == null))
                throw new ArgumentException("Inviluppo: almeno una combinazione valida richiesta.");
            foreach (var f in cases) Validate(f);
            var axes = cases[0].CoordinateSystem;
            var loads = cases.Select(c => c.ToCoordinateSystemWithEccentricity(axes)).ToArray();
            foreach (var f in loads) Validate(f);
            var labels = new[] { "N", "Vx", "Vy", "T", "Mx", "My" };
            var result = new List<LoadExtremum>();
            for (int k = 0; k < 6; k++)
                foreach (bool max in new[] { false, true })
                {
                    int best = 0;
                    for (int i = 1; i < loads.Length; i++)
                        if (max ? Values(loads[i])[k] > Values(loads[best])[k] : Values(loads[i])[k] < Values(loads[best])[k]) best = i;
                    result.Add(new LoadExtremum(labels[k], max, best, Values(loads[best])[k], loads[best]));
                }
            return result;
        }
        public static double[] Values(ResultBeamForces f) => new[] { f.N, f.V1, f.V2, f.T, f.M1, f.M2 };
        public static void Validate(ResultBeamForces f)
        {
            if (f?.CoordinateSystem == null || Values(f).Any(v => !double.IsFinite(v)))
                throw new ArgumentException("Azioni e riferimento devono essere definiti e finiti.");
        }
        public static string Name(ResultBeamForces f, int index = 0) => string.IsNullOrWhiteSpace(f.Name) ? $"Caso {index + 1}" : f.Name;
    }
}
