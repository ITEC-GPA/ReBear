using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Results;
using Grasshopper.Kernel;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class StressAnalysisComponent : GpcComponent
    {
        public StressAnalysisComponent()
            : base("Stress Analysis", "SA", "Stress Analysis")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
            pManager.AddGenericParameter("Forces", "F", "Forces", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis Results", "SAR", "Stress Analysis Results", GH_ParamAccess.list);
            pManager.AddGenericParameter("Strain Plane Results", "SPR", "Strain Plane Results", GH_ParamAccess.list);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_SectionChecker gH_SectionChecker = null;
            List<GH_ResultBeamForces> gH_ResultBeamForces = new List<GH_ResultBeamForces>();

            if (DA.GetData(0, ref gH_SectionChecker) && DA.GetDataList(1, gH_ResultBeamForces))
            {
                ResultBeamForces[] resultBeamForces = new ResultBeamForces[gH_ResultBeamForces.Count];
                for (int i = 0; i < gH_ResultBeamForces.Count; i++)
                    resultBeamForces[i] = new ResultBeamForces(gH_ResultBeamForces[i].Value.N, gH_ResultBeamForces[i].Value.V1,
                        gH_ResultBeamForces[i].Value.V2, gH_ResultBeamForces[i].Value.T, gH_ResultBeamForces[i].Value.M1, gH_ResultBeamForces[i].Value.M2,
                        gH_ResultBeamForces[i].Value.CoordinateSystem, i, gH_ResultBeamForces[i].Value.Name);

                SectionCheckerAttribute copy = new SectionCheckerAttribute(gH_SectionChecker.Value.SectionCheckerAttribute.Section, resultBeamForces, null);
                SectionCheckerModelCode2010 checkerCopy = new SectionCheckerModelCode2010(copy, gH_SectionChecker.Value.SectionCheckerOptionsModelCode2010, gH_SectionChecker.Value.StandardModelCode2010);
                GPC.Checkers.Concrete.Results.StressAnalysisResult[] result = checkerCopy.GetTensionAnalysisResult();
                GPC.Checker.Results.ResultType.StrainPlaneResult[] stressPlaneResult = new GPC.Checker.Results.ResultType.StrainPlaneResult[result.Length];

                for (int i = 0; i < result.Length; i++)
                {
                    var response = GPC.Checkers.Concrete.Analysis.LegacySectionCalculation.Capture(result[i]);
                    if (response.Diagnostics.Status != GPC.Checkers.Concrete.Analysis.CalculationStatus.Completed)
                        throw new InvalidOperationException("Analisi " + i + ": " + response.Diagnostics.Message);
                    if (result[i].LinearElasticAnalysis)
                        stressPlaneResult[i] = result[i].CalculateStrainPlaneResult(result[i].LinearElasticAnalysis, result[i].PsiRebar != null ? result[i].PsiRebar.Value : 0,
                            result[i].PsiTendon != null ? result[i].PsiTendon.Value : 0);
                    else
                        stressPlaneResult[i] = result[i].CalculateStrainPlaneResult();
                }

                DA.SetDataList(0, result.Select(r => new GH_StressAnalysisResult(r)).ToList());
                DA.SetDataList(1, stressPlaneResult.Select(r => new GH_StrainPlaneResult(r)).ToList());
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("2c7939e3-5f53-4d53-989b-a56b65b773e4");

    }
}
