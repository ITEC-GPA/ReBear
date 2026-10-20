using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Standards;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class StandardComponent : GpcComponent
    {
        protected Dictionary<int, string> _standardList = new Dictionary<int, string>() {
            { 0, "NTC2018" },
            { 1, "EN 1992-1-1" },
            { 2, "UNI EN 1992-1-1" },
        };

        public StandardComponent()
            : base("Standard", "S", "Standard", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Type", "T", "The standard", GH_ParamAccess.item, 0);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _standardList)
                mtParam.AddNamedValue(v.Value.ToString(), v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Standard", "S", "Standard", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            int type = 0;

            if (DA.GetData(0, ref type))
            {
                if (!_standardList.ContainsKey(type))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid standard");
                    return;
                }

                if (type == 0)
                    DA.SetData(0, new GH_Standard(new StandardNTC2018Concrete()));
                else if (type == 1)
                    DA.SetData(0, new GH_Standard(new StandardEN1992p11()));
                else if (type == 2)
                    DA.SetData(0, new GH_Standard(new StandardUNIEN1992p11()));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("3def8fa3-e056-4075-8661-bd51575c00ee");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
