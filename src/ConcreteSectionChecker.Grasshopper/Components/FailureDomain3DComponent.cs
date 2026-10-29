using System;
using System.Collections.Generic;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class FailureDomain3DComponent : ReBearComponent
    {
        public FailureDomain3DComponent() : base("3D Failure Domain Check", "FDA",
            "Dominio resistente 3D GPC del checker. Punti e mesh in (Mx [kNm], My [kNm], N [kN]).") { }
        public override Guid ComponentGuid => new("a4c3e502-6c1e-4954-a9ef-7ac9e8bab5ef");
        protected override Port[] Inputs => new[] { G("Concrete Checker", "CC", "Checker GPC.") };
        protected override Port[] Outputs => new[] { G("3D Failure Domain Analysis", "FDA", "Dominio GPC con il checker che l'ha prodotto."),
            List(P("3D Failure Domain Points", "FDP", "Punti del dominio (Mx kNm, My kNm, N kN).")), M("3D Failure Domain Mesh", "FDM", "Mesh del dominio, stesse unità.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var checker = Required<SectionCheckerModelCode2010>(d, 0);
            var result = checker.GetFailureDomainResult() ?? throw new InvalidOperationException("Dominio resistente non disponibile.");
            var points = result.Domain.DomainPoints.SelectMany(row => row)
                .Select(p => new Point3d(p.Point.X / 1e6, p.Point.Y / 1e6, p.Point.Z / 1000)).ToList();
            var mesh = RhinoMeshes.ToRhino(SectionMeshes.From(result.Domain.GetMesh(out _)));
            mesh.Transform(Transform.Scale(Plane.WorldXY, 1e-6, 1e-6, 1e-3));
            d.SetData(0, new GH_FailureDomain3DResult(result, checker));
            d.SetDataList(1, points);
            d.SetData(2, new GH_Mesh(mesh));
        }
    }

    public sealed class PostProcessFailureDomain3DComponent : ReBearComponent
    {
        public PostProcessFailureDomain3DComponent() : base("3D Failure Domain Analysis", "FDP",
            "Verifica N-Mx-My delle combinazioni con il checker del dominio; punti di domanda e resistenza per la visualizzazione. Report con Check Report.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000013");
        protected override Port[] Inputs => new[] { G("3D Failure Domain Analysis", "FDA", "Da 3D Failure Domain Check."),
            List(G("Forces", "F", "Combinazioni.")), T("Section ID", "ID", "Identificativo per gli inviluppi.", "Section") };
        protected override Port[] Outputs => new[] { List(G("Results", "R", "Esito N-Mx-My per combinazione (Check Report / Check Envelope).")),
            List(P("External Force", "EF", "Domanda (Mx kNm, My kNm, N kN) nel riferimento della combinazione.")),
            List(P("Resistance Force", "RF", "Punto resistente (Mx kNm, My kNm, N kN); non impostato se assente.")),
            List(N("Utilization", "η", "Rapporto di utilizzo GPC; NaN se non disponibile.")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var domain = RequiredGoo<GH_FailureDomain3DResult>(d, 0);
            var checker = domain.Checker ?? throw new InvalidOperationException("Ricalcolare il dominio con 3D Failure Domain Check di ReBear 0.3.");
            var forces = ReadList<ResultBeamForces>(d, 1); string id = Read(d, 2, "Section");
            var results = new List<CheckResult>(); var resistance = new List<GH_Point>(); var utilization = new List<double>();
            for (int i = 0; i < forces.Count; i++)
            {
                var f = forces[i]; string name = LoadEnvelopes.Name(f, i);
                SectionResistanceResponse point = null;
                var r = SectionChecks.Try(id, name, "N-Mx-My", () =>
                {
                    LoadEnvelopes.Validate(f);
                    point = ResistanceChecks.Evaluate(checker, f);
                    return SectionChecks.Resistance(point, checker, f, id, i);
                }) with { Case = name };
                results.Add(r);
                resistance.Add(new GH_Point(point is { N: not null, M1: not null, M2: not null } && !point.IsAxialForceOutsideDomain
                    ? new Point3d(point.M1.Value / 1e6, point.M2.Value / 1e6, point.N.Value / 1000) : Point3d.Unset));
                utilization.Add(Number(r.Ratio));
                if (r.Status == CheckStatus.NotEvaluated || point != null && (point.IsAxialForceOutsideDomain || point.IsZeroDemand || point.IsZeroMomentDemand))
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"{name}: {(r.Status == CheckStatus.NotEvaluated ? r.Message : point.Diagnostics.Message)}");
            }
            d.SetDataList(0, results.Select(r => new GH_CheckResult(r)));
            d.SetDataList(1, forces.Select(f => new Point3d(f.M1 / 1e6, f.M2 / 1e6, f.N / 1000)));
            d.SetDataList(2, resistance);
            d.SetDataList(3, utilization);
        }
    }

    public sealed class FailureDomainSummaryComponent : ReBearComponent
    {
        public FailureDomainSummaryComponent() : base("Section Resistance Summary", "SRS",
            "Momenti resistenti plastici ed elastici a N costante attorno agli assi globali X/Y per il baricentro; kNm con segno, strategia Iterative.") { }
        public override Guid ComponentGuid => new("0a07f98a-2c47-4bc6-84c3-5833a4f6b65a");
        protected override Port[] Inputs => new[] { G("Reinforced Concrete Section", "RCS", "Sezione GPC."), G("Standard", "S", "Norma GPC."),
            N("Axial Force [kN]", "N [kN]", "Sforzo normale costante, kN, compressione negativa.", 0) };
        protected override Port[] Outputs => new[] {
            N("MxRd+ pl [kNm]", "MxRd+ pl [kNm]", "Plastico, Mx positivo globale."), N("MyRd+ pl [kNm]", "MyRd+ pl [kNm]", "Plastico, My positivo globale."),
            N("MxRd- pl [kNm]", "MxRd- pl [kNm]", "Plastico, Mx negativo globale."), N("MyRd- pl [kNm]", "MyRd- pl [kNm]", "Plastico, My negativo globale."),
            N("MxRd+ el [kNm]", "MxRd+ el [kNm]", "Elastico, Mx positivo globale."), N("MyRd+ el [kNm]", "MyRd+ el [kNm]", "Elastico, My positivo globale."),
            N("MxRd- el [kNm]", "MxRd- el [kNm]", "Elastico, Mx negativo globale."), N("MyRd- el [kNm]", "MyRd- el [kNm]", "Elastico, My negativo globale.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var values = ResistanceChecks.Summary(Required<ReinforcedConcreteSection>(d, 0), Required<StandardModelCode2010>(d, 1), Finite(d, 2)).Values;
            for (int i = 0; i < values.Length; i++) d.SetData(i, values[i]);
        }
    }
}
