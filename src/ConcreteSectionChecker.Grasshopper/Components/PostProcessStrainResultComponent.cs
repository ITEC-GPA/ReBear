using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using Grasshopper.Kernel;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class PostProcessStrainResultComponent : GpcComponent
    {
        public PostProcessStrainResultComponent()
            : base("Print Stress Analysis Results", "ISAR", "Print Stress Analysis Results")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis", "SA", "Stress Analysis", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Id", "Id", "Id", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("N[kN]", "N[kN]", "N[kN]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mx[kNm]", "Mx[kNm]", "Mx[kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("My[kNm]", "My[kNm]", "My[kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("σc min[Mpa]", "σc min[Mpa]", "σc min[Mpa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("σc max[Mpa]", "σc max[Mpa]", "σc max[Mpa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("σs min[Mpa]", "σs min[Mpa]", "σs min[Mpa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("σs max[Mpa]", "σs max[Mpa]", "σs max[Mpa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("σsp min[Mpa]", "σsp min[Mpa]", "σsp min[Mpa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("σsp max[Mpa]", "σsp max[Mpa]", "σsp max[Mpa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("σss min[Mpa]", "σss min[Mpa]", "σss min[Mpa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("σss max[Mpa]", "σss max[Mpa]", "σss max[Mpa]", GH_ParamAccess.item);
            pManager.AddNumberParameter("εc min", "εc min", "εc min", GH_ParamAccess.item);
            pManager.AddNumberParameter("εc max", "εc max", "εc max", GH_ParamAccess.item);
            pManager.AddNumberParameter("εs min", "εs min", "εs min", GH_ParamAccess.item);
            pManager.AddNumberParameter("εs max", "εs max", "εs max", GH_ParamAccess.item);
            pManager.AddNumberParameter("εsp min", "εsp min", "εsp min", GH_ParamAccess.item);
            pManager.AddNumberParameter("εsp max", "εsp max", "εsp max", GH_ParamAccess.item);
            pManager.AddNumberParameter("εss min", "εss min", "εss min", GH_ParamAccess.item);
            pManager.AddNumberParameter("εss max", "εss max", "εss max", GH_ParamAccess.item);
            pManager.AddNumberParameter("d[mm]", "d[mm]", "d[mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("x[mm]", "x[mm]", "x[mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("θ[°]", "θ[°]", "θ[°]", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_StrainPlaneResult gH_StrainPlaneResult = null;

            if (DA.GetData(0, ref gH_StrainPlaneResult))
            {
                GPC.Checker.Results.ResultType.StrainPlaneResult stressPlaneResult = gH_StrainPlaneResult.Value;

                int cifresignificativeStress = 2;
                int cifresignificativeStrain = 2;
                int cifresignificativeDist = 2;

                List<double> valueToExport = new List<double>()
                {
                    stressPlaneResult.ResultBeamForce.N / 1000.0,
                    stressPlaneResult.ResultBeamForce.M1 / 1000000.0,
                    stressPlaneResult.ResultBeamForce.M2 / 1000000.0,
                    Math.Round(stressPlaneResult.SigmaCMin != double.MaxValue ? stressPlaneResult.SigmaCMin : 0, cifresignificativeStress),
                    Math.Round(stressPlaneResult.SigmaCMax != double.MinValue ? stressPlaneResult.SigmaCMax : 0, cifresignificativeStress),
                    Math.Round(stressPlaneResult.SigmaSMin != double.MaxValue ? stressPlaneResult.SigmaSMin : 0, cifresignificativeStress),
                    Math.Round(stressPlaneResult.SigmaSMax != double.MinValue ? stressPlaneResult.SigmaSMax : 0, cifresignificativeStress),
                    Math.Round(stressPlaneResult.SigmaPMin != double.MaxValue ? stressPlaneResult.SigmaPMin : 0, cifresignificativeStress),
                    Math.Round(stressPlaneResult.SigmaPMax != double.MinValue ? stressPlaneResult.SigmaPMax : 0, cifresignificativeStress),
                    Math.Round(stressPlaneResult.SigmaSSMin != double.MaxValue ? stressPlaneResult.SigmaSSMin : 0, cifresignificativeStress),
                    Math.Round(stressPlaneResult.SigmaSSMax != double.MinValue ? stressPlaneResult.SigmaSSMax : 0, cifresignificativeStress),
                    Math.Round(stressPlaneResult.EpsilonCMin != double.MaxValue ? stressPlaneResult.EpsilonCMin * 1000 : 0, cifresignificativeStrain),
                    Math.Round(stressPlaneResult.EpsilonCMax != double.MinValue ? stressPlaneResult.EpsilonCMax * 1000 : 0, cifresignificativeStrain),
                    Math.Round(stressPlaneResult.EpsilonSMin != double.MaxValue ? stressPlaneResult.EpsilonSMin * 1000 : 0, cifresignificativeStrain),
                    Math.Round(stressPlaneResult.EpsilonSMax != double.MinValue ? stressPlaneResult.EpsilonSMax * 1000 : 0, cifresignificativeStrain),
                    Math.Round(stressPlaneResult.EpsilonPMin != double.MaxValue ? stressPlaneResult.EpsilonPMin * 1000 : 0, cifresignificativeStrain),
                    Math.Round(stressPlaneResult.EpsilonPMax != double.MinValue ? stressPlaneResult.EpsilonPMax * 1000 : 0, cifresignificativeStrain),
                    Math.Round(stressPlaneResult.EpsilonSSMin != double.PositiveInfinity ? stressPlaneResult.EpsilonSSMin * 1000 : 0, cifresignificativeStrain),
                    Math.Round(stressPlaneResult.EpsilonSSMax != double.NegativeInfinity ? stressPlaneResult.EpsilonSSMax * 1000 : 0, cifresignificativeStrain),
                    Math.Round(stressPlaneResult.NetHeight, cifresignificativeDist),
                    Math.Round(stressPlaneResult.NeutralAxisDistance, cifresignificativeDist),
                    Math.Round(stressPlaneResult.NeutralAxisAngle, cifresignificativeDist),
                };


                List<string> j1 =  new List<string> { stressPlaneResult.ResultBeamForce.Id.ToString(), stressPlaneResult.ResultBeamForce.Name };
                j1.AddRange(valueToExport.Select(i => i.ToString()));
                var toExcel = string.Join(";", j1);
                string toExcelHearder = "Id;Name;N [kN];Mx [kNm];My [kNm];σc min [Mpa];σc max [Mpa];σs min [Mpa];σs max [Mpa];σsp min [Mpa];" +
                    "σsp max [Mpa];σss min[Mpa];σss max[Mpa];εc min;εc max;εs min;εs max;εsp min;εsp max;εss min;εss max;d [mm];x [mm];θ [°]";

                var count = 0;
                DA.SetData(count++, toExcelHearder);
                DA.SetData(count++, toExcel);
                DA.SetData(count++, j1[0]);
                DA.SetData(count++, j1[1]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
                DA.SetData(count++, valueToExport[count - 5]);
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("3951b5ca-01a2-4f6c-8acb-1b8a80ed1e65");

    }
}
