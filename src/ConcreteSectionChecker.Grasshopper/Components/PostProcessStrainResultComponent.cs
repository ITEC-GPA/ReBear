using System;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using GPC.Checker.Results.ResultType;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    /// <summary>
    /// 0.3 replacement of "Print Stress Analysis Results": no CSV strings (Check Report writes the table), no rounding,
    /// NaN where a quantity does not exist (GPC ±double.MaxValue sentinels, for example σp without tendons).
    /// </summary>
    public sealed class PostProcessStrainResultComponent : ReBearComponent
    {
        public PostProcessStrainResultComponent() : base("Stress Analysis Summary", "SAS",
            "Estremi tensionali e deformativi di un'analisi; NaN se la grandezza non esiste. Collegare Summary a Check Report per CSV/JSON.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000005");
        protected override Port[] Inputs => new[] { G("Strain Plane Result", "SPR", "Uscita Strain Plane Results di Stress Analysis.") };
        protected override Port[] Outputs => new[] { G("Summary", "S", "Riepilogo per Check Report."), I("Id", "Id", "Posizione della combinazione in Stress Analysis."),
            T("Case", "Case", "Nome della combinazione.") }
            .Concat(StressSummary.Header.Skip(2).Select(h => N(h, h.Split(' ')[0] + (h.Contains(" max") ? "+" : h.Contains(" min") ? "-" : ""), Describe(h))))
            .ToArray();
        private static string Describe(string header) => header switch
        {
            _ when header.StartsWith("σ") => "Tensione, MPa (compressione negativa); NaN se assente.",
            _ when header.StartsWith("ε") => "Deformazione, ‰ (accorciamento negativo); NaN se assente.",
            "d [mm]" => "Altezza utile nella direzione dell'asse neutro, mm.",
            "x [mm]" => "Profondità dell'asse neutro, mm; NaN se non definita.",
            "θ [°]" => "Inclinazione dell'asse neutro, gradi.",
            _ => "Azione della combinazione."
        };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var summary = StressSummary.From(Required<StrainPlaneResult>(d, 0), d.Iteration);
            d.SetData(0, Data(summary)); d.SetData(1, summary.Id); d.SetData(2, summary.Case);
            var values = summary.Values;
            for (int i = 0; i < values.Length; i++) d.SetData(i + 3, Number(values[i]));
        }
    }
}
