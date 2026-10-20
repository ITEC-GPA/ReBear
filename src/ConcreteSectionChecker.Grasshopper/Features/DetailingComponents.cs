using System;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Detailing;
using GPC.Checkers.Concrete.Durability;
using GPC.Model.Materials;
using GPC.Model.Standards;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Features
{
    public sealed class ConcreteCoverComponent : ReBearComponent
    {
        public ConcreteCoverComponent() : base("Concrete Cover", "Cover", "Copriferro nominale GPC per le esposizioni e il diametro effettivo specificati.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0013");
        protected override Port[] Inputs => new[] { G("Standard", "S", "Norma GPC."), G("Concrete", "C", "Calcestruzzo."),
            List(T("Exposures", "E", "Classi ambientali concorrenti, es. XC3, XD1.")),
            N("Actual bar diameter", "Ø", "Diametro effettivo rilevante per il copriferro, mm."),
            N("Adopted cover", "c", "Copriferro nominale disponibile, mm."), I("Design life", "Life", "Anni, secondo il profilo GPC.", 50),
            N("Deviation", "Δc", "Tolleranza esecutiva, mm.", 10), N("Aggregate", "dg", "Aggregato massimo, mm.", 20),
            B("Plate element", "Plate", "Elemento a piastra / geometria di soletta."),
            B("Special quality control", "QC", "Controllo speciale confermato; abilita la riduzione GPC."),
            B("Strength class reduction", "Strength", "Abilita il modificatore per resistenza della classe strutturale EN."),
            B("Rough surface", "Rough", "Superficie irregolare."), I("Abrasion", "Abr", "0 nessuna; 1,2,3 classi GPC.", 0),
            I("Ground", "Soil", "0 cassero; 1 terreno preparato; 2 terreno diretto.", 0),
            N("Pertinent minimum strength", "Cmin", "Resistenza minima pertinente per NTC, MPa; opzionale.", optional: true),
            T("Section ID", "ID", "Identificativo.", "Section") };
        protected override Port[] Outputs => new[] { G("Result", "R", "Confronto richiesto/disponibile."),
            N("Required nominal", "cnom", "Copriferro nominale richiesto mm."), N("Durability minimum", "cmin,dur", "Per Detailing Options, mm."),
            G("Cover data", "D", "Risultato GPC completo.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            object min = null; double? cmin = d.GetData(14, ref min) ? Convert.ToDouble(Unwrap(min)) : null;
            int abrasion = Read<int>(d, 12), ground = Read<int>(d, 13);
            if (abrasion < 0 || abrasion > 3 || ground < 0 || ground > 2)
                throw new ArgumentException("Abrasion: 0–3; Ground: 0–2.");
            var concrete = Required<ConcreteMaterialEuropeanCommon>(d, 1);
            var input = new CoverInput(ReadList<string>(d, 2), Math.Abs(concrete.Fck), Read<int>(d, 5), Read<bool>(d, 10),
                Read<bool>(d, 8), Read<bool>(d, 9), Read<double>(d, 3), Read<double>(d, 7), Read<double>(d, 6),
                Read<bool>(d, 11), abrasion * 5, ground == 0 ? 0 : ground == 1 ? 40 : 75, Read<bool>(d, 8), Read<bool>(d, 9), cmin);
            var r = DetailingChecks.Cover(Required<Standard>(d, 0), input, Read<double>(d, 4), Read<string>(d, 15));
            Result(d, r.Result); d.SetData(1, r.Native.Nominal); d.SetData(2, r.Native.Durability); d.SetData(3, r.Native);
        }
    }
    public sealed class AnchorageLapComponent : ReBearComponent
    {
        public AnchorageLapComponent() : base("Anchorage and Lap", "Anchor", "Ancoraggio rettilineo o sovrapposizione di barre tese ad aderenza migliorata; coefficienti α1…α5 = 1.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0014");
        protected override Port[] Inputs => new[] { G("Standard", "S", "Norma GPC."), G("Concrete", "C", "Calcestruzzo."),
            G("Steel", "S", "Acciaio della barra."), N("Diameter", "Ø", "Diametro mm."), N("Design stress", "σ", "Tensione di trazione σsd MPa, ≤ fyd."),
            N("Available length", "l", "Lunghezza disponibile mm."), B("Good bond", "Bond", "Condizioni di buona aderenza."),
            B("Lap", "Lap", "True sovrapposizione; false ancoraggio."), N("Lapped percentage", "%", "Percentuale barre sovrapposte.", 100),
            N("Lap clear distance", "a", "Interferro fra barre sovrapposte mm.", 0), T("Section ID", "ID", "Identificativo.", "Section") };
        protected override Port[] Outputs => new[] { G("Result", "R", "Esito GPC."), N("Required length", "lreq", "Lunghezza richiesta mm."),
            N("Bond strength", "fbd", "Resistenza di aderenza MPa.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var r = DetailingChecks.Anchorage(Required<StandardModelCode2010>(d, 0), Required<ConcreteMaterialEuropeanCommon>(d, 1),
                Required<SteelMaterial>(d, 2), Read<double>(d, 3), Read<double>(d, 4), Read<double>(d, 5), Read<bool>(d, 6),
                Read<bool>(d, 7), Read<double>(d, 8), Read<double>(d, 9), Read<string>(d, 10));
            Result(d, r.Result); d.SetData(1, r.Native.RequiredLength); d.SetData(2, r.Native.Fbd);
        }
    }
    public sealed class PlateDetailingOptionsComponent : ReBearComponent
    {
        public PlateDetailingOptionsComponent() : base("Plate Detailing Options", "PlateOpt", "Dati delle armature secondarie/orizzontali non ricavabili dalla sezione 2D.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0015");
        protected override Port[] Inputs => new[] { N("Secondary steel", "As2", "Somma delle due facce, mm²/m."),
            N("Secondary spacing", "s2", "Passo mm; 0 dato mancante, esito da completare."), B("Critical region", "Crit", "Zona critica della soletta.") };
        protected override Port[] Outputs => new[] { G("Plate data", "P", "Opzioni piastra/parete.") };
        protected override void SolveGpc(IGH_DataAccess d) => d.SetData(0, new PlateDetailingData(Read<double>(d, 0), Read<double>(d, 1), Read<bool>(d, 2)));
    }
    public sealed class DetailingOptionsComponent : ReBearComponent
    {
        public DetailingOptionsComponent() : base("Detailing Options", "DetOpt", "Ipotesi per le regole di dettaglio GPC. Acciaio unico delle armature; geometria riferita a XY.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0016");
        protected override Port[] Inputs => new[] { I("Member kind", "K", "0 trave, 1 pilastro, 2 soletta, 3 parete.", 0),
            N("Top tension width", "bt+", "Larghezza zona tesa superiore, mm."), N("Bottom tension width", "bt−", "Larghezza zona tesa inferiore, mm."),
            N("Web width", "bw", "Larghezza anima mm."), N("Link diameter", "Øw", "Diametro staffa mm, 0 senza staffe."),
            N("Link spacing", "sw", "Passo staffe mm.", 200), I("Link legs", "nw", "Numero bracci.", 2),
            N("Nominal cover", "c", "Copriferro alle staffe; alle barre se senza staffe, mm."),
            N("Aggregate", "dg", "Aggregato massimo mm.", 20), N("Cover deviation", "Δc", "Tolleranza copriferro mm.", 10),
            N("Durability minimum", "cmin,dur", "Da Concrete Cover; scollegato = controllo da completare.", optional: true),
            B("Lap zone", "Lap", "Zona di sovrapposizione."), B("Compression bars restrained", "Restr", "Confermare il trattenimento delle barre compresse."),
            B("End zones confirmed", "Ends", "Confermare dettagli delle zone di estremità richiesti da GPC."),
            G("Plate data", "P", "Richiesto per soletta/parete.", true) };
        protected override Port[] Outputs => new[] { G("Options", "O", "Opzioni dettagli.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            int kind = Read<int>(d, 0); if (kind < 0 || kind > 3) throw new ArgumentException("Tipo elemento non valido.");
            object cover = null; double? durability = d.GetData(10, ref cover) ? Convert.ToDouble(Unwrap(cover)) : null;
            d.SetData(0, new DetailingOptions((MemberDetailingKind)kind, Read<double>(d, 1), Read<double>(d, 2), Read<double>(d, 3),
                Read<double>(d, 4), Read<double>(d, 5), Read<int>(d, 6), Read<double>(d, 7), Read<double>(d, 8),
                Read<double>(d, 9), durability, Read<bool>(d, 11), Read<bool>(d, 12), Read<bool>(d, 13), Read<PlateDetailingData>(d, 14)));
        }
    }
    public sealed class ReinforcementDetailingComponent : ReBearComponent
    {
        public ReinforcementDetailingComponent() : base("Reinforcement Detailing", "Detail", "Minimi/massimi, interferro, staffe e copriferro secondo il profilo GPC; rende visibili le regole in sospeso.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0017");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Sezione e norma."), G("Options", "O", "Da Detailing Options."),
            N("Minimum axial force", "Nmin", "Massima compressione fra le combinazioni, kN negativa.", 0), T("Section ID", "ID", "Identificativo.", "Section") };
        protected override Port[] Outputs => new[] { List(G("Results", "R", "Un risultato per regola.")), T("CSV", "CSV", "Dettagli e confronti GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var results = DetailingChecks.Member(Required<SectionCheckerModelCode2010>(d, 0), Required<DetailingOptions>(d, 1),
                Read<double>(d, 2) * 1000, Read<string>(d, 3));
            d.SetDataList(0, results.Select(r => new GH_CheckResult(r))); d.SetData(1, CheckResults.Csv(results));
            if (results.Any(r => r.Status == CheckStatus.NotEvaluated)) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Alcune regole richiedono dati o conferme ulteriori: consultare i risultati.");
        }
    }
}
