using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ServiceabilityStressCheckComponent : GpcComponent
    {
        protected Dictionary<int, string> _serviceabilityLimitState = new Dictionary<int, string>() {
            { 0, "Quasi-Permanent" },
            { 1, "Frequent" },
            { 2, "Characteristic" },
        };

        public ServiceabilityStressCheckComponent()
            : base("Serviceability Stress Check", "SSC", "Serviceability Stress Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis Results", "SA", "Stress Analysis Results", GH_ParamAccess.item);
            int i = pManager.AddIntegerParameter("Serviceability Limit State", "SLS", "Serviceability Limit State", GH_ParamAccess.item);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _serviceabilityLimitState)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddPointParameter("Concrete Points", "CP", "Concrete Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Concrete Values", "CV", "Concrete Values", GH_ParamAccess.list);
            pManager.AddNumberParameter("Concrete Working Ratio", "CWR", "Concrete Working Ratio", GH_ParamAccess.list);
            pManager.AddPointParameter("Rebar Points", "RP", "Rebar Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rebar Values", "RV", "Rebar Values", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rebar Working Ratio", "RWR", "Rebar Working Ratio", GH_ParamAccess.list);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_StressAnalysisResult stress = null;
            int combination = 0;
            if (!DA.GetData(0, ref stress) || !DA.GetData(1, ref combination)) return;
            var r = ConcreteSectionChecker.Adapters.ServiceabilityChecks.Stress(stress.Value, combination);
            var response = GPC.Checkers.Concrete.Analysis.LegacySectionCalculation.Capture(stress.Value);
            DA.SetData(0, "Combinazione;SigmaCmin[MPa];LimiteC[MPa];WRC;SigmaSmax[MPa];WRS;WR;Esito");
            DA.SetData(1, GpcReports.Row(r.Combination, r.ConcreteMinStress, r.ConcreteLimit,
                r.ConcreteRatio, r.SteelMaxStress, r.SteelRatio, r.Ratio, r.Satisfied));
            DA.SetDataList(2, response.Concrete.Select(p => new Point3d(p.X, p.Y, 0)));
            DA.SetDataList(3, response.Concrete.Select(p => p.Stress));
            DA.SetDataList(4, response.Concrete.Select(p => r.ConcreteLimit.HasValue
                ? r.ConcretePoints.FirstOrDefault(c => c.Id == p.Id)?.Ratio ?? 0 : double.NaN));
            DA.SetDataList(5, response.Bars.Select(p => new Point3d(p.X, p.Y, 0)));
            DA.SetDataList(6, response.Bars.Select(p => p.Stress));
            DA.SetDataList(7, response.Bars.Select(p => r.SteelPoints.FirstOrDefault(b => b.Id == p.Id)?.Ratio ?? double.NaN));
            if (!r.Satisfied.HasValue) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Per questa combinazione GPC non prevede limiti tensionali; i rapporti non applicabili sono NaN.");
            else if (!r.Satisfied.Value) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Limiti tensionali SLE superati.");
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("4cdbe859-b9a3-42d1-87d4-42c24647fc68");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
