using System;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Geometry;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class CoordinateSystemComponent : ReBearComponent
    {
        public CoordinateSystemComponent() : base("Coordinate System", "CS", "Riferimento delle azioni da origine e due punti sugli assi X e Y, mm.") { }
        public override Guid ComponentGuid => new("8f839216-244a-4891-a1d5-bb0fb42f7909");
        protected override Port[] Inputs => new[] { T("Name", "N", "Nome del riferimento.", ""), P("Origin", "O", "Origine, mm."),
            P("P1", "P1", "Punto sull'asse X."), P("P2", "P2", "Punto nel piano XY dalla parte delle Y positive.") };
        protected override Port[] Outputs => new[] { G("Coordinate System", "CS", "Riferimento GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var o = Required<Rhino.Geometry.Point3d>(d, 1); var p1 = Required<Rhino.Geometry.Point3d>(d, 2); var p2 = Required<Rhino.Geometry.Point3d>(d, 3);
            if (o.DistanceTo(p1) <= 1e-9 || o.DistanceTo(p2) <= 1e-9 || Rhino.Geometry.Vector3d.CrossProduct(p1 - o, p2 - o).Length <= 1e-12)
                throw new ArgumentException("Origine e punti degli assi devono essere distinti e non allineati.");
            d.SetData(0, new GH_CoordinateSystem(new CoordinateSystem(new Point3d(o.X, o.Y, o.Z), new Point3d(p1.X, p1.Y, p1.Z),
                new Point3d(p2.X, p2.Y, p2.Z), Read(d, 0, ""))));
        }
    }

    /// <summary>
    /// 0.3: global X/Y directions at the centroid. Up to 0.2 the axes were (−X, −Y), the internal axes of the GPC solver,
    /// so that forces given in this system had Mx and My with the opposite sign of the global convention.
    /// </summary>
    public sealed class CoordinateSystemSectionComponent : ReBearComponent
    {
        public CoordinateSystemSectionComponent() : base("Section Coordinate System", "SCS",
            "Riferimento con assi X e Y globali nel baricentro della sezione (omogeneizzato per sezioni composte).") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000012");
        protected override Port[] Inputs => new[] { G("Section", "S", "Sezione GPC.") };
        protected override Port[] Outputs => new[] { G("Coordinate System", "CS", "Riferimento baricentrico con assi globali.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var section = Required<ReinforcedConcreteSection>(d, 0);
            var centroid = section.IsCompositeSteelConcrete ? section.GetHomogenizedCentroid(out double _, out double _) : section.Centroid;
            d.SetData(0, new GH_CoordinateSystem(new CoordinateSystem(new Point3d(centroid.X, centroid.Y, 0), Vector3d.XAxis, Vector3d.YAxis, "Section centroid")));
        }
    }

    public sealed class PsiFactorComponent : ReBearComponent
    {
        public PsiFactorComponent() : base("Psi coefficient", "ψ", "Coefficiente ψ corrispondente al rapporto di omogeneizzazione n assegnato (GPC).") { }
        public override Guid ComponentGuid => new("7480e84d-4e82-4be1-a021-425dff4803e1");
        protected override Port[] Inputs => new[] { G("Concrete Material", "CM", "Calcestruzzo GPC."), G("Steel material", "SM", "Acciaio delle barre."),
            N("Homogenized factor", "n", "Rapporto di omogeneizzazione Es/Ec,eff.", 15) };
        protected override Port[] Outputs => new[] { N("ψ factor", "ψ", "Coefficiente ψ delle barre.") };
        protected override void SolveGpc(IGH_DataAccess d) => d.SetData(0, ReinforcedConcreteSection.CalculateHomogenizedFactorPhi(
            Finite(d, 2, positive: true), Required<GPC.Model.Materials.SteelMaterialEN1992>(d, 1), Required<GPC.Model.Materials.ConcreteMaterialEuropeanCommon>(d, 0)));
    }
}
