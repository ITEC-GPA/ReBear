using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using static GPC.Checkers.Concrete.Checkers.SectionCheckerModelCode2010;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class HomogenizedMechanicalPropertiesComponent : GpcComponent
    {
        public HomogenizedMechanicalPropertiesComponent()
            : base("Homogenized Mechanical Properties", "HP", "Homogenized Mechanical Properties")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Reinforced Concrete Section", "RCS", "The Reinforced Concrete Section", GH_ParamAccess.item);
            pManager.AddNumberParameter("ψ factor", "ψ", "ψ factor", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Area", "A", "Area", GH_ParamAccess.item);
            pManager.AddNumberParameter("Sx", "Sx", "Sx", GH_ParamAccess.item);
            pManager.AddNumberParameter("Sy", "Sy", "Sy", GH_ParamAccess.item);
            pManager.AddNumberParameter("Jxx", "Jxx", "Jxx", GH_ParamAccess.item);
            pManager.AddNumberParameter("Jyy", "Jyy", "Jyy", GH_ParamAccess.item);
            pManager.AddNumberParameter("Jxy", "Jxy", "Jxy", GH_ParamAccess.item);
            pManager.AddNumberParameter("Jp", "Jp", "Jp", GH_ParamAccess.item);
            pManager.AddNumberParameter("J11", "J11", "J11", GH_ParamAccess.item);
            pManager.AddNumberParameter("J22", "J22", "J22", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "Area", GH_ParamAccess.item);
            pManager.AddPointParameter("Centroid", "C", "Centroid", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_ReinforcedConcreteSection gH_ReinforcedConcreteSection = null;
            double psifactor = 0;

            if (DA.GetData(0, ref gH_ReinforcedConcreteSection) && DA.GetData(1, ref psifactor))
            {
                var prop = gH_ReinforcedConcreteSection.Value.GetHomogeneizedMechanicalProperties(psifactor);
                DA.SetData(0, gH_ReinforcedConcreteSection.Value.Name);
                DA.SetData(1, prop.areaH);
                DA.SetData(2, prop.SxH);
                DA.SetData(3, prop.SyH);
                DA.SetData(4, prop.JxxH);   
                DA.SetData(5, prop.JyyH);
                DA.SetData(6, prop.JxyH);
                DA.SetData(7, prop.JpH);
                DA.SetData(8, prop.J11H);
                DA.SetData(9, prop.J22H);
                DA.SetData(10, prop.angleX);
                DA.SetData(11, new Point3d(prop.centroidH.X, prop.centroidH.Y, 0));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("c9e7fd77-486c-416e-987b-cf6521e649fd");

    }
}
