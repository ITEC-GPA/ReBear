using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class CircularSectionShearCheckComponent : GpcComponent
    {
        public CircularSectionShearCheckComponent()
            : base("Circular Section Shear Check", "SC", "Shear Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force", "F", "Force", GH_ParamAccess.item);
            pManager.AddNumberParameter("Diameter", "D", "Diameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup Diameter", "SD", "Stirrup Diameter", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Legs", "NL", "Number of Legs of a stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Spacing", "S", "Spacing", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup slope", "SS", "Stirrup slope", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitudinal rebar diameter", "LRD", "Longitudinal rebar diameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Cover", "CC", "Concrete Cover", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd", "Vrsd", "Vrsd", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd", "Vrcd", "Vrcd", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd", "Vrd", "Vrd", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio", "WR", "Working Ratio", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_SectionChecker checker = null;
            GH_ResultBeamForces force = null;
            double diameter = 0, link = 0, spacing = 0, alpha = 0, bar = 0, cover = 0;
            int legs = 0;
            if (!DA.GetData(0, ref checker) || !DA.GetData(1, ref force) || !DA.GetData(2, ref diameter)
                || !DA.GetData(3, ref link) || !DA.GetData(4, ref legs) || !DA.GetData(5, ref spacing)
                || !DA.GetData(6, ref alpha) || !DA.GetData(7, ref bar) || !DA.GetData(8, ref cover)) return;
            var r = ConcreteSectionChecker.Adapters.ShearChecks.Circular(checker.Value, force.Value,
                diameter, link, legs, spacing, alpha, bar, cover);
            GpcReports.Shear(this, DA, ("Circolare", r));
            DA.SetData(2, GpcReports.Resistance(r, x => x.VRsd));
            DA.SetData(3, GpcReports.Resistance(r, x => x.VRcd));
            DA.SetData(4, GpcReports.Resistance(r, x => x.VRd));
            DA.SetData(5, GpcReports.Number(r.Ratio));
        }


        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("fdb8a104-8595-4b8f-9d1a-1ac36a664488");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
