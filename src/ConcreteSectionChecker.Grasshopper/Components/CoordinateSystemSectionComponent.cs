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
    public class CoordinateSystemSectionComponent : GpcComponent
    {
        public CoordinateSystemSectionComponent()
            : base("Secton Coordinate System", "CS", "The Section Principal Coordinate System")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Section", "C", "The Reinforced Concrete Section", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Coordinate System", "CS", "The Coordinate System", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_ReinforcedConcreteSection gH_ReinforcedConcreteSection = null;

            if (DA.GetData(0, ref gH_ReinforcedConcreteSection))
            {
                CoordinateSystem coordinateSystem = null;
                if (gH_ReinforcedConcreteSection.Value.IsCompositeSteelConcrete)
                    coordinateSystem = new CoordinateSystem(gH_ReinforcedConcreteSection.Value.GetHomogenizedCentroid(out double _, out double _), new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0));
                else
                    coordinateSystem = new CoordinateSystem(gH_ReinforcedConcreteSection.Value.Centroid, new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0));
                DA.SetData(0, new GH_CoordinateSystem(coordinateSystem));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("c84a4593-64c3-44b1-b663-6482d43b820b");

    }
}
