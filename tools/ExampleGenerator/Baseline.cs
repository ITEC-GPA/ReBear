using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

/// <summary>
/// Comparison of the regenerated results with the results published in 0.2.2 (tools/ExampleGenerator/baseline-0.2.2):
/// the README of every example states what changed and why, and the summary feeds docs/MIGRATION.md.
/// </summary>
internal static class Baseline
{
    internal const double Relative = 1e-9, Absolute = 1e-10;
    internal static readonly List<(string Example, string Quantity, string Old, string New, double Delta)> All = new();

    private static string F(double v) => v.ToString("G9", CultureInfo.InvariantCulture);
    private static bool Same(double a, double b) => Math.Abs(a - b) <= Math.Max(Absolute, Relative * Math.Max(Math.Abs(a), Math.Abs(b)));
    /// <summary>Relative difference; with a zero value the absolute difference (marked negative for the display).</summary>
    private static double Delta(double a, double b) => a == b ? 0 : a == 0 || b == 0 ? -Math.Abs(a - b) : Math.Abs(a - b) / Math.Max(Math.Abs(a), Math.Abs(b));
    internal static string Show(double delta) => double.IsNaN(delta) ? "—" : delta < 0 ? "ass. " + (-delta).ToString("0.0E+0", CultureInfo.InvariantCulture) : delta.ToString("0.0E+0", CultureInfo.InvariantCulture);

    internal static string Differences(string slug, string json, string changes, IReadOnlyList<Grasshopper.Kernel.GH_Component> nodes)
    {
        using var palette = JsonDocument.Parse(File.ReadAllText(Path.Combine(Program.Root, "tests/Compatibility/palette-0.2.2.json")));
        var kept = palette.RootElement.GetProperty("components").EnumerateArray().Select(c => c.GetProperty("guid").GetGuid()).ToHashSet();
        var renewed = nodes.Where(n => !kept.Contains(n.ComponentGuid)).Select(n => n.Name).Distinct().OrderBy(n => n).ToArray();
        string path = Path.Combine(Program.Root, "tools/ExampleGenerator/baseline-0.2.2", slug + ".json");
        var text = new StringBuilder("\n## Differenze rispetto a ReBear 0.2.2\n\n");
        text.AppendLine(renewed.Length == 0 ? "Tutti i componenti del grafo conservano il `ComponentGuid` di 0.2.2.\n"
            : $"Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): {string.Join(", ", renewed.Select(n => "**" + n + "**"))}. Gli altri componenti del grafo conservano il GUID di 0.2.2.\n");
        if (!string.IsNullOrWhiteSpace(changes)) text.AppendLine(changes.Trim() + "\n");
        if (!File.Exists(path)) return text.AppendLine("Esempio nuovo: nessun risultato 0.2.2 da confrontare.").ToString();
        using var old = JsonDocument.Parse(File.ReadAllText(path));
        using var now = JsonDocument.Parse(json);
        var rows = new List<(string Quantity, string Old, string New, double Delta)>();
        void Number(string quantity, JsonElement a, JsonElement b)
        {
            if (a.ValueKind == JsonValueKind.Number && b.ValueKind == JsonValueKind.Number)
            {
                double x = a.GetDouble(), y = b.GetDouble();
                if (!Same(x, y)) rows.Add((quantity, F(x), F(y), Delta(x, y)));
            }
            else if (a.ValueKind != b.ValueKind) rows.Add((quantity, Show(a), Show(b), double.NaN));
        }
        var oldValues = old.RootElement.GetProperty("values").EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
        foreach (var p in now.RootElement.GetProperty("values").EnumerateObject())
            if (oldValues.TryGetValue(p.Name, out var o)) Number(p.Name, o, p.Value);
            else rows.Add((p.Name, "—", Show(p.Value), double.NaN));
        string Key(JsonElement c) => $"{c.GetProperty("Case").GetString()} / {c.GetProperty("Check").GetString()}";
        var oldChecks = old.RootElement.GetProperty("checks").EnumerateArray().GroupBy(Key).ToDictionary(g => g.Key, g => g.ToArray());
        foreach (var group in now.RootElement.GetProperty("checks").EnumerateArray().GroupBy(Key))
        {
            var current = group.ToArray();
            if (!oldChecks.TryGetValue(group.Key, out var previous) || previous.Length != current.Length)
            { rows.Add((group.Key, previous == null ? "—" : previous.Length + " esiti", current.Length + " esiti", double.NaN)); continue; }
            for (int i = 0; i < current.Length; i++)
            {
                string label = group.Key + (current.Length > 1 ? $" [{i + 1}]" : "");
                string a = previous[i].GetProperty("Status").GetString(), b = current[i].GetProperty("Status").GetString();
                if (a != b) rows.Add((label + " esito", a, b, double.NaN));
                foreach (var field in new[] { "Ratio", "Demand", "Capacity" })
                    Number(label + " " + field switch { "Ratio" => "η", "Demand" => "domanda", _ => "capacità" }, previous[i].GetProperty(field), current[i].GetProperty(field));
            }
        }
        foreach (var removed in oldChecks.Keys.Except(now.RootElement.GetProperty("checks").EnumerateArray().Select(Key)))
            rows.Add((removed, "presente", "assente", double.NaN));
        var a1 = Leaves(old.RootElement.GetProperty("series")).ToArray(); var b1 = Leaves(now.RootElement.GetProperty("series")).ToArray();
        if (a1.Length != b1.Length) rows.Add(("Serie numerica", a1.Length + " valori", b1.Length + " valori", double.NaN));
        else
        {
            // Differences normalized by the largest magnitude of the series: values near zero do not inflate the ratio.
            double scale = Math.Max(Absolute, a1.Concat(b1).Select(Math.Abs).DefaultIfEmpty(0).Max());
            var deltas = a1.Zip(b1, (x, y) => Same(x, y) ? 0 : Math.Abs(x - y) / scale).ToArray();
            if (deltas.Any(d => d > 0)) rows.Add(("Serie numerica (Δ massimo / valore massimo della serie)", $"{a1.Length} valori", $"{deltas.Count(d => d > 0)} diversi", deltas.Max()));
        }
        text.AppendLine($"Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/{slug}.json`); soglia {Relative:G1} relativo, {Absolute:G1} assoluto.\n");
        if (rows.Count == 0) return text.AppendLine("Nessuna variazione numerica oltre la soglia e nessun esito diverso.").ToString();
        text.AppendLine("| Grandezza | 0.2.2 | 0.3 | Δ relativo (ass. se un valore è nullo) |\n|---|---:|---:|---:|");
        foreach (var r in rows)
        {
            text.AppendLine($"| {r.Quantity} | {r.Old} | {r.New} | {Show(r.Delta)} |");
            All.Add((slug, r.Quantity, r.Old, r.New, r.Delta));
        }
        return text.ToString();
    }

    private static string Show(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Number => F(e.GetDouble()), JsonValueKind.Null => "—", JsonValueKind.Undefined => "—", _ => e.ToString()
    };

    private static IEnumerable<double> Leaves(JsonElement e)
    {
        switch (e.ValueKind)
        {
            case JsonValueKind.Number: yield return e.GetDouble(); break;
            case JsonValueKind.Array: foreach (var x in e.EnumerateArray()) foreach (var v in Leaves(x)) yield return v; break;
            case JsonValueKind.Object: foreach (var p in e.EnumerateObject()) foreach (var v in Leaves(p.Value)) yield return v; break;
        }
    }
}
