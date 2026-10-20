using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Geometry;
using GPC.Model.Standards;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class CoordinateSystemComponent : GpcComponent
    {
        public CoordinateSystemComponent()
            : base("Coordinate System", "CS", "The Coordinate System")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The Coordiante System Name", GH_ParamAccess.item, "");
            pManager.AddPointParameter("Origin", "O", "The Coordiante System Origin", GH_ParamAccess.item);
            pManager.AddPointParameter("P1", "P1", "The Coordiante System X axis", GH_ParamAccess.item);
            pManager.AddPointParameter("P2", "P2", "The Coordiante System Y axis", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Coordinate System", "CS", "The Coordinate System", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            string name = string.Empty;
            Rhino.Geometry.Point3d origin = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Point3d p1 = Rhino.Geometry.Point3d.Unset;
            Rhino.Geometry.Point3d p2 = Rhino.Geometry.Point3d.Unset;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref origin) && DA.GetData(2, ref p1) && DA.GetData(3, ref p2))
            {
                CoordinateSystem coordinateSystem = new CoordinateSystem(new Point3d(origin.X, origin.Y, origin.Z), new Point3d(p1.X, p1.Y, p1.Z), new Point3d(p2.X, p2.Y, p2.Z), name);
                DA.SetData(0, new GH_CoordinateSystem(coordinateSystem));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("8f839216-244a-4891-a1d5-bb0fb42f7909");

    }
}
