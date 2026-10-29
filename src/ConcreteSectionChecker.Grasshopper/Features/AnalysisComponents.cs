using System;
using System.Linq;
using System.Threading;
using ConcreteSectionChecker.Adapters;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Cracking;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Serviceability;
using GPC.Model.Materials;
using GPC.Model.Results;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ConcreteSectionChecker.Grasshopper.Features
{
    public sealed class MomentCurvatureComponent : ReBearAsyncComponent
    {
        public MomentCurvatureComponent() : base("Moment Curvature", "Mχ", "Analisi GPC a N costante in background. Checker non lineare; curvatura 1/m e momenti kNm.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0018");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Checker non lineare."), N("Axial force", "N", "kN, compressione negativa.", 0),
            N("Moment direction", "θ", "Gradi negli assi del checker.", 0), I("Steps", "n", "10–500 passi.", 60),
            N("End fraction", "f", "Frazione del momento limite (0,1].", 1), N("Axial tolerance", "tolN", "Tolleranza sull'equilibrio di N, kN.", 1),
            B("Run", "Run", "Avvia il calcolo.") };
        protected override Port[] Outputs => new[] { C("Diagram", "D", "Polilinea (χ in 1/m, M in kNm)."),
            List(N("Curvature", "χ", "1/m.")), List(N("Moment", "M", "kNm.")), N("Yield curvature", "χy", "1/m, NaN se non raggiunta."),
            N("Ultimate curvature", "χu", "1/m, NaN se non raggiunta."), T("Status", "S", "Convergenza, eventuale interruzione e limiti."),
            G("Response", "R", "Risultato GPC completo.") };
        protected override Func<CancellationToken, Action<IGH_DataAccess>> Prepare(IGH_DataAccess d)
        {
            if (!Read(d, 6, false)) return null;
            var checker = Required<SectionCheckerModelCode2010>(d, 0);
            double n = Finite(d, 1) * 1000, direction = Finite(d, 2), end = Finite(d, 4), tolerance = Finite(d, 5) * 1000; int steps = Required<int>(d, 3);
            return token =>
            {
                var r = SectionAnalyses.MomentCurvature(SectionAnalyses.Detached(checker, SectionSolver.FailureAnalysisTypes.ConstantN),
                    n, direction, steps, end, tolerance, token);
                var points = r.Points.Select(p => new Point3d(p.Curvature * 1000, p.Moment / 1e6, 0)).ToArray();
                return o =>
                {
                    if (points.Length >= 2) o.SetData(0, new PolylineCurve(points));
                    o.SetDataList(1, r.Points.Select(p => p.Curvature * 1000)); o.SetDataList(2, r.Points.Select(p => p.Moment / 1e6));
                    o.SetData(3, r.YieldCurvature * 1000 ?? double.NaN); o.SetData(4, r.UltimateCurvature * 1000 ?? double.NaN);
                    o.SetData(5, (r.Status + " " + r.InterruptionMessage).Trim()); o.SetData(6, Data(r));
                    if (r.InterruptedAtStep.HasValue) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Curva parziale: " + r.InterruptionMessage);
                };
            };
        }
    }
    public sealed class DomainSliceComponent : ReBearAsyncComponent
    {
        public DomainSliceComponent() : base("Domain Slice at N", "MxMy", "Sezione Mx–My del dominio a N assegnato, in background; ogni punto è risolto e controllato da GPC.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0019");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Checker e tipo di dominio."), N("Axial force", "N", "kN, compressione negativa.", 0),
            I("Directions", "n", "8–360 direzioni; controllare la sensibilità alla discretizzazione.", 64), B("Run", "Run", "Avvia il calcolo.") };
        protected override Port[] Outputs => new[] { C("Mx-My curve", "D", "Polilinea chiusa in kNm."),
            List(N("Mx", "Mx", "kNm.")), List(N("My", "My", "kNm.")), List(N("Solved N", "N", "N effettivamente equilibrato, kN.")) };
        protected override Func<CancellationToken, Action<IGH_DataAccess>> Prepare(IGH_DataAccess d)
        {
            if (!Read(d, 3, false)) return null;
            var checker = Required<SectionCheckerModelCode2010>(d, 0); double n = Finite(d, 1) * 1000; int samples = Required<int>(d, 2);
            if (samples < 8 || samples > 360) throw new ArgumentException($"{Label(2)}: 8–360 direzioni richieste.");
            return token =>
            {
                var r = SectionAnalyses.Slice(SectionAnalyses.Detached(checker, SectionSolver.FailureAnalysisTypes.ConstantN, samples), n, samples, token);
                var points = r.Select(p => new Point3d(p.Mx, p.My, 0)).ToList(); points.Add(points[0]);
                return o =>
                {
                    o.SetData(0, new PolylineCurve(points)); o.SetDataList(1, r.Select(p => p.Mx)); o.SetDataList(2, r.Select(p => p.My)); o.SetDataList(3, r.Select(p => p.N));
                };
            };
        }
    }
    public sealed class EffectiveCrackedSectionComponent : ReBearComponent
    {
        public EffectiveCrackedSectionComponent() : base("Effective Cracked Section", "AcEff", "Regioni efficaci calcolate da GPC per la fessurazione; non ricostruisce formule o zone per approssimazione.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0020");
        protected override Port[] Inputs => new[] { G("Crack data", "Cr", "Uscita Crack data di Serviceability Check.") };
        protected override Port[] Outputs => new[] { List(T("Regions", "K", "Chiavi delle regioni.")), List(N("Effective concrete area", "Ac", "mm².")),
            List(N("Effective steel area", "As", "mm².")), List(N("Crack width", "w", "mm; NaN se non valutata.")),
            List(Surface("Regions preview", "G", "Brep delle regioni, con fori quando presenti.")), List(T("Bar indices", "i", "Indici base zero delle barre ordinarie contribuenti.")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var r = Required<SectionCrackResult>(d, 0);
            d.SetDataList(0, r.Regions.Select(p => p.Key)); d.SetDataList(1, r.Regions.Select(p => p.Area));
            d.SetDataList(2, r.Regions.Select(p => p.SteelArea)); d.SetDataList(3, r.Regions.Select(p => p.Width ?? double.NaN));
            Curve Loop(System.Collections.Generic.IReadOnlyList<GPC.Geometry.Point2d> points)
            {
                var vertices = points.Select(p => new Point3d(p.X, p.Y, 0)).ToList(); vertices.Add(vertices[0]); return new PolylineCurve(vertices);
            }
            d.SetDataList(4, r.Regions.Select(p =>
            {
                if (p.Outline.Count < 3) return null;
                var curves = new[] { Loop(p.Outline) }.Concat(p.Holes.Where(h => h.Count >= 3).Select(Loop));
                var faces = Brep.CreatePlanarBreps(curves, 1e-6);
                if (faces?.Length == 1) return faces[0];
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Preview non disponibile per la regione " + p.Key);
                return null; // Preserve one output slot per region, including failed previews.
            }));
            d.SetDataList(5, r.Regions.Select(p => string.Join(",", p.BarIndices)));
            if (r.Regions.Count == 0) AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Nessuna regione efficace restituita: " + r.Status);
        }
    }
    public sealed class HomogenizationOptionsComponent : ReBearComponent
    {
        public HomogenizationOptionsComponent() : base("Creep and Homogenization", "φ-n", "Conversione GPC fra coefficiente viscoso assegnato φ e rapporto modulare n. Non stima φ da età o umidità.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0021");
        protected override Port[] Inputs => new[] { G("Concrete", "C", "Calcestruzzo europeo, Ecm."), G("Steel", "S", "Acciaio, Es."),
            B("Input is n", "n?", "True rapporto modulare; false coefficiente viscoso."), N("Value", "V", "φ oppure n.", 0) };
        protected override Port[] Outputs => new[] { N("Creep coefficient", "φ", "φ assegnato/ricavato."), N("Modular ratio", "n", "Rapporto Es/Ec,eff."),
            N("Effective concrete modulus", "Ec,eff", "MPa.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var c = Required<ConcreteMaterialEuropeanCommon>(d, 0); var s = Required<SteelMaterial>(d, 1);
            var r = Homogenization.Resolve(s.E, c.Ecm, Read(d, 2, false), Finite(d, 3));
            d.SetData(0, r.Phi); d.SetData(1, r.N); d.SetData(2, c.Ecm / Homogenization.CreepFactor(r.Phi));
        }
    }
    public sealed class ReinforcementSearchComponent : ReBearAsyncComponent
    {
        public ReinforcementSearchComponent() : base("Reinforcement Candidate Search", "AsSearch",
            "Sceglie la minima As fra i checker candidati che soddisfano tutte le combinazioni N-Mx-My; candidati in parallelo, in background. Non verifica taglio, SLE o dettagli.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0022");
        protected override Port[] Inputs => new[] { List(G("Candidate checkers", "C", "1–100 candidati espliciti; vincoli geometrici definiti a monte.")),
            List(G("Forces", "F", "Combinazioni N-Mx-My, massimo 1000.")), N("Target utilization", "η", "0 < η ≤ 1.", 1), B("Run", "Run", "Avvia la ricerca.") };
        protected override Port[] Outputs => new[] { I("Selected index", "i", "-1 se nessun candidato idoneo."),
            G("Selected checker", "C", "Checker selezionato."), List(G("All results", "R", "Esiti di ogni candidato e combinazione.")),
            N("Selected steel area", "As", "mm², NaN se nessun candidato.") };
        protected override Func<CancellationToken, Action<IGH_DataAccess>> Prepare(IGH_DataAccess d)
        {
            if (!Read(d, 3, false)) return null;
            var candidates = ReadList<SectionCheckerModelCode2010>(d, 0); var forces = ReadList<ResultBeamForces>(d, 1); double target = Finite(d, 2);
            return token =>
            {
                // isolate: every candidate gets a private checker for its worker thread.
                var r = SectionAnalyses.Search(candidates, forces, target, token, isolate: true);
                return o =>
                {
                    o.SetData(0, r.Index); o.SetDataList(2, r.Results.Select(p => new GH_CheckResult(p)));
                    o.SetData(3, r.Index < 0 ? double.NaN : candidates[r.Index].SectionCheckerAttribute.Section.AreaRebars);
                    if (r.Index >= 0) o.SetData(1, new GH_SectionChecker(candidates[r.Index]));
                    else AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Nessun candidato soddisfa tutti i casi N-Mx-My.");
                };
            };
        }
    }
}
