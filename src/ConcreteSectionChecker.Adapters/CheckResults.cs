using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using GPC.Checker.Results.ResultType;
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

    /// <summary>
    /// The only CSV/JSON writer of ReBear: separator ';', every field quoted and escaped, numbers in the invariant
    /// culture (decimal point, round-trip precision), a missing value is an empty field (JSON null), never zero.
    /// </summary>
    public static class Report
    {
        public const char Separator = ';';
        internal static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters = { new JsonStringEnumConverter() }
        };
        public static string Field(object value)
        {
            string text = value switch
            {
                null => "",
                double d when double.IsNaN(d) => "",
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
        public static string Row(IEnumerable<object> values) => string.Join(Separator, values.Select(Field));
        public static string Csv(IEnumerable<string> header, IEnumerable<IEnumerable<object>> rows)
            => string.Join(Environment.NewLine, new[] { Row(header) }.Concat(rows.Select(Row)));
        public static string Json(string schema, object results) => JsonSerializer.Serialize(new
        {
            schema, version = 1,
            engine = typeof(GPC.Checkers.Concrete.Shear.SectionShearCalculator).Assembly.GetName().FullName,
            results
        }, JsonOptions);
        /// <summary>Finite value or null: GPC marks absent extrema with ±double.MaxValue (±∞ after the ψ scaling of linear analyses).</summary>
        public static double? Finite(double value) => double.IsFinite(value) && Math.Abs(value) < 1e300 ? value : null;
    }

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

        public static IEnumerable<string> Summary(IEnumerable<CheckResult> results) => Envelope(results).Select(r =>
            $"{r.Section} / {r.Check}: {r.Status}; caso={r.Governing?.Case}; valutati={r.Evaluated}/{r.Count}; non valutati={r.NotEvaluated}");

        public static string Csv(IEnumerable<CheckResult> results) => Report.Csv(
            new[] { "Section", "Case", "Check", "Status", "Ratio", "Demand", "Capacity", "Unit", "Message", "Reference", "Details" },
            results.Select(r => new object[] { r.Section, r.Case, r.Check, r.Status, r.Ratio, r.Demand, r.Capacity, r.Unit, r.Message, r.Reference, r.Details }));

        public static string Json(IEnumerable<CheckResult> results) => Report.Json("ReBear.CheckReport", results.ToArray());
    }

    /// <summary>
    /// Extremes of one stress analysis (GPC strain-plane summary). Forces kN/kNm, stresses MPa, strains ‰, lengths mm,
    /// angle degrees. A quantity without the relevant material (for example tendons) is null, never zero.
    /// </summary>
    public sealed record StressSummary(int Id, string Case, double? N, double? Mx, double? My,
        double? SigmaCMin, double? SigmaCMax, double? SigmaSMin, double? SigmaSMax, double? SigmaPMin, double? SigmaPMax,
        double? SigmaSSMin, double? SigmaSSMax, double? EpsilonCMin, double? EpsilonCMax, double? EpsilonSMin, double? EpsilonSMax,
        double? EpsilonPMin, double? EpsilonPMax, double? EpsilonSSMin, double? EpsilonSSMax,
        double? NetHeight, double? NeutralAxisDistance, double? NeutralAxisAngle)
    {
        public static readonly string[] Header = { "Id", "Case", "N [kN]", "Mx [kNm]", "My [kNm]",
            "σc min [MPa]", "σc max [MPa]", "σs min [MPa]", "σs max [MPa]", "σp min [MPa]", "σp max [MPa]", "σss min [MPa]", "σss max [MPa]",
            "εc min [‰]", "εc max [‰]", "εs min [‰]", "εs max [‰]", "εp min [‰]", "εp max [‰]", "εss min [‰]", "εss max [‰]",
            "d [mm]", "x [mm]", "θ [°]" };
        /// <summary>Numeric values in <see cref="Header"/> order after Id and Case.</summary>
        public double?[] Values => new[] { N, Mx, My, SigmaCMin, SigmaCMax, SigmaSMin, SigmaSMax, SigmaPMin, SigmaPMax, SigmaSSMin, SigmaSSMax,
            EpsilonCMin, EpsilonCMax, EpsilonSMin, EpsilonSMax, EpsilonPMin, EpsilonPMax, EpsilonSSMin, EpsilonSSMax, NetHeight, NeutralAxisDistance, NeutralAxisAngle };
        public override string ToString() => FormattableString.Invariant(
            $"{Case}: N={N:G6} kN, Mx={Mx:G6} kNm, My={My:G6} kNm, σc,min={SigmaCMin:G6} MPa, σs,max={SigmaSMax:G6} MPa, x={NeutralAxisDistance:G6} mm");

        public static StressSummary From(StrainPlaneResult r, int index = 0)
        {
            if (r?.ResultBeamForce == null) throw new ArgumentException("Risultato del piano di deformazione non valido.");
            var f = r.ResultBeamForce;
            double? Strain(double v) => Report.Finite(v) * 1000;
            return new(f.Id, LoadEnvelopes.Name(f, index), f.N / 1000, f.M1 / 1e6, f.M2 / 1e6,
                Report.Finite(r.SigmaCMin), Report.Finite(r.SigmaCMax), Report.Finite(r.SigmaSMin), Report.Finite(r.SigmaSMax),
                Report.Finite(r.SigmaPMin), Report.Finite(r.SigmaPMax), Report.Finite(r.SigmaSSMin), Report.Finite(r.SigmaSSMax),
                Strain(r.EpsilonCMin), Strain(r.EpsilonCMax), Strain(r.EpsilonSMin), Strain(r.EpsilonSMax),
                Strain(r.EpsilonPMin), Strain(r.EpsilonPMax), Strain(r.EpsilonSSMin), Strain(r.EpsilonSSMax),
                Report.Finite(r.NetHeight), Report.Finite(r.NeutralAxisDistance), Report.Finite(r.NeutralAxisAngle));
        }
        public static string Csv(IEnumerable<StressSummary> rows) => Report.Csv(Header,
            rows.Select(r => new object[] { r.Id, r.Case }.Concat(r.Values.Cast<object>())));
        public static string Json(IEnumerable<StressSummary> rows) => Report.Json("ReBear.StressReport", rows.ToArray());
        public static IEnumerable<string> Summary(IEnumerable<StressSummary> rows)
        {
            var values = rows.ToArray();
            string Extreme(string label, Func<StressSummary, double?> select, bool minimum)
            {
                var found = values.Where(v => select(v).HasValue).ToArray();
                if (found.Length == 0) return $"{label}: non disponibile";
                var row = minimum ? found.OrderBy(v => select(v)).First() : found.OrderByDescending(v => select(v)).First();
                return FormattableString.Invariant($"{label} = {select(row):G6} (caso {row.Case})");
            }
            return new[] { Extreme("σc,min [MPa]", v => v.SigmaCMin, true), Extreme("σs,max [MPa]", v => v.SigmaSMax, false),
                Extreme("σs,min [MPa]", v => v.SigmaSMin, true), Extreme("σp,max [MPa]", v => v.SigmaPMax, false) };
        }
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
        /// <summary>The combination name; an unnamed one becomes "Caso k", k = 1-based position in its list or component iteration.</summary>
        public static string Name(ResultBeamForces f, int index = 0) => string.IsNullOrWhiteSpace(f?.Name) ? $"Caso {index + 1}" : f.Name;
    }
}
