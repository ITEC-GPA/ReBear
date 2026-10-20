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
    public class ConcreteMechanicalPropertiesComponent : GpcComponent
    {
        public ConcreteMechanicalPropertiesComponent()
            : base("Concrete Mechanical Properties", "CP", "Concrete Mechanical Properties")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Reinforced Concrete Section", "RCS", "The Reinforced Concrete Section", GH_ParamAccess.item);
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

            if (DA.GetData(0, ref gH_ReinforcedConcreteSection))
            {
                var staticMoments = gH_ReinforcedConcreteSection.Value.CalculateStaticMoments();
                DA.SetData(0, gH_ReinforcedConcreteSection.Value.Name);
                DA.SetData(1, gH_ReinforcedConcreteSection.Value.Area);
                DA.SetData(2, staticMoments.Sx);
                DA.SetData(3, staticMoments.Sy);
                DA.SetData(4, gH_ReinforcedConcreteSection.Value.Jxx);   
                DA.SetData(5, gH_ReinforcedConcreteSection.Value.Jyy);
                DA.SetData(6, gH_ReinforcedConcreteSection.Value.Jxy);
                DA.SetData(7, gH_ReinforcedConcreteSection.Value.Jp);
                DA.SetData(8, gH_ReinforcedConcreteSection.Value.J11);
                DA.SetData(9, gH_ReinforcedConcreteSection.Value.J22);
                DA.SetData(10, gH_ReinforcedConcreteSection.Value.AngleX1);
                DA.SetData(11, new Point3d(gH_ReinforcedConcreteSection.Value.Centroid.X, gH_ReinforcedConcreteSection.Value.Centroid.Y, 0));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("aba32c4e-70fa-4e76-bef9-bb02214a1279");

    }
}
