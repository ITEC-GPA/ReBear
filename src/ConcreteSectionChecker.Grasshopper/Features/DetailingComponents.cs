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
    /// <summary>
    /// 0.3: the specification and the actual bar diameter feed the two requirements of the GPC cover assessment, and every
    /// GPC modifier has its own port (in 0.2 one port set both the EN and the NTC flag of the same idea).
    /// </summary>
    public sealed class ConcreteCoverComponent : ReBearComponent
    {
        public ConcreteCoverComponent() : base("Concrete Cover", "Cover",
            "Copriferro nominale GPC: requisito della specifica (diametro di riferimento) e delle barre reali; esito non valutato senza barre reali.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000004");
        protected override Port[] Inputs => new[] { G("Standard", "S", "Norma GPC."), G("Concrete", "C", "Calcestruzzo."),
            List(T("Exposures", "E", "Classi ambientali concorrenti, es. XC3, XD1.")),
            N("Specification bar diameter", "Øspec", "Diametro della specifica dei materiali (requisito di aderenza di riferimento), mm.", 20),
            N("Actual bar diameter", "Ø", "Diametro effettivo delle barre, mm; scollegato = barre reali non note (esito da completare).", optional: true),
            N("Adopted cover", "c", "Copriferro nominale disponibile, mm."), I("Design life", "Life", "Anni, secondo il profilo GPC.", 50),
            N("Deviation", "Δc", "Tolleranza esecutiva Δcdev, mm.", 10), N("Aggregate", "dg", "Aggregato massimo, mm.", 20),
            B("Slab geometry (EN)", "Slab", "EN: geometria a soletta, riduzione di una classe strutturale."),
            B("Special quality control (EN)", "QC", "EN: controllo di qualità speciale, riduzione di una classe strutturale."),
            B("Strength class reduction (EN)", "Strength", "EN: riduzione di classe strutturale per la resistenza del calcestruzzo."),
            B("Plate element (NTC)", "Plate", "NTC: elemento a piastra (valori tabellari ridotti)."),
            B("Quality control reduction (NTC)", "QC NTC", "NTC: riduzione per produzione con controllo di qualità."),
            B("Rough surface", "Rough", "Superficie irregolare."), I("Abrasion", "Abr", "0 nessuna; 1,2,3 classi GPC.", 0, Named("None", "XM1", "XM2", "XM3")),
            I("Ground", "Soil", "0 cassero; 1 terreno preparato; 2 terreno diretto.", 0, Named("Formwork", "Prepared ground", "Direct ground")),
            N("Pertinent minimum strength", "Cmin", "Resistenza minima pertinente per NTC, MPa.", optional: true),
            T("Section ID", "ID", "Identificativo.", "Section") };
        protected override Port[] Outputs => new[] { G("Result", "R", "Confronto richiesto/disponibile."),
            N("Required nominal", "cnom", "Copriferro nominale richiesto (massimo fra specifica e barre reali), mm."),
            N("Durability minimum", "cmin,dur", "Per Detailing Options, mm."), G("Cover data", "D", "Risultato GPC completo (barre reali se note).") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            int abrasion = Choice(d, 15, 4), ground = Choice(d, 16, 3);
            var concrete = Required<ConcreteMaterialEuropeanCommon>(d, 1);
            var specification = new CoverInput(ReadList<string>(d, 2), Math.Abs(concrete.Fck), Required<int>(d, 6), Read(d, 11, false),
                Read(d, 9, false), Read(d, 10, false), Finite(d, 3, positive: true), Finite(d, 8, positive: true), Finite(d, 7, nonNegative: true),
                Read(d, 14, false), abrasion * 5, ground == 0 ? 0 : ground == 1 ? 40 : 75, Read(d, 12, false), Read(d, 13, false), ReadOptional(d, 17));
            var r = DetailingChecks.Cover(Required<Standard>(d, 0), specification, Finite(d, 5, nonNegative: true), ReadOptional(d, 4), Read(d, 18, "Section"));
            Result(d, r.Result); d.SetData(1, r.Assessment.Required); d.SetData(2, r.Native.Durability); d.SetData(3, Data(r.Native));
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
                Required<SteelMaterial>(d, 2), Finite(d, 3), Finite(d, 4), Finite(d, 5), Read(d, 6, false),
                Read(d, 7, false), Finite(d, 8), Finite(d, 9), Read(d, 10, "Section"));
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
        protected override void SolveGpc(IGH_DataAccess d) =>
            d.SetData(0, Data(new PlateDetailingData(Finite(d, 0, nonNegative: true), Finite(d, 1, nonNegative: true), Read(d, 2, false))));
    }
    public sealed class DetailingOptionsComponent : ReBearComponent
    {
        public DetailingOptionsComponent() : base("Detailing Options", "DetOpt", "Ipotesi per le regole di dettaglio GPC. Acciaio unico delle armature; geometria riferita a XY.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0016");
        protected override Port[] Inputs => new[] { I("Member kind", "K", "0 trave, 1 pilastro, 2 soletta, 3 parete.", 0, Named("Beam", "Column", "Slab", "Wall")),
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
            d.SetData(0, Data(new DetailingOptions((MemberDetailingKind)Choice(d, 0, 4), Finite(d, 1), Finite(d, 2), Finite(d, 3),
                Finite(d, 4), Finite(d, 5), Required<int>(d, 6), Finite(d, 7), Finite(d, 8),
                Finite(d, 9), ReadOptional(d, 10), Read(d, 11, false), Read(d, 12, false), Read(d, 13, false), Read<PlateDetailingData>(d, 14))));
        }
    }
    public sealed class ReinforcementDetailingComponent : ReBearComponent
    {
        public ReinforcementDetailingComponent() : base("Reinforcement Detailing", "Detail", "Minimi/massimi, interferro, staffe e copriferro secondo il profilo GPC; rende visibili le regole in sospeso.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0017");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Sezione e norma."), G("Options", "O", "Da Detailing Options."),
            N("Minimum axial force", "Nmin", "Massima compressione fra le combinazioni, kN negativa.", 0), T("Section ID", "ID", "Identificativo.", "Section") };
        protected override Port[] Outputs => new[] { List(G("Results", "R", "Un risultato per regola.")), T("CSV", "CSV", "Dettagli e confronti GPC (stesso formato di Check Report).") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var results = DetailingChecks.Member(Required<SectionCheckerModelCode2010>(d, 0), Required<DetailingOptions>(d, 1),
                Finite(d, 2) * 1000, Read(d, 3, "Section"));
            d.SetDataList(0, results.Select(r => new GH_CheckResult(r))); d.SetData(1, CheckResults.Csv(results));
            if (results.Any(r => r.Status == CheckStatus.NotEvaluated)) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Alcune regole richiedono dati o conferme ulteriori: consultare i risultati.");
        }
    }
}
