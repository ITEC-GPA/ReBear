using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class RebarMaterialComponent : GpcComponent
    {
        protected Dictionary<int, string> _steelList = new Dictionary<int, string>() {
            { 0, "B450A" },
            { 1, "B450C" },
            { 2, "B500A" },
            { 3, "B500B" },
            { 4, "B500C" },
        };

        protected Dictionary<int, string> _hardeningType = new Dictionary<int, string>() {
            { 0, "Elasto-Plastic" },
            { 1, "Elasto-Hardening" },
        };

        public RebarMaterialComponent()
            : base("Database Rebar Steel", "RS", "Database Rebar Steel", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER   )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Type", "T", "The property type", GH_ParamAccess.item, 1);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _steelList)
                mtParam.AddNamedValue(v.Value, v.Key);
            int j = pManager.AddIntegerParameter("Behaviour", "B", "The behaviour type", GH_ParamAccess.item, 0);
            Param_Integer mtParamj = pManager[j] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _hardeningType)
                mtParamj.AddNamedValue(v.Value, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Rebar Steel Material", "RS", "Rebar Steel Material", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            int type = 0;
            int hard = 0;

            if (DA.GetData(0, ref type) && DA.GetData(1, ref hard))
            {
                if (!_steelList.ContainsKey(type))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid type");
                    return;
                }
                SteelMaterialEN1992 steelMaterialEN1992 = null;
                if (type == 0)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.B450A;
                else if (type == 1)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.B450C;
                else if (type == 2)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.B500A;
                else if (type == 3)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.B500B;
                else if (type == 4)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.B500C;
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get data");
                    return;
                }

                if(hard  == 1)
                    steelMaterialEN1992.StressStrainCurve = SteelMaterial.StressStrainCurveType.ElasticHardening;

                DA.SetData(0, new GH_SteelMaterial(steelMaterialEN1992));
            }
        }

        //protected override Bitmap Icon => Resources.material;
        
        public override Guid ComponentGuid => new Guid("606ebfd0-d9e7-407e-aaa3-eb8579fe2f6b");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
