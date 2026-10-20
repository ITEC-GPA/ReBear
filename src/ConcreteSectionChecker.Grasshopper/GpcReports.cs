using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPC.Checkers.Concrete.Shear;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper
{
    internal static class GpcReports
    {
        internal static string Row(params object[] values) => string.Join(";", values.Select(v =>
        {
            string s = v is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : v?.ToString() ?? "";
            return s.IndexOfAny(new[] { ';', '"', '\r', '\n' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }));

        internal static double Number(double? value) => value ?? double.NaN;
        internal static double Resistance(SectionShearResult r, Func<SectionShearResult, double> select)
            => r == null || r.Verdict == ShearVerdict.NotEvaluated ? double.NaN : select(r) / 1000;

        internal static void Shear(GH_Component component, IGH_DataAccess data, params (string label, SectionShearResult result)[] results)
        {
            var header = new List<object>();
            var values = new List<object>();
            foreach (var (label, r) in results)
            {
                header.AddRange(new[] { label + " modello", label + " riferimento", label + " stato", label + " VRsd[kN]",
                    label + " VRcd[kN]", label + " VRd[kN]", label + " VEd[kN]", label + " WR", label + " cot(theta)", label + " dettagli GPC" });
                values.AddRange(new object[] { r?.Model, r?.Reference, r?.Status ?? "Staffe assenti",
                    Resistance(r, x => x.VRsd), Resistance(r, x => x.VRcd), Resistance(r, x => x.VRd),
                    r?.Demand / 1000, r?.Ratio, r?.CotTheta,
                    r == null ? "" : string.Join(" | ", r.Details.Select(d => $"{d.Symbol}={d.Value.ToString("G17", CultureInfo.InvariantCulture)} {d.Unit} ({d.Expression})")) });
                if (r == null || r.Verdict == ShearVerdict.NotEvaluated)
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, label + ": " + (r?.Status ?? "Staffe assenti; uscite relative non applicabili (NaN)."));
                else if (r.Verdict == ShearVerdict.NotSatisfied)
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, label + ": " + r.Status);
            }
            data.SetData(0, Row(header.ToArray()));
            data.SetData(1, Row(values.ToArray()));
        }
    }
}
