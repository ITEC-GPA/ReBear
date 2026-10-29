using System;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Results;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Features
{
    public sealed class LoadEnvelopeComponent : ReBearComponent
    {
        public LoadEnvelopeComponent() : base("Load Envelope", "LoadEnv",
            "Minimi/massimi per ramo, con combinazioni concomitanti. Trasporta tutte le azioni negli assi e origine del primo caso.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0001");
        protected override Port[] Inputs => new[] { List(G("Forces", "F", "Combinazioni nello stesso ramo; N/Nmm.")) };
        protected override Port[] Outputs => new[] {
            List(T("Extremum", "E", "N min/max, Vx min/max, Vy min/max, T min/max, Mx min/max, My min/max.")),
            List(N("Value", "V", "Forze kN; momenti kNm.")), List(I("Case index", "i", "Indice originale, base zero.")),
            List(G("Concomitant forces", "F", "Tutta la combinazione governante; non un vettore sintetico di massimi.")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var results = LoadEnvelopes.Calculate(ReadList<ResultBeamForces>(d, 0));
            d.SetDataList(0, results.Select(r => r.Component + (r.Maximum ? " max" : " min")));
            d.SetDataList(1, results.Select(r => r.Value / (r.Component is "N" or "Vx" or "Vy" ? 1000 : 1e6)));
            d.SetDataList(2, results.Select(r => r.Index));
            d.SetDataList(3, results.Select(r => new GH_ResultBeamForces(r.Force)));
        }
    }
    public sealed class CheckEnvelopeComponent : ReBearComponent
    {
        public CheckEnvelopeComponent() : base("Check Envelope", "CheckEnv",
            "Inviluppo per sezione e verifica nel ramo. I risultati non valutati impediscono un esito positivo completo.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0002");
        protected override Port[] Inputs => new[] { List(G("Results", "R", "Risultati strutturati ReBear.")) };
        protected override Port[] Outputs => new[] { List(T("Group", "G", "Sezione / verifica.")), List(T("Status", "S", "Stato complessivo.")),
            List(G("Governing", "R", "Caso governante, anche quando non valutabile; null se tutti non richiesti.")),
            List(I("Pending", "P", "Numero di casi non valutati.")), List(I("Count", "N", "Numero totale dei casi.")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var results = CheckResults.Envelope(ReadList<CheckResult>(d, 0));
            d.SetDataList(0, results.Select(r => r.Section + " / " + r.Check)); d.SetDataList(1, results.Select(r => r.Status.ToString()));
            d.SetDataList(2, results.Select(r => new GH_CheckResult(r.Governing)));
            d.SetDataList(3, results.Select(r => r.NotEvaluated)); d.SetDataList(4, results.Select(r => r.Count));
        }
    }
    public sealed class CheckResultComponent : ReBearComponent
    {
        public CheckResultComponent() : base("Check Result", "Result", "Legge un risultato strutturato senza sostituire i dati mancanti con zero.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0003");
        protected override Port[] Inputs => new[] { G("Result", "R", "Risultato ReBear.") };
        protected override Port[] Outputs => new[] { T("Section", "S", "Identificativo sezione."), T("Case", "C", "Combinazione."),
            T("Check", "K", "Verifica."), T("Status", "St", "Satisfied, NotSatisfied, NotEvaluated, NotRequired."),
            N("Utilization", "η", "NaN se non definito."), N("Demand", "Ed", "Domanda; NaN se non applicabile."),
            N("Capacity", "Rd", "Capacità/limite; NaN se non applicabile."), T("Unit", "U", "Unità di domanda e capacità."),
            T("Message", "M", "Stato e limitazioni."), T("Reference", "Ref", "Riferimento restituito da GPC."), T("Details", "D", "Dettagli GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var r = Required<CheckResult>(d, 0);
            object[] values = { r.Section, r.Case, r.Check, r.Status.ToString(), r.Ratio ?? double.NaN,
                r.Demand ?? double.NaN, r.Capacity ?? double.NaN, r.Unit, r.Message, r.Reference, r.Details };
            for (int i = 0; i < values.Length; i++) d.SetData(i, values[i]);
        }
    }
    public sealed class CheckReportComponent : ReBearComponent
    {
        public CheckReportComponent() : base("Check Report", "Report",
            "Unico report di ReBear: tabella CSV e JSON versionato di esiti (Check Result) oppure di riepiloghi tensionali (Stress Analysis Summary); nessuna scrittura su disco.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0004");
        protected override Port[] Inputs => new[] { List(G("Results", "R", "Esiti oppure riepiloghi tensionali dello stesso ramo (un solo tipo per report).")) };
        protected override Port[] Outputs => new[] { T("CSV", "CSV", "Separatore ;, numeri invariant, campi con escaping, valore mancante = campo vuoto."),
            T("JSON", "JSON", "Schema ReBear.CheckReport o ReBear.StressReport v1 e versione motore."), List(T("Summary", "S", "Sintesi degli inviluppi o degli estremi.")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var values = ReadList<object>(d, 0);
            if (values.Count > 0 && values.All(v => v is StressSummary))
            {
                var rows = values.Cast<StressSummary>().ToArray();
                d.SetData(0, StressSummary.Csv(rows)); d.SetData(1, StressSummary.Json(rows)); d.SetDataList(2, StressSummary.Summary(rows));
                return;
            }
            if (values.Any(v => v is not CheckResult))
                throw new ArgumentException($"{Label(0)}: collegare soli esiti (Check Result) oppure soli riepiloghi tensionali, in report separati.");
            var results = values.Cast<CheckResult>().ToArray();
            d.SetData(0, CheckResults.Csv(results)); d.SetData(1, CheckResults.Json(results)); d.SetDataList(2, CheckResults.Summary(results));
        }
    }
}
