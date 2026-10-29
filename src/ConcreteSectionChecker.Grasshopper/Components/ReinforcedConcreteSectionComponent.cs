using System;
using System.Collections.Generic;
using System.Linq;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class ReinforcedConcreteSectionComponent : ReBearComponent
    {
        public ReinforcedConcreteSectionComponent() : base("Reinforced Concrete Section", "RCS",
            "Sezione da una faccia piana in XY (contorno e fori poligonali) e dalle barre nelle stesse coordinate, mm.") { }
        public override Guid ComponentGuid => new("f247cd41-e5cb-4a35-b9b8-e06b4c43afc6");
        protected override Port[] Inputs => new[] { T("Section Name", "n", "Nome della sezione.", ""), Surface("Surface", "S", "Una sola faccia piana parallela a XY, eventualmente forata."),
            G("Concrete Material", "M", "Calcestruzzo GPC."), List(G("Rebar elements", "RE", "Barre e trefoli nelle coordinate XY della superficie.")) };
        protected override Port[] Outputs => new[] { G("Reinforced Concrete Section", "RCS", "Sezione GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            string name = Read(d, 0, "");
            var brep = Required<Brep>(d, 1);
            var concrete = Required<ConcreteMaterial>(d, 2);
            var rebars = ReadList<ReinforcedConcreteRebar>(d, 3);
            if (brep.Faces.Count != 1)
                throw new ArgumentException("La sezione richiede una sola faccia piana, eventualmente con fori. Unire prima le superfici.");
            var face = brep.Faces[0];
            if (!face.TryGetPlane(out Plane plane) || Math.Abs(plane.Normal.Z) < 1 - 1e-8 || Math.Abs(plane.Origin.Z) > 1e-6)
                throw new ArgumentException("La sezione e le barre devono essere riferite al piano XY (Z = 0). Portare la geometria nel piano prima della verifica.");
            GPC.Geometry.Polygon2d Polygon(BrepLoop loop)
            {
                if (!loop.To3dCurve().TryGetPolyline(out Polyline polyline) || !polyline.IsClosed || polyline.Count < 4)
                    throw new ArgumentException("I contorni della sezione e dei fori devono essere polilinee chiuse.");
                var polygon = new GPC.Geometry.Polygon2d();
                for (int i = 0; i < polyline.Count - 1; i++) polygon.Add(polyline[i].X, polyline[i].Y);
                return polygon;
            }
            var holes = new List<GPC.Geometry.Polygon2d>();
            foreach (BrepLoop loop in face.Loops)
                if (loop != face.OuterLoop) holes.Add(Polygon(loop));
            var section = new ReinforcedConcreteSection(new GPC.Geometry.Shape2d(Polygon(face.OuterLoop), holes.ToArray()), concrete, name);
            section.AddRebars(rebars);
            if (rebars.Count == 0) AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Sezione senza barre.");
            d.SetData(0, new GH_ReinforcedConcreteSection(section));
        }
    }

    public abstract class SectionPropertiesComponent : ReBearComponent
    {
        protected SectionPropertiesComponent(string name, string nick, string description) : base(name, nick, description) { }
        protected override Port[] Outputs => new[] { T("Name", "N", "Nome della sezione."), N("Area", "A", "Area, mm²."),
            N("Sx", "Sx", "Momento statico rispetto all'asse X globale, mm³."), N("Sy", "Sy", "Momento statico rispetto all'asse Y globale, mm³."),
            N("Jxx", "Jxx", "Momento d'inerzia baricentrico, mm⁴."), N("Jyy", "Jyy", "Momento d'inerzia baricentrico, mm⁴."),
            N("Jxy", "Jxy", "Prodotto d'inerzia baricentrico, mm⁴."), N("Jp", "Jp", "Momento d'inerzia polare, mm⁴."),
            N("J11", "J11", "Momento d'inerzia principale massimo, mm⁴."), N("J22", "J22", "Momento d'inerzia principale minimo, mm⁴."),
            N("Principal angle", "θ1", "Angolo dell'asse principale 1 rispetto a X, gradi."), P("Centroid", "C", "Baricentro in XY, mm.") };
        protected void Output(IGH_DataAccess d, string name, params double[] values)
        {
            d.SetData(0, name);
            for (int i = 0; i < 10; i++) d.SetData(i + 1, values[i]);
            d.SetData(11, new Point3d(values[10], values[11], 0));
        }
    }

    public sealed class ConcreteMechanicalPropertiesComponent : SectionPropertiesComponent
    {
        public ConcreteMechanicalPropertiesComponent() : base("Concrete Mechanical Properties", "CP", "Proprietà geometriche della sola sezione di calcestruzzo, mm.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000010");
        protected override Port[] Inputs => new[] { G("Reinforced Concrete Section", "RCS", "Sezione GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var s = Required<ReinforcedConcreteSection>(d, 0); var moments = s.CalculateStaticMoments();
            Output(d, s.Name, s.Area, moments.Sx, moments.Sy, s.Jxx, s.Jyy, s.Jxy, s.Jp, s.J11, s.J22, s.AngleX1 * 180 / Math.PI, s.Centroid.X, s.Centroid.Y);
        }
    }

    public sealed class HomogenizedMechanicalPropertiesComponent : SectionPropertiesComponent
    {
        public HomogenizedMechanicalPropertiesComponent() : base("Homogenized Mechanical Properties", "HP",
            "Proprietà della sezione omogeneizzata a calcestruzzo con il coefficiente ψ delle barre, mm.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000011");
        protected override Port[] Inputs => new[] { G("Reinforced Concrete Section", "RCS", "Sezione GPC."),
            N("ψ factor", "ψ", "Coefficiente ψ (viscosità) delle barre; 0 = breve durata.", 0) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var s = Required<ReinforcedConcreteSection>(d, 0); double psi = Finite(d, 1, nonNegative: true);
            var p = s.GetHomogeneizedMechanicalProperties(psi);
            Output(d, s.Name, p.areaH, p.SxH, p.SyH, p.JxxH, p.JyyH, p.JxyH, p.JpH, p.J11H, p.J22H, p.angleX * 180 / Math.PI, p.centroidH.X, p.centroidH.Y);
        }
    }
}
