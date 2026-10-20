using System;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Detailing;
using GPC.Checkers.Concrete.Durability;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Standards;

internal static partial class Program
{
    private static void ResistanceExample()
    {
        using var e=new Example("10-resistenza-inviluppo","Resistenza N-Mx-My e inviluppo degli esiti",
            "Rettangolo 300 × 500, 8Ø20, C30/37 e B450C. Dominio plastico GPC, criterio a N costante e strategia Intersection. Tre combinazioni includono flessione deviata, domanda elevata e sforzo normale esterno al dominio.",
            "Le resistenze si cercano lungo la direzione dei momenti richiesti mantenendo N. Il primo caso N=−300 kN, Mx=80 e My=10 kNm è moderato; il secondo Mx=400 kNm mostra un possibile superamento; il terzo N=−10000000 kN è intenzionalmente fuori campo resistente. Quest'ultimo deve risultare NotSatisfied anche senza un rapporto numerico disponibile. L'inviluppo non può eliminare il caso esterno perché η è null.");
        var f=new Fixture(e);f.Prepare();
        var cases=new[]{f.Force("SLU-moderato",-300,0,0,0,80,10),f.Force("SLU-elevato",-300,0,0,0,400,0),f.Force("Fuori-dominio",-1e7,0,0,0,80,0)};
        var batch=e.Graph.Add("BatchSectionChecksComponent");e.Graph.Wire(f.Checker,batch,0);foreach(var c in cases)e.Graph.Wire(c.Node,batch,1);
        e.Graph.Set(batch,2,"S");e.Graph.Set(batch,3,true);e.Graph.Set(batch,8,true);
        var env=e.Graph.Add("CheckEnvelopeComponent");e.Graph.Wire(batch,env,0);e.Graph.Report(batch);
        e.Results.AddRange(SectionChecks.Batch(f.Engine,cases.Select(c=>c.Force).ToArray(),"S",true));
        Require(e.Results[0].Status==CheckStatus.Satisfied && e.Results[2].Status==CheckStatus.NotSatisfied,"Resistance expected verdicts");
        Require(CheckResults.Envelope(e.Results).Single().Status==CheckStatus.NotSatisfied,"Envelope fail");e.Save();
    }
    private static void SleExamples()
    {
        foreach(bool cracking in new[]{false,true})
        {
            using var e=cracking
                ?new Example("12-fessurazione","Fessurazione e regioni efficaci",
                    "Rettangolo 300 × 500, 8Ø20, analisi lineare con ψrebar=1.5. Caso quasi permanente N=−100 kN e Mx=100 kNm, esposizione XC4, barre non sensibili ad aderenza migliorata, lunga durata. Il copriferro di fessurazione è alla barra: 30+8=38 mm.",
                    "Stress Analysis determina la sezione fessurata. Serviceability Check v2 applica i limiti tensionali e il modello di fessurazione GPC; Effective Cracked Section visualizza le regioni Ac,eff e le barre contribuenti. Il controllo automatico richiede larghezza finita non negativa, almeno una regione e η = w/limite quando il criterio è quello di ampiezza. Nei dettagli sono disponibili kt, aree efficaci e passaggi del motore: non si sostituiscono con formule locali nel plugin.")
                :new Example("11-tensioni-sle","SLE in compressione uniforme e combinazione frequente",
                    "Sezione 300 × 500 con 8Ø20, N=−300 kN e momenti nulli. Per questo confronto analitico si imposta ψrebar=0, analisi lineare e calcestruzzo teso escluso. Si esaminano la combinazione caratteristica e quella frequente.",
                    "Con n=Es/Ec e Ah=Ac+(n−1)As, la compressione uniforme vale σc=N/Ah e |σs|=n|σc|. Il generatore confronta le tensioni effettive GPC con tali valori (tolleranza relativa 10⁻⁶). Il controllo caratteristico restituisce un rapporto; per la combinazione frequente i limiti tensionali non definiti dal profilo restano NotRequired. Non interpretare tale esito come una capacità infinita.");
            var f=new Fixture(e);f.Prepare();
            if(!cracking){e.Graph.Set(f.Options,5,0d);f.Engine.SectionCheckerOptions.PsiCoefficientRebar=0;}
            var force=f.Force(cracking?"SLE-QP":"SLE-CHAR",cracking?-100:-300,0,0,0,cracking?100:0,0);
            var analysis=e.Graph.Add("StressAnalysisComponent");e.Graph.Wire(f.Checker,analysis,0);e.Graph.Wire(force.Node,analysis,1);
            var options=e.Graph.Add("SleOptionsComponent");e.Graph.Set(options,0,cracking?0:2);e.Graph.Set(options,1,"XC4");
            e.Graph.Set(options,2,false);e.Graph.Set(options,3,false);e.Graph.Set(options,4,true);e.Graph.Set(options,5,38d);e.Graph.Set(options,9,false);
            var check=e.Graph.Add("ServiceabilityCheckComponent");e.Graph.Wire(analysis,check,0);e.Graph.Wire(options,check,1);e.Graph.Set(check,2,"S");e.Graph.Report(check);
            var stress=f.Engine.GetTensionAnalysisResult(force.Force);var p=new SleOptions(cracking?0:2,"XC4",false,false,true,38);
            e.Results.Add(SectionChecks.Stress(stress,p,"S"));var crack=SectionChecks.Crack(stress,p,"S");e.Results.Add(crack.Result);
            if(cracking)
            {
                var regions=e.Graph.Add("EffectiveCrackedSectionComponent");e.Graph.Wire(check,regions,0,1);
                Require(crack.Native.Width.HasValue && double.IsFinite(crack.Native.Width.Value) && crack.Native.Width>=0 && crack.Native.Regions.Count>0,"Cracking result");
                Close(crack.Native.Width.Value/crack.Native.Limit.Value,crack.Result.Ratio.Value,"Crack ratio");
                e.Values["Regioni efficaci"]=crack.Native.Regions.Count;
                e.Values["Ac,eff totale [mm²]"]=crack.Native.Regions.Sum(x=>x.Area);
                e.Series=crack.Native.Regions.Select(r=>new {r.Key,r.Area,r.SteelArea,r.Width,r.BarIndices});
            }
            else
            {
                var r=ServiceabilityChecks.Stress(stress,2);double n=SteelMaterialEN1992Data.B450C.E/ConcreteMaterialEN1992Data.C30_37.E;
                double area=f.Model.Area+(n-1)*f.Model.AreaRebars;
                Close(-300000/area,r.ConcreteMinStress,"Uniform concrete stress");
                Close(n*300000/area,r.SteelMaxStress,"Uniform steel stress");
                e.Values["Area omogeneizzata [mm²]"]=area;e.Values["σc [MPa]"]=r.ConcreteMinStress;e.Values["|σs| [MPa]"]=r.SteelMaxStress;
                var frequentOptions=e.Graph.Add("SleOptionsComponent");e.Graph.Set(frequentOptions,0,1);e.Graph.Set(frequentOptions,1,"XC4");
                e.Graph.Set(frequentOptions,5,38d);
                var frequent=e.Graph.Add("ServiceabilityCheckComponent");e.Graph.Wire(analysis,frequent,0);e.Graph.Wire(frequentOptions,frequent,1);e.Graph.Set(frequent,2,"S-frequente");e.Graph.Report(frequent);
                var fp=p with {Combination=1};
                var fr=SectionChecks.Stress(stress,fp,"S-frequente");e.Results.Add(fr);e.Results.Add(SectionChecks.Crack(stress,fp,"S-frequente").Result);
                Require(fr.Status==CheckStatus.NotRequired && fr.Ratio==null,"Frequent stresses not required");
            }
            e.Save();
        }
    }
    private static void CoverExample()
    {
        using var e=new Example("13-copriferro","Copriferro per esposizioni concorrenti",
            "Calcestruzzo C30/37 e profilo EN 1992-1-1 disponibile nel set GPC. Esposizioni concorrenti XC4 e XD1, vita 50 anni, diametro effettivo Ø20, aggregato 20 mm e Δcdev=10 mm. Non si applicano riduzioni di classe, controllo speciale, abrasione o contatto col terreno.",
            "Classe strutturale S4 senza modificatori: cmin,dur governa su XD1 (35 mm), rispetto a XC4 (30 mm). cmin,b=20 mm; cmin=max(10,20,35)=35 mm e cnom=35+10=45 mm. Il copriferro adottato 40 mm è insufficiente: l'esito atteso è NotSatisfied, con domanda 45 e disponibile 40 mm. Il riferimento è il profilo GPC fissato, non una selezione automatica dell'edizione normativa più recente.");
        var g=e.Graph;var concrete=g.Add("ConcreteMaterialComponent");g.Set(concrete,0,3);var standard=g.Add("StandardComponent");g.Set(standard,0,1);
        var cover=g.Add("ConcreteCoverComponent");g.Wire(standard,cover,0);g.Wire(concrete,cover,1);g.Set(cover,2,"XC4","XD1");g.Set(cover,3,20d);g.Set(cover,4,40d);g.Set(cover,15,"S");g.Report(cover);
        var r=DetailingChecks.Cover(new StandardEN1992p11(),new CoverInput(new[]{"XC4","XD1"},30,50,false,false,false,20,20,10),40,"S");
        e.Results.Add(r.Result);e.Values["cmin,b [mm]"]=r.Native.Bond;e.Values["cmin,dur [mm]"]=r.Native.Durability;e.Values["cnom [mm]"]=r.Native.Nominal;
        Close(45,r.Native.Nominal,"Cover example");Require(r.Result.Status==CheckStatus.NotSatisfied,"Cover insufficiency");e.Save();
    }
    private static void AnchorageExample()
    {
        using var e=new Example("14-ancoraggio-sovrapposizione","Ancoraggio e sovrapposizione di barre tese",
            "C30/37, B450C Ø20 e σsd=300 MPa, EN 1992-1-1, condizioni di buona aderenza. Barra rettilinea tesa, coefficienti α1…α5 unitari. Lunghezza disponibile 1000 mm; per la sovrapposizione si assume il 100% delle barre sovrapposte e interferro 20 mm.",
            "Con fctk,0.05=0.7×0.3×fck^(2/3), fbd=2.25 fctk,0.05/γc e lb,rqd=Øσsd/(4fbd), si verifica indipendentemente la lunghezza base. Il ramo di ancoraggio e quello di sovrapposizione richiamano separatamente GPC, che applica i minimi e i coefficienti pertinenti. La lunghezza di sovrapposizione deve risultare non inferiore a quella di ancoraggio in questa configurazione. Nessuna riduzione per ganci o confinamento è assunta.");
        var g=e.Graph;var c=g.Add("ConcreteMaterialComponent");g.Set(c,0,3);var s=g.Add("RebarMaterialComponent");g.Set(s,0,1);
        var standard=g.Add("StandardComponent");g.Set(standard,0,1);
        foreach(bool lap in new[]{false,true})
        {
            var a=g.Add("AnchorageLapComponent");g.Wire(standard,a,0);g.Wire(c,a,1);g.Wire(s,a,2);g.Numbers(a,3,20,300,1000);g.Set(a,6,true);g.Set(a,7,lap);g.Set(a,8,100d);g.Set(a,9,20d);g.Set(a,10,"S");g.Report(a);
            var r=DetailingChecks.Anchorage(new StandardEN1992p11(),ConcreteMaterialEN1992Data.C30_37,SteelMaterialEN1992Data.B450C,20,300,1000,true,lap,100,20,"S");
            e.Results.Add(r.Result);
            if(!lap)
            {
                double fbd=2.25*.7*.3*Math.Pow(30,2d/3)/1.5;
                Close(fbd,r.Native.Fbd,"Bond strength");Close(20*300/(4*fbd),r.Native.BasicLength,"Basic length");
                e.Values["fbd [MPa]"]=r.Native.Fbd;e.Values["lb,rqd [mm]"]=r.Native.BasicLength;
            }
        }
        Require(e.Results[1].Demand>=e.Results[0].Demand,"Lap length");e.Save();
    }
    private static void DetailingExamples()
    {
        foreach(int kind in new[]{0,1,2})
        {
            string[] titles={"Dettagli di armatura di una trave","Dettagli di un pilastro con conferme mancanti","Soletta: armatura principale e secondaria"};
            string[] slugs={"15-dettagli-trave","16-dettagli-pilastro","17-dettagli-soletta"};
            string[] explanations={
                "Trave rettangolare 300 × 500, 8Ø20, staffe Ø8 a due bracci /150, cnom=30 mm e aggregato 20 mm. Le larghezze tese superiore/inferiore e dell'anima sono 300 mm. Si assegna cmin,dur=20 mm e Δcdev=10 mm, senza zona di sovrapposizione.",
                "Pilastro 300 × 500, 8Ø20, staffe Ø8/150 e N minimo=−600 kN. Il copriferro nominale è 30 mm; si lasciano intenzionalmente senza conferma il trattenimento delle barre compresse e i dettagli delle zone di estremità. Si omette anche cmin,dur.",
                "Striscia di soletta larga 1000 mm e alta 250 mm, sei Ø12 per faccia, cbar=30 mm, senza staffe. Armatura secondaria totale delle due facce 500 mm²/m, passo 200 mm, regione non critica. Profilo NTC2018; cmin,dur=20 e Δcdev=10 mm."
            };
            using var e=new Example(slugs[kind],titles[kind],explanations[kind],
                "Reinforcement Detailing produce una riga per ogni regola GPC, conservando Actual, Limit, unità, riferimento e verso del confronto. Gli esiti possono includere regole non soddisfatte o non valutabili: l'esempio serve a leggerle, non assume che il layout sia già approvato. Check Report espone la lista completa. Il controllo automatico richiede un insieme di regole non vuoto e valori disponibili finiti; nel pilastro richiede esplicitamente almeno una regola NotEvaluated, così la mancanza delle conferme non diventa un esito positivo.");
            var f=kind==2?new Fixture(e,"RectangularRcSectionComponent",1000,250,30,0,12,6,6,0):new Fixture(e);f.Prepare();
            double width=kind==2?1000:300, link=kind==2?0:8, spacing=kind==2?0:150;int legs=kind==2?0:2;
            var options=e.Graph.Add("DetailingOptionsComponent");e.Graph.Set(options,0,kind);e.Graph.Numbers(options,1,width,width,width,link,spacing);e.Graph.Set(options,6,legs);
            e.Graph.Numbers(options,7,30,20,10);if(kind!=1)e.Graph.Set(options,10,20d);
            e.Graph.Set(options,11,false);e.Graph.Set(options,12,kind!=1);e.Graph.Set(options,13,kind!=1);
            PlateDetailingData plate=null;
            if(kind==2)
            {
                var p=e.Graph.Add("PlateDetailingOptionsComponent");e.Graph.Set(p,0,500d);e.Graph.Set(p,1,200d);e.Graph.Set(p,2,false);e.Graph.Wire(p,options,14);
                plate=new PlateDetailingData(500,200,false);
            }
            double n=kind==1?-600:0;
            var check=e.Graph.Add("ReinforcementDetailingComponent");e.Graph.Wire(f.Checker,check,0);e.Graph.Wire(options,check,1);e.Graph.Set(check,2,n);e.Graph.Set(check,3,"S");e.Graph.Report(check);
            var pdet=new DetailingOptions((MemberDetailingKind)kind,width,width,width,link,spacing,legs,30,20,10,kind==1?null:20,false,kind!=1,kind!=1,plate);
            e.Results.AddRange(DetailingChecks.Member(f.Engine,pdet,n*1000,"S"));
            Require(e.Results.Count>5 && e.Results.All(r=>(!r.Demand.HasValue||double.IsFinite(r.Demand.Value))&&(!r.Capacity.HasValue||double.IsFinite(r.Capacity.Value))),"Detailing results");
            if(kind==1)Require(e.Results.Any(r=>r.Status==CheckStatus.NotEvaluated),"Unconfirmed details");
            e.Values["As totale [mm²]"]=f.Model.AreaRebars;e.Save();
        }
    }
    private static void AnalysisExamples()
    {
        using(var e=new Example("18-momento-curvatura","Momento–curvatura a N costante",
            "Rettangolo 300 × 500, 8Ø20, N=−300 kN e direzione del momento 0° (Mx). Checker non lineare, dominio plastico, 20 passi e frazione finale 1. Tolleranza assiale assegnata 1 kN.",
            "Il motore cerca il momento limite a N costante e risolve gli stati intermedi. Le curvature GPC in 1/mm sono moltiplicate per 1000 per ottenere 1/m; i momenti Nmm sono divisi per 10⁶ per ottenere kNm. Il controllo automatico verifica equilibrio assiale del punto limite entro 1 kN, almeno due punti e valori finiti. Leggere Status e InterruptedAtStep: una curva interrotta resta parziale e non viene presentata come interamente convergente. Per uso progettuale aumentare i passi e confrontare curva, snervamento e limite."))
        {
            var f=new Fixture(e);f.Prepare(true);
            var m=e.Graph.Add("MomentCurvatureComponent");e.Graph.Wire(f.Checker,m,0);e.Graph.Set(m,1,-300d);e.Graph.Set(m,2,0d);e.Graph.Set(m,3,20);e.Graph.Set(m,4,1d);e.Graph.Set(m,5,1d);e.Graph.Set(m,6,true);
            var r=SectionAnalyses.MomentCurvature(f.Engine,-300000,0,20,1,1000);
            Require(r.Points.Count>=2&&Math.Abs(r.AxialResidual)<=1000&&r.Points.All(p=>double.IsFinite(p.Curvature)&&double.IsFinite(p.Moment)),"M-chi result");
            e.Values["Momento limite [kNm]"]=r.LimitMoment/1e6;e.Values["Residuo N al limite [kN]"]=r.AxialResidual/1000;e.Values["Numero punti"]=r.Points.Count;
            if(r.YieldCurvature.HasValue)e.Values["χy [1/m]"]=r.YieldCurvature.Value*1000;
            if(r.UltimateCurvature.HasValue)e.Values["χu [1/m]"]=r.UltimateCurvature.Value*1000;
            e.Series=new{r.Status,r.InterruptedAtStep,r.InterruptionMessage,points=r.Points.Select(p=>new{curvature_per_m=p.Curvature*1000,moment_kNm=p.Moment/1e6,p.Yielded,p.Limit})};e.Save();
        }
        using(var e=new Example("19-dominio-n-costante","Sezione Mx–My del dominio a N assegnato",
            "Sezione rettangolare simmetrica, N=−300 kN e 16 direzioni. Ogni direzione viene risolta a N costante; l'uscita è una polilinea chiusa nel piano dei momenti in kNm.",
            "I vettori di prova sono Mx=cosθ e My=sinθ, campionati uniformemente. Per ogni punto si controlla lo sforzo normale effettivamente equilibrato, con tolleranza relativa 10⁻³ (0.3 kN per questo esempio). I punti opposti devono mostrare la simmetria della sezione e delle armature: si confrontano Mx(θ) e −Mx(θ+π), analogamente per My, entro l'1%. La polilinea è una discretizzazione: raddoppiare le direzioni per valutarne la sensibilità."))
        {
            var f=new Fixture(e);f.Prepare();var d=e.Graph.Add("DomainSliceComponent");e.Graph.Wire(f.Checker,d,0);e.Graph.Set(d,1,-300d);e.Graph.Set(d,2,16);e.Graph.Set(d,3,true);
            var r=SectionAnalyses.Slice(f.Engine,-300000,16);
            foreach(var point in r)Close(-300,point.N,"Slice axial balance",1e-3);
            for(int i=0;i<8;i++){Close(r[i].Mx,-r[i+8].Mx,"Mx symmetry",.01);Close(r[i].My,-r[i+8].My,"My symmetry",.01);}
            e.Values["Mx massimo [kNm]"]=r.Max(p=>p.Mx);e.Values["My massimo [kNm]"]=r.Max(p=>p.My);
            e.Values["Residuo N massimo [kN]"]=r.Max(p=>Math.Abs(p.N+300));e.Series=r.Select(p=>new{Mx_kNm=p.Mx,My_kNm=p.My,N_kN=p.N});e.Save();
        }
        using(var e=new Example("20-ricerca-armatura","Scelta della minima armatura fra tre candidati",
            "Tre sezioni 300 × 500 mm, tutte con quattro barre superiori e quattro inferiori; diametri candidati 16, 20 e 24 mm. C30/37, B450C, copriferro staffa 30 e staffa Ø8 mm. SLU1: N=−300 kN, Mx=180 e My=5 kNm; SLU2: N=−200 kN, Mx=150 e My=10 kNm. Obiettivo η≤0.9.",
            "Per ogni candidato si eseguono entrambe le verifiche N-Mx-My. Si scartano quelli con esiti non valutati/non soddisfatti o utilizzo superiore a 0.9, poi si seleziona l'area minima fra i rimanenti. Il controllo indipendente sul risultato ricalcola questo filtro e confronta indice e area selezionati. La ricerca è discreta e limitata ai tre layout assegnati: non ottimizza diametri continui e non verifica automaticamente taglio, SLE o dettagli."))
        {
            var f=new Fixture(e,"RectangularRcSectionComponent",300,500,30,8,16,4,4,0);f.Prepare();
            var sections=new[]{(f.Section,f.Model),f.Build("RectangularRcSectionComponent",new double[]{300,500,30,8,20,4,4,0}),f.Build("RectangularRcSectionComponent",new double[]{300,500,30,8,24,4,4,0})};
            var candidateNodes=new[]{f.Checker,f.MakeChecker(sections[1].Item1),f.MakeChecker(sections[2].Item1)};
            var models=sections.Select(x=>ModelChecker(x.Item2)).ToArray();
            var loads=new[]{f.Force("SLU1",-300,0,0,0,180,5),f.Force("SLU2",-200,0,0,0,150,10)};
            var search=e.Graph.Add("ReinforcementSearchComponent");foreach(var c in candidateNodes)e.Graph.Wire(c,search,0);foreach(var load in loads)e.Graph.Wire(load.Node,search,1);
            e.Graph.Set(search,2,.9);e.Graph.Set(search,3,true);e.Graph.Report(search,2);
            var r=SectionAnalyses.Search(models,loads.Select(x=>x.Force).ToArray(),.9);e.Results.AddRange(r.Results);
            int expected=Enumerable.Range(0,3).Where(i=>r.Results.Where(x=>x.Section=="Candidate "+i).All(x=>x.Status==CheckStatus.Satisfied&&x.Ratio<=.9))
                .OrderBy(i=>models[i].SectionCheckerAttribute.Section.AreaRebars).DefaultIfEmpty(-1).First();
            Require(r.Index==expected&&r.Index==1,"Candidate selected: Ø16 excluded, Ø20 is the first admissible layout");
            e.Values["Indice selezionato (base zero)"]=r.Index;e.Values["As selezionata [mm²]"]=models[r.Index].SectionCheckerAttribute.Section.AreaRebars;
            e.Values["Diametro selezionato [mm]"]=new[]{16,20,24}[r.Index];e.Save();
        }
    }
}
