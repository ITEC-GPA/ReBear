using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Results;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class SectionForceComponent : GpcComponent
    {
        protected Dictionary<int, string> _concreteList = new Dictionary<int, string>() {
            { 0, "Nmm" },
            { 1, "kNm" },
        };

        /// <summary>
        /// Initializes a new instance of the ForceComponent class.
        /// </summary>
        public SectionForceComponent()
          : base("Force component", "FC", "Create a force object", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "The force unique name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Axial Force", "N", "The beam element forces", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Shear X", "VX", "The beam element forces", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Shear Y", "VY", "The beam element forces", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Torsion", "T", "The beam element forces", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Bending X", "MX", "The beam element forces", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Bending Y", "MY", "The beam element forces", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("Coordinate System", "CS", "The Analysis Coordinate System", GH_ParamAccess.item);
            int i = pManager.AddIntegerParameter("Units", "U", "The Force Unit system", GH_ParamAccess.item, 1);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _concreteList)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Force", "F", "The force", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveGpc(IGH_DataAccess DA)
        {
            string name = "";
            double N = 0;
            double V2 = 0;
            double V3 = 0;
            double T = 0;
            double M2 = 0;
            double M3 = 0;
            GH_CoordinateSystem gH_CoordinateSystem = null;
            int units = 0;

            int nn = 0;
            if (!DA.GetData(nn++, ref name))
                return;
            if (!DA.GetData(nn++, ref N))
                return;
            if (!DA.GetData(nn++, ref V2))
                return;
            if (!DA.GetData(nn++, ref V3))
                return;
            if (!DA.GetData(nn++, ref T))
                return;
            if (!DA.GetData(nn++, ref M2))
                return;
            if (!DA.GetData(nn++, ref M3))
                return;
            if (!DA.GetData(nn++, ref gH_CoordinateSystem) || !DA.GetData(nn++, ref units)) return;
            if (gH_CoordinateSystem?.Value is null) throw new ArgumentException("Sistema di riferimento delle forze non valido.");
            if (units != 0 && units != 1) throw new ArgumentOutOfRangeException(nameof(units), "Unità: 0 = N, Nmm; 1 = kN, kNm.");
            foreach (double value in new[] { N, V2, V3, T, M2, M3 })
                if (!double.IsFinite(value)) throw new ArgumentException("Le azioni devono essere valori finiti.");

            ResultBeamForces beamForceResultModel = null;
            if (units == 0)
                beamForceResultModel = new ResultBeamForces(N, V2, V3, T, M2, M3, gH_CoordinateSystem.Value, -1, name);
            else
                beamForceResultModel = new ResultBeamForces(N * 1000, V2 * 1000, V3 * 1000, T * 1000000, M2 * 1000000, M3 * 1000000, gH_CoordinateSystem.Value, -1, name);

            DA.SetData(0, new GH_ResultBeamForces(beamForceResultModel));
        }

        //protected override System.Drawing.Bitmap Icon => Properties.Resources.BeamResultsIcon;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ddd3c603-f931-4db5-ab11-6d1da9ffd01e");
    }
}
