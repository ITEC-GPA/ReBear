using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class RectangularSectionShearCheckComponent : GpcComponent
    {
        public RectangularSectionShearCheckComponent()
            : base("Rectangular Section Shear Check", "SC", "Shear Check")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force", "F", "Force", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Base", "B", "Base", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup Diameter", "SD", "Stirrup Diameter", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Legs X", "NLX", "Number of Legs of a stirrup in X direction", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Legs Y", "NLY", "Number of Legs of a stirrup in Y direction", GH_ParamAccess.item);
            pManager.AddNumberParameter("Spacing", "S", "Spacing", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup slope X", "SS X", "Stirrup slope X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup slope Y", "SS Y", "Stirrup slope Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitudinal rebar diameter X", "LRD X", "Longitudinal rebar diameter X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitudinal rebar diameter Y", "LRD Y", "Longitudinal rebar diameter Y", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of longit rebarsX", "Number of longit rebarsX", "Number of longit rebarsX", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of longit rebarsY", "Number of longit rebarsY", "Number of longit rebarsY", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Cover", "CC", "Concrete Cover", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Area", "CA", "Concrete area", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd X no stirrup", "Vrsd X no stirrup", "Vrsd X no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd Y no stirrup", "Vrsd Y no stirrup", "Vrsd Y no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd X no stirrup", "Vrcd X no stirrup", "Vrcd X no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd Y no stirrup", "Vrcd Y no stirrup", "Vrcd Y no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd X no stirrup", "Vrd X no stirrup", "Vrd X no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd Y no stirrup", "Vrd Y no stirrup", "Vrd Y no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd X", "Vrsd X", "Vrsd X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd Y", "Vrsd Y", "Vrsd Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd X", "Vrcd X", "Vrcd X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd Y", "Vrcd Y", "Vrcd Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd X", "Vrd X", "Vrd X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd Y", "Vrd Y", "Vrd Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio X no stirrup", "WR X NS", "Working Ratio X no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio Y no stirrup", "WR Y NS", "Working Ratio Y no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio X", "WR X", "Working Ratio X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio Y", "WR Y", "Working Ratio Y", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_SectionChecker checker = null;
            GH_ResultBeamForces force = null;
            double height = 0, width = 0, link = 0, spacing = 0, alphaX = 0, alphaY = 0;
            double barX = 0, barY = 0, cover = 0, area = 0;
            int legsX = 0, legsY = 0, countX = 0, countY = 0;
            if (!DA.GetData(0, ref checker) || !DA.GetData(1, ref force) || !DA.GetData(2, ref height)
                || !DA.GetData(3, ref width) || !DA.GetData(4, ref link) || !DA.GetData(5, ref legsX)
                || !DA.GetData(6, ref legsY) || !DA.GetData(7, ref spacing) || !DA.GetData(8, ref alphaX)
                || !DA.GetData(9, ref alphaY) || !DA.GetData(10, ref barX) || !DA.GetData(11, ref barY)
                || !DA.GetData(12, ref countX) || !DA.GetData(13, ref countY) || !DA.GetData(14, ref cover)
                || !DA.GetData(15, ref area)) return;
            var r = ConcreteSectionChecker.Adapters.ShearChecks.Rectangular(checker.Value, force.Value,
                height, width, link, legsX, legsY, spacing, alphaX, alphaY, barX, barY, countX, countY, cover, area);
            GpcReports.Shear(this, DA, ("X senza staffe", r[0]), ("Y senza staffe", r[1]), ("X", r[2]), ("Y", r[3]));
            int output = 2;
            foreach (int start in new[] { 0, 2 })
            {
                DA.SetData(output++, GpcReports.Resistance(r[start], x => x.VRsd));
                DA.SetData(output++, GpcReports.Resistance(r[start + 1], x => x.VRsd));
                DA.SetData(output++, GpcReports.Resistance(r[start], x => x.VRcd));
                DA.SetData(output++, GpcReports.Resistance(r[start + 1], x => x.VRcd));
                DA.SetData(output++, GpcReports.Resistance(r[start], x => x.VRd));
                DA.SetData(output++, GpcReports.Resistance(r[start + 1], x => x.VRd));
            }
            foreach (var result in r) DA.SetData(output++, GpcReports.Number(result?.Ratio));
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("73aec799-492f-4978-9e12-48faf9853d10");

    }
}
