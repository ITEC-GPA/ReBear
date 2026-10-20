using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class RebarComponent : GpcComponent
    {
        protected Dictionary<int, int> _diameterList = new Dictionary<int, int>() {
            { 0, 6 },
            { 1, 8 },
            { 2, 10 },
            { 3, 12 },
            { 4, 14 },
            { 5, 16 },
            { 6, 18 },
            { 7, 20 },
            { 8, 22 },
            { 9, 24 },
            { 10, 26 },
            { 11, 28 },
            { 12, 30 },
            { 13, 32 },
        };

        public RebarComponent()
            : base("Rebar Element", "RE", "Rebar Element", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER   )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "P", "The rebar position", GH_ParamAccess.item);
            int i = pManager.AddIntegerParameter("Diamater", "D", "The rebar diameter", GH_ParamAccess.item);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, int> v in _diameterList)
                mtParam.AddNamedValue(v.Value.ToString(), v.Key);
            pManager.AddGenericParameter("Rebar Material", "M", "The rebar material", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Rebar Element", "RE", "Rebar Element", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            int type = 0;
            GH_SteelMaterial rebarMaterial = null;
            Rhino.Geometry.Point3d location = Rhino.Geometry.Point3d.Unset;

            if (DA.GetData(1, ref type) && DA.GetData(2, ref rebarMaterial) && DA.GetData(0, ref location))
            {
                if (!_diameterList.ContainsKey(type))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid diameter");
                    return;
                }

                int diameter = _diameterList[type]; 
                RebarSectionCircular rebarSection = rebarSection = new RebarSectionCircular(diameter, rebarMaterial.Value);
                GPC.Geometry.Point2d rebarPosition = new GPC.Geometry.Point2d(location.X, location.Y);

                ReinforcedConcreteRebar reinforcedConcreteRebar = new ReinforcedConcreteRebar(rebarSection, rebarPosition);
                
                DA.SetData(0, new GH_Rebar(reinforcedConcreteRebar));
            }
        }

        //protected override Bitmap Icon => Resources.material;
        
        public override Guid ComponentGuid => new Guid("411284c2-923a-4475-94a5-8405ece71ce0");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
