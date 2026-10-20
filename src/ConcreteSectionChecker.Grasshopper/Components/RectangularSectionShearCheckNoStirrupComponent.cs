using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class RectangularSectionShearCheckNoStirrupComponent : GpcComponent
    {
        public RectangularSectionShearCheckNoStirrupComponent()
            : base("Rectangular Section Shear Check No stirrup", "SC", "Shear Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force", "F", "Force", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Base", "B", "Base", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitudinal rebar diameter", "LRD", "Longitudinal rebar diameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number of Rebar", "NR", "Number of Rebar", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Cover", "CC", "Concrete Cover", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Area", "CA", "Concrete area", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrds", "Vrdc", "Vrdc", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrdc", "Vrds", "Vrds", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd", "Vrd", "Vrd", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio", "WR", "Working Ratio", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_SectionChecker checker = null;
            GH_ResultBeamForces force = null;
            double height = 0, width = 0, bar = 0, count = 0, cover = 0, area = 0;
            if (!DA.GetData(0, ref checker) || !DA.GetData(1, ref force) || !DA.GetData(2, ref height)
                || !DA.GetData(3, ref width) || !DA.GetData(4, ref bar) || !DA.GetData(5, ref count)
                || !DA.GetData(6, ref cover) || !DA.GetData(7, ref area)) return;
            var r = ConcreteSectionChecker.Adapters.ShearChecks.WithoutStirrups(checker.Value, force.Value,
                height, width, bar, count, cover, area);
            GpcReports.Shear(this, DA, ("Y senza staffe", r));
            DA.SetData(2, GpcReports.Resistance(r, x => x.VRsd));
            DA.SetData(3, GpcReports.Resistance(r, x => x.VRcd));
            DA.SetData(4, GpcReports.Resistance(r, x => x.VRd));
            DA.SetData(5, GpcReports.Number(r.Ratio));
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("08625aa4-a62b-4d06-91fb-9c9ffcf076fc");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
