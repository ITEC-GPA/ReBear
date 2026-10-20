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
    public class TendonComponent : GpcComponent
    {
        public TendonComponent()
            : base("Tendon Element", "Tendon Element", "Tendon Element")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "P", "The tendon position", GH_ParamAccess.item);
            pManager.AddNumberParameter("Diamater", "D", "The tendon diameter", GH_ParamAccess.item);
            pManager.AddGenericParameter("Tendon Material", "M", "The tendon material", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pretension", "P", "The tendon pretension", GH_ParamAccess.item, 1400);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Tendon Element", "TE", "Tendon Element", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            double diameter = 0;
            GH_SteelMaterial rebarMaterial = null;
            Rhino.Geometry.Point3d location = Rhino.Geometry.Point3d.Unset;
            double pretension = 0;

            if (DA.GetData(1, ref diameter) && DA.GetData(2, ref rebarMaterial) && DA.GetData(0, ref location) && DA.GetData(3, ref pretension))
            {
                RebarSectionCircular rebarSection = rebarSection = new RebarSectionCircular(diameter, rebarMaterial.Value);
                GPC.Geometry.Point2d rebarPosition = new GPC.Geometry.Point2d(location.X, location.Y);

                ReinforcedConcreteRebar reinforcedConcreteRebar = new ReinforcedConcreteRebar(rebarSection, rebarPosition, pretension);
                
                DA.SetData(0, new GH_Rebar(reinforcedConcreteRebar));
            }
        }

        //protected override Bitmap Icon => Resources.material;
        
        public override Guid ComponentGuid => new Guid("e0846aae-ae28-41dc-8773-b5e4232ba6df");

    }
}
