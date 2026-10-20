using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Results;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class PostProcessFailureDomain3DComponent : GpcComponent
    {
        public PostProcessFailureDomain3DComponent()
            : base("3D Failure Domain Analysis", "FDA", "3D Failure domain Analysis", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("3D Failure Domain Analysis", "FDA", "3D Failure Domain Analysis", GH_ParamAccess.item);
            pManager.AddGenericParameter("Forces", "F", "Forces", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.list);
            pManager.AddPointParameter("External Force", "EF", "External Force", GH_ParamAccess.list);
            pManager.AddPointParameter("Resistance Force", "RF", "Resistance Force", GH_ParamAccess.list);
            pManager.AddNumberParameter("Working Ratio", "wR", "Working Ratio", GH_ParamAccess.list);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_FailureDomain3DResult gH_FailureDomain3DResult = null;
            List<GH_ResultBeamForces> gH_ResultBeamForces = new List<GH_ResultBeamForces>();

            if (DA.GetData(0, ref gH_FailureDomain3DResult) && DA.GetDataList(1, gH_ResultBeamForces))
            {
                ResultBeamForces[] resultBeamForces = new ResultBeamForces[gH_ResultBeamForces.Count];
                List<Point3d> externalPointsRhino = new List<Point3d>();
                string[] toExcelBuffer = new string[gH_ResultBeamForces.Count];
                string ToExcelHeader = "Id;Name;N [kN];Vx [kN];Vy [kN];T [kNm];Mx [kNm];My [kNm];NRd [kN];MxRd [kNm];MyRd [kNm];WRm;WRmLim";

                for (int i = 0; i < gH_ResultBeamForces.Count; i++)
                {
                    resultBeamForces[i] = new ResultBeamForces(gH_ResultBeamForces[i].Value.N, gH_ResultBeamForces[i].Value.V1,
                        gH_ResultBeamForces[i].Value.V2, gH_ResultBeamForces[i].Value.T, gH_ResultBeamForces[i].Value.M1, gH_ResultBeamForces[i].Value.M2,
                        gH_ResultBeamForces[i].Value.CoordinateSystem, i, gH_ResultBeamForces[i].Value.Name);
                    externalPointsRhino.Add(new Point3d(gH_ResultBeamForces[i].Value.M1 / 1000000.0, gH_ResultBeamForces[i].Value.M2 / 1000000.0, gH_ResultBeamForces[i].Value.N / 1000.0));
                    toExcelBuffer[i] = $"{i};{gH_ResultBeamForces[i].Value.Name};{gH_ResultBeamForces[i].Value.N / 1000};{gH_ResultBeamForces[i].Value.V1 / 1000};{gH_ResultBeamForces[i].Value.V2 / 1000};" +
                        $"{gH_ResultBeamForces[i].Value.T / 1000000};{gH_ResultBeamForces[i].Value.M1 / 1000000};{gH_ResultBeamForces[i].Value.M2 / 1000000};";
                }

                var checker = gH_FailureDomain3DResult.Checker
                    ?? throw new InvalidOperationException("Ricalcolare il dominio con il componente 3D Failure Domain Check di questo plugin.");
                var points = resultBeamForces.Select(f => ConcreteSectionChecker.Adapters.ResistanceChecks.Evaluate(checker, f)).ToArray();
                GH_Point[] resistancePointsRhino = new GH_Point[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    resistancePointsRhino[i] = new GH_Point(points[i].N.HasValue && points[i].M1.HasValue && points[i].M2.HasValue
                        ? new Point3d(points[i].M1.Value / 1000000, points[i].M2.Value / 1000000, points[i].N.Value / 1000)
                        : Point3d.Unset);
                    toExcelBuffer[i] = string.Concat(toExcelBuffer[i], GpcReports.Row(points[i].N / 1000, points[i].M1 / 1000000, points[i].M2 / 1000000), ";");
                    if (points[i].IsAxialForceOutsideDomain || points[i].IsZeroDemand || points[i].IsZeroMomentDemand)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Forza {i}: {points[i].Diagnostics.Message}");
                }
                for (int i = 0; i < points.Length; i++)
                {
                    toExcelBuffer[i] = string.Concat(toExcelBuffer[i], GpcReports.Row(points[i].Utilization), ";");
                    toExcelBuffer[i] = string.Concat(toExcelBuffer[i], "1.0");
                }

                double[] wr = points.Select(p => GpcReports.Number(p.Utilization)).ToArray();

                DA.SetData(0, ToExcelHeader);
                DA.SetDataList(1, toExcelBuffer);
                DA.SetDataList(2, externalPointsRhino);
                DA.SetDataList(3, resistancePointsRhino);
                DA.SetDataList(4, wr);
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("96102691-a47f-4cb5-a411-db638040b01e");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
