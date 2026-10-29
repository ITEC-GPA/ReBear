using System;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class RebarComponent : ReBearComponent
    {
        public RebarComponent() : base("Rebar Element", "RE", "Barra ordinaria circolare nel piano XY della sezione; diametro in mm.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000008");
        protected override Port[] Inputs => new[] { P("Position", "P", "Centro della barra in XY, mm (Z ignorata se nulla)."),
            N("Diameter", "Ø", "Diametro, mm.", 20), G("Rebar Material", "M", "Acciaio per barre ordinarie.") };
        protected override Port[] Outputs => new[] { G("Rebar Element", "RE", "Barra GPC con preview.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var position = Required<Point3d>(d, 0); double diameter = Finite(d, 1, positive: true);
            var steel = Required<SteelMaterialEN1992>(d, 2);
            if (steel.SteelType == SteelMaterial.SteelTypes.Tendon) throw new ArgumentException("Usare Tendon Element per l'acciaio da precompressione.");
            if (Math.Abs(position.Z) > 1e-6) throw new ArgumentException("La barra deve giacere nel piano XY della sezione.");
            d.SetData(0, new GH_Rebar(new ReinforcedConcreteRebar(new RebarSectionCircular(diameter, steel), new GPC.Geometry.Point2d(position.X, position.Y))));
        }
    }

    public sealed class TendonComponent : ReBearComponent
    {
        public TendonComponent() : base("Tendon Element", "TE", "Trefolo o barra da precompressione nel piano XY; diametro in mm, tensione iniziale in MPa.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000009");
        protected override Port[] Inputs => new[] { P("Position", "P", "Centro in XY, mm."), N("Diameter", "Ø", "Diametro equivalente, mm."),
            G("Tendon Material", "M", "Acciaio da precompressione."), N("Pretension", "σp", "Tensione di precompressione, MPa (trazione positiva).", 1400) };
        protected override Port[] Outputs => new[] { G("Tendon Element", "TE", "Trefolo GPC con preview.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var position = Required<Point3d>(d, 0); double diameter = Finite(d, 1, positive: true);
            var steel = Required<SteelMaterialEN1992>(d, 2); double sigma = Finite(d, 3, nonNegative: true);
            if (Math.Abs(position.Z) > 1e-6) throw new ArgumentException("Il trefolo deve giacere nel piano XY della sezione.");
            d.SetData(0, new GH_Rebar(new ReinforcedConcreteRebar(new RebarSectionCircular(diameter, steel), new GPC.Geometry.Point2d(position.X, position.Y), sigma)));
        }
    }
}
