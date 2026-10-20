using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ConcreteMaterialComponent : GpcComponent
    {
        protected Dictionary<int, string> _concreteList = new Dictionary<int, string>() {
            { 0, "C20/25" },
            { 1, "C25/30" },
            { 2, "C28/35" },
            { 3, "C30/37" },
            { 4, "C32/40" },
            { 5, "C35/45" },
            { 6, "C40/50" },
            { 7, "C45/55" },
            { 8, "C50/60" },
            { 9, "C55/67" },
            { 10, "C60/75" },
            { 11, "C70/85" },
            { 12, "C80/90" },
            { 13, "C90/105" },
        };

        public ConcreteMaterialComponent()
            : base("Database Concrete", "DCM", "Database Concrete")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Type", "T", "The property type", GH_ParamAccess.item, 1);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _concreteList)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Material", "CM", "Concrete Material", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            int type = 0;

            if (DA.GetData(0, ref type))
            {
                if (!_concreteList.ContainsKey(type))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid type");
                    return;
                }
                if (type == 0)
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C20_25));
                else if (type == 1)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C25_30));
                else if (type == 2)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C28_35));
                else if (type == 3)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C30_37));
                else if (type == 4)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C32_40));
                else if (type == 5)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C35_45));
                else if (type == 6)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C40_50));
                else if (type == 7)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C45_55));
                else if (type == 8)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C50_60));
                else if (type == 9)                                     
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C55_67));
                else if (type == 10)                                    
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C60_75));
                else if (type == 11)                                    
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C70_85));
                else if (type == 12)                                    
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C80_95));
                else if (type == 13)                                    
                    DA.SetData(0, new GH_ConcreteMaterial(ConcreteMaterialEN1992Data.C90_105));
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get data");
                    return;
                }
            }
        }

        //protected override Bitmap Icon => Resources.material;
        
        public override Guid ComponentGuid => new Guid("e2aa1e8b-3fa9-49a5-8d5e-991d4eac5bc6");

    }
}
