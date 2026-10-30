using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Detailing;
using GPC.Checkers.Concrete.Durability;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Serviceability;
using GPC.Checkers.Concrete.Torsion;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;
using Grasshopper.Kernel;

internal static partial class Program
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals, Converters = { new JsonStringEnumConverter() } };
    private static readonly List<(string Folder, string Title, string Checks)> Index = new();
    private static string F(double value) => value.ToString("G9", CultureInfo.InvariantCulture);
    private static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
    private static void Close(double expected, double actual, string message, double relative = 1e-6)
        => Require(double.IsFinite(actual) && Math.Abs(expected-actual) <= relative*Math.Max(1,Math.Abs(expected)), message+$": {expected} != {actual}");

    private sealed class Example : IDisposable
    {
        internal readonly Graph Graph = new();
        internal readonly Dictionary<string,double> Values = new();
        internal readonly List<CheckResult> Results = new();
        internal object Series;
        /// <summary>Why this example differs from 0.2.2 beyond the engine change (component replacements, fixes).</summary>
        internal string changes = "";
        private readonly string slug, title, explanation, verification;
        internal Example(string slug, string title, string explanation, string verification)
        { this.slug=slug; this.title=title; this.explanation=explanation; this.verification=verification; }
        internal void Save()
        {
            string folder=Path.Combine(OutputRoot,slug); Directory.CreateDirectory(folder);
            string wiring=Graph.Save(folder,title);
            string results=JsonSerializer.Serialize(new {
                schema="ReBear.WorkedExample", version=1, example=slug, engine=typeof(SectionCheckerModelCode2010).Assembly.GetName().FullName,
                verification="Calcolo adapter GPC eseguito; archivio GH riletto; soluzione dentro Rhino non eseguita.",
                values=Values, checks=Results, series=Series },Json);
            File.WriteAllText(Path.Combine(folder,"results.json"),results);
            var text=new StringBuilder("# "+slug[..2]+" — "+title+"\n\n"+explanation+"\n\n");
            text.AppendLine("## Esecuzione\n\nAprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear " + ReBearVersion + " in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.\n");
            text.AppendLine("## Svolgimento e controllo\n\n"+verification+"\n");
            if(Values.Count>0)
            {
                text.AppendLine("| Quantità calcolata | Valore |\n|---|---:|");
                foreach(var v in Values) text.AppendLine($"| {v.Key} | {F(v.Value)} |");
                text.AppendLine();
            }
            if(Results.Count>0)
            {
                text.AppendLine("| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |\n|---|---|---|---:|---:|---:|---|");
                foreach(var r in Results) text.AppendLine($"| {r.Case} | {r.Check} | {r.Status} | {(r.Ratio.HasValue?F(r.Ratio.Value):"—")} | {(r.Demand.HasValue?F(r.Demand.Value):"—")} | {(r.Capacity.HasValue?F(r.Capacity.Value):"—")} | {r.Unit} |");
                text.AppendLine("\nEsiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.\n");
            }
            text.AppendLine("Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.");
            text.Append(Baseline.Differences(slug,results,changes,Graph.Nodes));
            text.Append(wiring);
            File.WriteAllText(Path.Combine(folder,"README.md"),text.ToString().TrimEnd()+Environment.NewLine);
            Index.Add((slug,title,$"{Results.Count} esiti; {Values.Count} valori"));
            Console.WriteLine("PASS "+slug+" — "+title);
        }
        public void Dispose() => Graph.Dispose();
    }

    private sealed class Fixture
    {
        internal readonly Example E;
        internal Graph G => E.Graph;
        internal GH_Component Concrete, Steel, Section, Standard, Axes, Options, Checker;
        internal ReinforcedConcreteSection Model;
        internal SectionCheckerModelCode2010 Engine;
        /// <summary>Without a shape only the actions and their coordinate system: no materials or section on the canvas.</summary>
        internal Fixture(Example e, string shape="RectangularRcSectionComponent", params double[] dimensions)
        {
            E=e; if(shape==null) return;
            Concrete=G.Add("ConcreteMaterialComponent"); G.Set(Concrete,0,3);
            Steel=G.Add("RebarMaterialComponent");
            if(dimensions.Length==0) dimensions=new double[]{300,500,30,8,20,4,4,0};
            (Section,Model)=Build(shape,dimensions);
        }
        internal (GH_Component,ReinforcedConcreteSection) Build(string shape, double[] p)
        {
            var section=G.Add(shape); G.Wire(Concrete,section,0); G.Wire(Steel,section,1); G.Dimensions(section,2,p);
            var c=ConcreteMaterialEN1992Data.C30_37; var s=SteelMaterialEN1992Data.B450C;
            ReinforcedConcreteSection model=shape switch {
                "RectangularRcSectionComponent"=>SectionBuilders.Rectangle(c,s,p[0],p[1],p[2],p[3],p[4],(int)p[5],(int)p[6],(int)p[7]),
                "CircularRcSectionComponent"=>SectionBuilders.Circle(c,s,p[0],p[1],p[2],p[3],(int)p[4],(int)p[5]),
                "TeeRcSectionComponent"=>SectionBuilders.Tee(c,s,p[0],p[1],p[2],p[3],p[4],p[5],p[6],(int)p[7],(int)p[8],(int)p[9]),
                "IRcSectionComponent"=>SectionBuilders.ISection(c,s,p[0],p[1],p[2],p[3],p[4],p[5],p[6],p[7],p[8],(int)p[9],(int)p[10],(int)p[11]),
                "BoxRcSectionComponent"=>SectionBuilders.Box(c,s,p[0],p[1],p[2],p[3],p[4],p[5],p[6],(int)p[7],(int)p[8],(int)p[9],(int)p[10]),
                _=>throw new NotSupportedException(shape)
            };
            return(section,model);
        }
        internal GH_Component CoordinateSystem()
        {
            if(Axes!=null)return Axes;
            Axes=G.Add("CoordinateSystemComponent"); G.Set(Axes,0,"Global XY");
            G.Set(Axes,1,new Rhino.Geometry.Point3d(0,0,0)); G.Set(Axes,2,new Rhino.Geometry.Point3d(1,0,0)); G.Set(Axes,3,new Rhino.Geometry.Point3d(0,1,0));
            return Axes;
        }
        internal void Prepare(bool nonlinear=false, int standard=0)
        {
            Standard=G.Add("StandardComponent"); if(standard!=0) G.Set(Standard,0,standard);
            Options=G.Add("CheckerOptionsComponent"); G.Wire(CoordinateSystem(),Options,0);
            G.Set(Options,2,0); if(nonlinear) G.Set(Options,4,0);
            Checker=MakeChecker(Section);
            Engine=ModelChecker(Model,nonlinear,standard);
        }
        internal GH_Component MakeChecker(GH_Component section)
        {
            var checker=G.Add("CheckerComponent"); G.Wire(section,checker,0); G.Wire(Standard,checker,1); G.Wire(Options,checker,2); return checker;
        }
        internal (GH_Component Node,ResultBeamForces Force) Force(string name,double n,double vx,double vy,double t,double mx,double my)
        {
            var force=G.Add("SectionForceComponent"); G.Set(force,0,name);
            var actions=new[]{n,vx,vy,t,mx,my};
            for(int i=0;i<actions.Length;i++) if(actions[i]!=0) G.Set(force,i+1,actions[i]);
            G.Wire(CoordinateSystem(),force,7);
            return(force,new ResultBeamForces(n*1000,vx*1000,vy*1000,t*1e6,mx*1e6,my*1e6,GPC.Geometry.CoordinateSystem.Global,name:name));
        }
        internal (GH_Component Node,ShearOptions Value) Shear(double bw,double depth,double area,double links=2*Math.PI*16)
        {
            var options=G.Add("ShearOptionsComponent"); G.Wire(Steel,options,0); G.Wire(Steel,options,1);
            G.Numbers(options,2,bw,depth,area,links,150); G.Set(options,8,1.5);
            return(options,new ShearOptions(SteelMaterialEN1992Data.B450C,SteelMaterialEN1992Data.B450C,bw,depth,area,links,150,CotTheta:1.5));
        }
        internal void Geometry(double requiredCover,double requiredClear)
        {
            var check=G.Add("SectionGeometryCheckComponent"); G.Wire(Section,check,0); G.Set(check,1,requiredCover); G.Set(check,2,requiredClear); G.Set(check,3,"S");
            G.Report(check); var info=SectionBuilders.Inspect(Model);
            E.Values.Add("Ac [mm²]",info.Area); E.Values.Add("As [mm²]",info.SteelArea); E.Values.Add("Numero barre",info.Bars);
            E.Values.Add("cbar,min [mm]",info.MinimumBarCover); E.Values.Add("Interferro minimo [mm]",info.MinimumClearSpacing);
            Require(info.MinimumBarCover>=requiredCover && info.MinimumClearSpacing>=requiredClear,"Geometry acceptance");
            E.Results.Add(new CheckResult("S","Geometry","Geometry",CheckStatus.Satisfied,null,
                Message:$"Copriferro minimo {F(info.MinimumBarCover)} mm ≥ {F(requiredCover)}; interferro {F(info.MinimumClearSpacing)} mm ≥ {F(requiredClear)}."));
        }
    }
    private static SectionCheckerModelCode2010 ModelChecker(ReinforcedConcreteSection section,bool nonlinear=false,int standard=0)
        => new(new SectionCheckerAttribute(section),new SectionCheckerModelCode2010.SectionOptionsModelCode2010 {
            ForceReferenceCoordinateSystem=CoordinateSystem.Global, FailureDomainType=SectionSolver.FailureDomainTypes.Plastic,
            FailureAnalysisType=SectionSolver.FailureAnalysisTypes.ConstantN, DomainPointStrategy=SectionSolver.DomainPointStrategyTypes.Iterative,
            StressAnalysisType=nonlinear?SectionSolver.StressAnalysisTypes.NonLinear:SectionSolver.StressAnalysisTypes.Linear,
            PsiCoefficientRebar=1.5,PsiCoefficientTendon=0,ConsiderTensileConcrete=false
        },standard==0?new StandardNTC2018Concrete():new StandardEN1992p11());

    private static void Geometries()
    {
        using(var e=new Example("01-rettangolo","Sezione rettangolare 300 × 500",
            "Sezione in C30/37 e B450C, quattro barre Ø20 superiori e quattro inferiori. Copriferro alla staffa 30 mm e staffa Ø8. L'esempio costruisce la sezione ed esegue il controllo geometrico delle barre.",
            "Distanza dei centri dal bordo: a = 30 + 8 + 20/2 = 48 mm. Ac = 300 × 500 = 150000 mm²; As = 8 × π20²/4. Il copriferro alla barra è 38 mm. L'interferro della fila è (300 − 2×48)/3 − 20 = 48 mm. Il controllo automatico confronta aree e interferro con queste espressioni."))
        {
            var f=new Fixture(e); f.Geometry(38,25); Close(150000,f.Model.Area,"Rectangular area"); Close(800*Math.PI,f.Model.AreaRebars,"Steel area");
            Close(48,SectionBuilders.Inspect(f.Model).MinimumClearSpacing,"Clear distance"); e.Save();
        }
        using(var e=new Example("02-cerchio","Sezione circolare e rapporto di omogeneizzazione",
            "Pilastro di diametro 600 mm, 12Ø20, cstaffa=40 mm e Østaffa=10 mm. Il contorno è un poligono inscritto di 64 lati. A lato è predisposta la conversione da φ=1.5 a rapporto modulare n.",
            "Ac = n/2 × R² sin(2π/n), con n=64 e R=300 mm; confrontare con πR² per misurare l'errore geometrico. Il raggio della corona è R cos(π/64) − (40+10+10), così il copriferro è rispettato sui lati. Per omogeneizzazione n = Es(1+φ)/Ecm ed Ec,eff=Ecm/(1+φ); è una conversione da φ assegnato, non una previsione temporale."))
        {
            var f=new Fixture(e,"CircularRcSectionComponent",600,40,10,20,12,64); f.Geometry(50,25);
            e.Values["Errore area rispetto al cerchio [%]"]=100*(1-f.Model.Area/(Math.PI*300*300));
            var h=e.Graph.Add("HomogenizationOptionsComponent"); e.Graph.Wire(f.Concrete,h,0); e.Graph.Wire(f.Steel,h,1); e.Graph.Set(h,3,1.5);
            var r=Homogenization.Resolve(SteelMaterialEN1992Data.B450C.E,ConcreteMaterialEN1992Data.C30_37.Ecm,false,1.5);
            e.Values["n [-]"]=r.N; e.Values["Ec,eff [MPa]"]=ConcreteMaterialEN1992Data.C30_37.Ecm/2.5;
            Close(64*.5*90000*Math.Sin(2*Math.PI/64),f.Model.Area,"Polygon area"); e.Save();
        }
        using(var e=new Example("03-sezione-t","Sezione a T con ala superiore",
            "Sezione a T: bf=800, h=700, bw=300, tf=180 mm. Armatura Ø20: sei barre nell'ala superiore, tre inferiori nell'anima, due per ciascun lato dell'anima. cstaffa=30 mm, Østaffa=8 mm.",
            "Ac = bf tf + bw(h−tf) = 800×180 + 300×520 = 300000 mm². Le barre laterali escludono quelle delle file superiore/inferiore. Il controllo comprende contenimento e distanza anche dai rientri del contorno. L'origine è il centro dell'ingombro: non coincide necessariamente con il baricentro. Non viene dedotto un modello di taglio dall'ala."))
        {
            var f=new Fixture(e,"TeeRcSectionComponent",800,700,300,180,30,8,20,6,3,2);f.Geometry(38,25);
            Close(300000,f.Model.Area,"T area");Require(f.Model.Rebars.Count()==13,"T bars");e.Save();
        }
        using(var e=new Example("04-sezione-i","Sezione a I con ali differenti",
            "Sezione a I: bt=600, bb=500, h=800, bw=240, tt=tb=180 mm. Ø20 con cinque barre superiori, quattro inferiori e due per ogni lato dell'anima. Copriferro esterno staffa 30 mm, staffa Ø8.",
            "Ac = bt tt + bb tb + bw(h−tt−tb) = 303600 mm². As è la somma delle 13 barre Ø20. Si controllano i rientri, il copriferro e l'interferro. L'asse dell'ingombro rimane il riferimento geometrico; per analisi baricentriche predisporre il riferimento mediante Section Coordinate System (assi globali nel baricentro)."))
        {
            var f=new Fixture(e,"IRcSectionComponent",600,500,800,240,180,180,30,8,20,5,4,2);f.Geometry(38,25);
            Close(303600,f.Model.Area,"I area");e.Save();
        }
        using(var e=new Example("05-sezione-cava","Sezione cava con due corone",
            "Ingombro 1000 × 1200 mm e foro centrale 500 × 700 mm. Barre Ø20, copriferro alle staffe 35 mm e staffe Ø10. Corona esterna: nx=6, ny=8; interna: nx=4, ny=6. Gli angoli di ogni corona vengono contati una sola volta.",
            "Ac = 1000×1200 − 500×700 = 850000 mm². Numero barre = [2×6+2×(8−2)] + [2×4+2×(6−2)] = 40. Il controllo misura il copriferro sia dal bordo esterno sia dal foro, verificando che nessuna barra cada nella cavità. Non si assume automaticamente un profilo resistente a torsione."))
        {
            var f=new Fixture(e,"BoxRcSectionComponent",1000,1200,500,700,35,10,20,6,8,4,6);f.Geometry(45,25);
            Close(850000,f.Model.Area,"Box area");Require(f.Model.Rebars.Count()==40,"Box bars");e.Save();
        }
    }

    private static void EnvelopesExample()
    {
        using var e=new Example("06-inviluppo-azioni","Inviluppo con combinazioni concomitanti",
            "Tre combinazioni nominali A, B e C nello stesso riferimento globale. Nessun coefficiente di combinazione è generato dal componente: le azioni sono già combinate. L'obiettivo è conservare l'associazione fra l'estremo e le altre cinque azioni dello stesso caso.",
            "Si confrontano min/max separatamente per N, Vx, Vy, T, Mx e My. Il minimo N è −500 kN nel caso B; il massimo Mx è 120 kNm nel caso C. Il vettore restituito per Mx max conserva N=−100 kN e Vy=120 kN di C, anche se il minimo N appartiene a B. L'indice è base zero e gli estremi ripetuti mantengono la prima occorrenza.");
        var f=new Fixture(e,shape:null);
        var cases=new[]{f.Force("A",-300,60,90,2,45,15),f.Force("B",-500,-80,40,-4,100,-25),f.Force("C",-100,20,120,7,120,35)};
        var env=e.Graph.Add("LoadEnvelopeComponent");foreach(var item in cases)e.Graph.Wire(item.Node,env,0);
        var r=LoadEnvelopes.Calculate(cases.Select(x=>x.Force).ToArray());
        e.Series=r.Select(x=>new{x.Component,x.Maximum,x.Index,caseName=x.Force.Name,value=x.Value/(x.Component is "N" or "Vx" or "Vy"?1000:1e6),actions=LoadEnvelopes.Values(x.Force)});
        e.Values["N minimo [kN]"]=r.Single(x=>x.Component=="N"&&!x.Maximum).Value/1000;
        e.Values["Mx massimo [kNm]"]=r.Single(x=>x.Component=="Mx"&&x.Maximum).Value/1e6;
        var govern=r.Single(x=>x.Component=="Mx"&&x.Maximum);Close(-100e3,govern.Force.N,"Concomitant N");Close(120e3,govern.Force.V2,"Concomitant Vy");e.Save();
    }
    private static void ShearExamples()
    {
        using(var e=new Example("07-taglio-senza-staffe","Taglio senza staffe: caso valutabile e caso escluso",
            "Rettangolo 300 × 500 mm, 8Ø20. Direzione Y: bw=300, d=452 mm, Asl=4π20²/4. Si confrontano un caso compresso e uno teso; Asw=0 esclude il contributo delle staffe.",
            "Nel caso compresso N=−300 kN e Vy=60 kN. Con σcp=300000/150000=2 MPa, k=1+√(200/452), ρl=Asl/(bw d), il controllo indipendente usa VRd=max{[0.18/1.5 k(100ρl fck)^(1/3)+0.15σcp]bw d; [0.035 k^(3/2)√fck+0.15σcp]bw d}. Nel secondo caso N=+100 kN: il profilo GPC restituisce NotEvaluated, che deve impedire un inviluppo complessivo positivo."))
        {
            var f=new Fixture(e);f.Prepare();var opt=f.Shear(300,452,400*Math.PI,0);
            var cases=new[]{f.Force("Compresso",-300,0,60,0,45,0),f.Force("Teso",100,0,60,0,45,0)};
            var batch=e.Graph.Add("BatchSectionChecksComponent");e.Graph.Wire(f.Checker,batch,0);foreach(var v in cases)e.Graph.Wire(v.Node,batch,1);
            e.Graph.Set(batch,2,"S");e.Graph.Set(batch,3,false);e.Graph.Wire(opt.Node,batch,5);e.Graph.Set(batch,8,true);e.Graph.Report(batch);
            e.Results.AddRange(SectionChecks.Batch(f.Engine,cases.Select(x=>x.Force).ToArray(),"S",false,y:opt.Value));
            double k=1+Math.Sqrt(200d/452),rho=400*Math.PI/(300*452);
            double cap=Math.Max((.18/1.5*k*Math.Pow(100*rho*30,1d/3)+.15*2)*300*452,(.035*Math.Pow(k,1.5)*Math.Sqrt(30)+.15*2)*300*452);
            Close(cap/1000,e.Results[0].Capacity.Value,"Analytical shear");
            Require(e.Results[1].Status==CheckStatus.NotEvaluated,"Axial tension excluded");e.Values["VRd analitico [kN]"]=cap/1000;e.Save();
        }
        using(var e=new Example("08-taglio-xy","Taglio X e Y in un unico batch",
            "Stessa sezione rettangolare; staffe Ø8 a due bracci, passo 150 mm, B450C, cotθ=1.5. Un unico Batch Section Checks riceve le opzioni distinte X e Y e due combinazioni complete.",
            "Asw = 2π8²/4. Per X si assegnano bw=500, d=252 mm e Asl=2π20²/4; per Y bw=300, d=452 mm e Asl=4π20²/4. Le domande sono V1 per X e V2 per Y, mentre i momenti associati sono M2 e M1. Il controllo automatico verifica quattro risultati e la corretta domanda su entrambe le direzioni. Modificare un'opzione X non deve cambiare il significato dell'opzione Y."))
        {
            var f=new Fixture(e);f.Prepare();var x=f.Shear(500,252,200*Math.PI);var y=f.Shear(300,452,400*Math.PI);
            var cases=new[]{f.Force("SLU1",-300,60,90,0,45,15),f.Force("SLU2",-200,-100,70,0,70,-25)};
            var batch=e.Graph.Add("BatchSectionChecksComponent");e.Graph.Wire(f.Checker,batch,0);foreach(var c in cases)e.Graph.Wire(c.Node,batch,1);
            e.Graph.Set(batch,2,"S");e.Graph.Set(batch,3,false);e.Graph.Wire(x.Node,batch,4);e.Graph.Wire(y.Node,batch,5);e.Graph.Set(batch,8,true);
            var env=e.Graph.Add("CheckEnvelopeComponent");e.Graph.Wire(batch,env,0);e.Graph.Report(batch);
            e.Results.AddRange(SectionChecks.Batch(f.Engine,cases.Select(c=>c.Force).ToArray(),"S",false,x.Value,y.Value));
            Require(e.Results.Count==4,"Two directions per case");Close(60,e.Results[0].Demand.Value,"Vx");Close(90,e.Results[1].Demand.Value,"Vy");e.Save();
        }
        using(var e=new Example("09-torsione-taglio","Torsione con due tagli concomitanti",
            "Sezione 300 × 500 mm con 8Ø20. Si assegna un profilo efficace Ak=80000 mm², uk=1200 mm, tef=100 mm. Staffe chiuse Ø8/150, cotθ=1.5; area longitudinale aggiuntiva per torsione 800 mm². È un dato disponibile assegnato e richiede conferma della sua effettiva disposizione.",
            "Azioni: N=−300 kN, Vx=20 kN, Vy=30 kN, T=5 kNm, Mx=45 kNm e My=15 kNm. La torsione usa l'area di un braccio At=π8²/4; le opzioni dei tagli usano due bracci. Il controllo richiama GPC con entrambe le direzioni e confronta il rapporto con quello della sola torsione, a parità degli altri dati. L'interazione non deve migliorare l'utilizzo. Geometria efficace e quota aggiuntiva di armatura non vengono inferite dal builder."))
        {
            var f=new Fixture(e);f.Prepare();var x=f.Shear(500,252,200*Math.PI);var y=f.Shear(300,452,400*Math.PI);var load=f.Force("SLU-TV",-300,20,30,5,45,15);
            var t=e.Graph.Add("TorsionOptionsComponent");e.Graph.Numbers(t,0,80000,1200,100);e.Graph.Wire(f.Steel,t,3);e.Graph.Wire(f.Steel,t,4);e.Graph.Numbers(t,5,Math.PI*16,150,800,1.5);e.Graph.Set(t,10,true);
            var check=e.Graph.Add("TorsionShearCheckComponent");e.Graph.Wire(f.Checker,check,0);e.Graph.Wire(load.Node,check,1);e.Graph.Wire(t,check,2);e.Graph.Wire(x.Node,check,3);e.Graph.Wire(y.Node,check,4);e.Graph.Set(check,5,"S");e.Graph.Report(check);
            var opt=new TorsionOptions(new TorsionGeometry(80000,1200,100),SteelMaterialEN1992Data.B450C,SteelMaterialEN1992Data.B450C,Math.PI*16,150,800,1.5);
            var r=AdvancedShearChecks.Torsion(f.Engine,load.Force,opt,x.Value,y.Value,"S");e.Results.Add(r);
            var pure=AdvancedShearChecks.Torsion(f.Engine,new ResultBeamForces(-300e3,0,0,5e6,45e6,15e6,CoordinateSystem.Global),opt,null,null,"S");
            Require(r.Ratio>=pure.Ratio,"Combined torsion utilization");e.Values["η sola torsione [-]"]=pure.Ratio.Value;e.Save();
        }
    }
}
