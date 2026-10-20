using System;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Cracking;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Results;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Features
{
    public sealed class ResistanceCheckComponent : ReBearComponent
    {
        public ResistanceCheckComponent() : base("Section Resistance Check", "NM", "Verifica N-Mx-My GPC con esito strutturato e convergenza esplicita.", "2 Checks") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0009");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Checker SLU/SLV."), G("Force", "F", "Azioni N/Nmm."),
            T("Section ID", "ID", "Identificativo per gli inviluppi.", "Section") };
        protected override Port[] Outputs => new[] { G("Result", "R", "Esito N-Mx-My.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var f = Required<ResultBeamForces>(d, 1); var id = Read<string>(d, 2);
            Result(d, SectionChecks.Try(id, LoadEnvelopes.Name(f), "N-Mx-My",
                () => SectionChecks.Resistance(Required<SectionCheckerModelCode2010>(d, 0), f, id)));
        }
    }
    public sealed class SleOptionsComponent : ReBearComponent
    {
        public SleOptionsComponent() : base("SLE Options", "SLEOpt", "Contesto SLE esplicito per limiti tensionali e fessurazione.", "2 Checks") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0010");
        protected override Port[] Inputs => new[] { I("Combination", "C", "0 quasi permanente; 1 frequente; 2 caratteristica.", 0),
            T("Exposure", "E", "Codice della classe ambientale, per esempio XC3."),
            B("Sensitive reinforcement", "Sens", "Sensibilità delle armature nel criterio GPC."),
            B("Short term", "Short", "True breve durata; false lunga durata."), B("Ribbed bars", "Rib", "Barre ad aderenza migliorata.", true),
            N("Longitudinal bar cover", "c", "Copriferro alla superficie delle barre longitudinali (include il diametro staffa), mm."),
            N("Design crack limit", "wlim", "Limite assegnato, mm; scollegato = norma.", optional: true),
            N("Spacing override", "s", "Spaziatura assegnata mm; scollegato = geometria.", optional: true),
            N("Formula cover override", "cf", "Copriferro della formula GPC mm; scollegato = c.", optional: true),
            B("Concentric rings", "Rings", "Conferma disposizione in anelli concentrici per sezioni circolari.") };
        protected override Port[] Outputs => new[] { G("Options", "O", "Opzioni SLE."), List(T("Exposure catalogue", "E", "Codici GPC disponibili.")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            int combo = Read<int>(d, 0); _ = ServiceabilityChecks.Combination(combo);
            string exposure = Read<string>(d, 1)?.Trim().ToUpperInvariant();
            if (!CrackRequirements.ExposureClasses.Contains(exposure)) throw new ArgumentException("Classe ambientale non riconosciuta.");
            double? Optional(int i) { object value = null; return d.GetData(i, ref value) ? Convert.ToDouble(Unwrap(value)) : null; }
            d.SetData(0, new SleOptions(combo, exposure, Read<bool>(d, 2), Read<bool>(d, 3), Read<bool>(d, 4),
                Read<double>(d, 5), Optional(6), Optional(7), Optional(8), Read<bool>(d, 9)));
            d.SetDataList(1, CrackRequirements.ExposureClasses);
        }
    }
    public sealed class ServiceabilityCheckComponent : ReBearComponent
    {
        public ServiceabilityCheckComponent() : base("Serviceability Check v2", "SLE", "Limiti tensionali e fessurazione su analisi GPC, con risultati per gli inviluppi.", "2 Checks") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0011");
        protected override Port[] Inputs => new[] { G("Stress result", "S", "Risultato di Stress Analysis."),
            G("Options", "O", "Da SLE Options."), T("Section ID", "ID", "Identificativo per gli inviluppi.", "Section") };
        protected override Port[] Outputs => new[] { List(G("Results", "R", "Tensioni e fessurazione.")), G("Crack data", "Cr", "Risultato GPC per Effective Cracked Section.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var s = Required<StressAnalysisResult>(d, 0); var o = Required<SleOptions>(d, 1); string id = Read<string>(d, 2);
            var stress = SectionChecks.Try(id, LoadEnvelopes.Name(s.Force), "SLE stress", () => SectionChecks.Stress(s, o, id));
            SectionCrackResult native = null;
            var crack = SectionChecks.Try(id, LoadEnvelopes.Name(s.Force), "SLE cracking",
                () => { var r = SectionChecks.Crack(s, o, id); native = r.Native; return r.Result; });
            d.SetDataList(0, new[] { new GH_CheckResult(stress), new GH_CheckResult(crack) }); d.SetData(1, native);
            foreach (var r in new[] { stress, crack }.Where(r => r.Status == CheckStatus.NotEvaluated))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, r.Message);
        }
    }
    public sealed class BatchSectionChecksComponent : ReBearComponent
    {
        public BatchSectionChecksComponent() : base("Batch Section Checks", "Batch", "Tutte le combinazioni del ramo, esecuzione seriale e inviluppi. Le opzioni scollegate escludono la relativa verifica.", "2 Checks") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0012");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Checker con opzioni coerenti con le verifiche richieste."),
            List(G("Forces", "F", "Combinazioni del ramo.")), T("Section ID", "ID", "Identificativo sezione.", "Section"),
            B("N-Mx-My", "NM", "Eseguire la verifica di resistenza.", true),
            G("Shear X", "X", "Opzioni taglio X.", true), G("Shear Y", "Y", "Opzioni taglio Y.", true),
            G("Torsion", "T", "Opzioni torsione.", true), G("SLE", "SLE", "Opzioni SLE; usare un ramo con combinazioni omogenee.", true),
            B("Run", "Run", "Avvia il calcolo. False disabilita il componente.") };
        protected override Port[] Outputs => new[] { List(G("Results", "R", "Un risultato per caso e verifica, inclusi i non valutati.")),
            T("CSV", "CSV", "Tabella dei risultati."), List(T("Envelope", "Env", "Sintesi per verifica.")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            if (!Read<bool>(d, 8)) return;
            var results = SectionChecks.Batch(Required<SectionCheckerModelCode2010>(d, 0), ReadList<ResultBeamForces>(d, 1),
                Read<string>(d, 2), Read<bool>(d, 3), Read<ShearOptions>(d, 4), Read<ShearOptions>(d, 5), Read<TorsionOptions>(d, 6), Read<SleOptions>(d, 7));
            d.SetDataList(0, results.Select(r => new GH_CheckResult(r))); d.SetData(1, CheckResults.Csv(results));
            d.SetDataList(2, CheckResults.Envelope(results).Select(r => $"{r.Check}: {r.Status}, caso {r.Governing?.Case}, non valutati {r.NotEvaluated}/{r.Count}"));
            if (results.Any(r => r.Status == CheckStatus.NotEvaluated)) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Alcuni casi non sono valutabili: consultare gli esiti.");
        }
    }
}
