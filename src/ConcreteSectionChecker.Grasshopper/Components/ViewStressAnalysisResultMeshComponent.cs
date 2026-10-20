using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ViewStressAnalysisResultMeshComponent : GpcComponent
    {
        public ViewStressAnalysisResultMeshComponent()
            : base("Inspect Strain Analysis Results", "ISAR", "Strain Analysis Results")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis Results", "SA", "Stress Analysis Results", GH_ParamAccess.item);
            pManager.AddPointParameter("Concrete Points", "CP", "Concrete Points", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Concrete Stress", "CS", "Concrete Stress", GH_ParamAccess.list);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_StressAnalysisResult gH_StressAnalysisResult = null;
            List<GH_Point> concretePoints = new List<GH_Point>();

            if (DA.GetData(0, ref gH_StressAnalysisResult) && DA.GetDataList(1, concretePoints))
            {
                ReinforcedConcreteSection section = (ReinforcedConcreteSection)gH_StressAnalysisResult.Value.ConcreteSection;
                GPC.Checkers.Concrete.Results.StressAnalysisResult result = gH_StressAnalysisResult.Value;

                List<double> concreteStress = new List<double>();
                for (int i = 0; i < concretePoints.Count; i++)
                {
                    GPC.Geometry.Point2d point = new GPC.Geometry.Point2d(concretePoints[i].Value.X, concretePoints[i].Value.Y);
                    if (result.LinearElasticAnalysis)
                        concreteStress.Add(result.GetConcreteTension(result.PsiRebar.Value, point));
                    else
                        concreteStress.Add(result.GetConcreteTension(point));
                }
                DA.SetDataList(0, concreteStress);
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("8ae4225f-e39f-4304-bbdb-2610c34dd354");

    }
}
