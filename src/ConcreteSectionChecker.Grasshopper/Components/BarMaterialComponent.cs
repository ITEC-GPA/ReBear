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
    public class BarMaterialComponent : GpcComponent
    {
        protected Dictionary<int, string> _steelList = new Dictionary<int, string>() {
            { 0, "Y1180C" },
            { 1, "Y1240C" },
            { 2, "Y1280C" },
            { 3, "Y1370C" },
            { 4, "Y1420C" },
            { 5, "Y1470C" },
        };

        protected Dictionary<int, string> _hardeningType = new Dictionary<int, string>() {
            { 0, "Elasto-Plastic" },
            { 1, "Elasto-Hardening" },
        };

        public BarMaterialComponent()
            : base("Database Bar Steel", "DBS", "Database Bar Steel")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Type", "T", "The property type", GH_ParamAccess.item, 0);
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
            pManager.AddGenericParameter("Bar Steel Material", "BS", "Bar Steel Material", GH_ParamAccess.item);
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
                    steelMaterialEN1992 = SteelMaterialEN1992Data.Y1180C;
                else if (type == 1)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.Y1240C;
                else if (type == 2)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.Y1280C;
                else if (type == 3)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.Y1370C;
                else if (type == 4)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.Y1420C;
                else if (type == 5)
                    steelMaterialEN1992 = SteelMaterialEN1992Data.Y1470C;
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get data");
                    return;                    
                }
                if (hard == 1)
                    steelMaterialEN1992.StressStrainCurve = SteelMaterial.StressStrainCurveType.ElasticHardening;

                DA.SetData(0, new GH_SteelMaterial(steelMaterialEN1992));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("23875d38-d43a-4051-b19c-a6ae39139f7b");

    }
}
