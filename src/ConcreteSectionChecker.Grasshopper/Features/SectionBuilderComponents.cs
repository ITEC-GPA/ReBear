using System;
using System.Collections.Generic;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ConcreteSectionChecker.Grasshopper.Features
{
    public abstract class SectionBuilderComponent : ReBearComponent
    {
        protected SectionBuilderComponent(string name, string nick, string description) : base(name, nick, description, "6 Sections") { }
        protected static Port ConcretePort => G("Concrete", "C", "Materiale calcestruzzo.");
        protected static Port SteelPort => G("Steel", "S", "Acciaio delle barre longitudinali.");
        protected static Port CoverPort => N("Link cover", "c", "Copriferro alla superficie esterna della staffa, mm.");
        protected static Port LinkPort => N("Link diameter", "Øw", "Diametro staffa usato per posizionare le barre, mm; non definisce il modello di taglio.", 8);
        protected static Port BarPort => N("Bar diameter", "Ø", "Diametro delle barre longitudinali, mm.", 20);
        protected override Port[] Outputs => new[] { G("RC section", "S", "Sezione GPC compatibile con Concrete Checker."),
            Surface("Concrete preview", "C", "Brep XY, mm."), List(G("Rebars", "R", "Barre GPC con preview.")),
            N("Concrete area", "Ac", "Area lorda al netto dei fori, mm²."), N("Steel area", "As", "Area delle barre, mm²."),
            N("Minimum bar cover", "cbar", "Distanza effettiva del calcestruzzo dalla superficie delle barre, mm.") };
        internal static Curve Loop(IEnumerable<GPC.Geometry.Point2d> vertices)
        {
            var points = vertices.Select(p => new Point3d(p.X, p.Y, 0)).ToList();
            if (points.Count < 3) throw new ArgumentException("Contorno non valido.");
            points.Add(points[0]); return new PolylineCurve(points);
        }
        protected void Output(IGH_DataAccess d, ReinforcedConcreteSection section)
        {
            var info = SectionBuilders.Inspect(section);
            d.SetData(0, new GH_ReinforcedConcreteSection(section));
            var loops = new List<Curve> { Loop(section.ConcreteShape.Fill2d.Points) };
            loops.AddRange((section.ConcreteShape.Holes2d ?? Array.Empty<GPC.Geometry.Polygon2d>()).Select(h => Loop(h.Points)));
            var breps = Brep.CreatePlanarBreps(loops, 1e-6);
            if (breps?.Length != 1) throw new InvalidOperationException("Preview della sezione non costruibile.");
            d.SetData(1, breps[0]); d.SetDataList(2, section.Rebars.Select(b => new GH_Rebar(b)));
            d.SetData(3, info.Area); d.SetData(4, info.SteelArea); d.SetData(5, info.MinimumBarCover);
        }
    }
    public sealed class RectangularRcSectionComponent : SectionBuilderComponent
    {
        public RectangularRcSectionComponent() : base("Rectangular RC Section", "RectRC", "Rettangolo centrato nell'origine XY. Barre superiori/inferiori e laterali; geometria in mm.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0024");
        protected override Port[] Inputs => new[] { ConcretePort, SteelPort, N("Width", "b", "Dimensione X, mm."), N("Height", "h", "Dimensione Y, mm."),
            CoverPort, LinkPort, BarPort, I("Top bars", "nt", "Barre fila superiore.", 4), I("Bottom bars", "nb", "Barre fila inferiore.", 4),
            I("Side bars per face", "ns", "Barre su ciascun lato, escluse le barre delle file superiore/inferiore.", 0) };
        protected override void SolveGpc(IGH_DataAccess d) => Output(d, SectionBuilders.Rectangle(Required<ConcreteMaterial>(d, 0), Required<SteelMaterial>(d, 1),
            Read<double>(d, 2), Read<double>(d, 3), Read<double>(d, 4), Read<double>(d, 5), Read<double>(d, 6),
            Read<int>(d, 7), Read<int>(d, 8), Read<int>(d, 9)));
    }
    public sealed class CircularRcSectionComponent : SectionBuilderComponent
    {
        public CircularRcSectionComponent() : base("Circular RC Section", "CircRC",
            "Cerchio discretizzato con poligono inscritto. Barre su un anello; il copriferro minimo è rispettato rispetto ai lati del poligono.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0025");
        protected override Port[] Inputs => new[] { ConcretePort, SteelPort, N("Diameter", "D", "Diametro nominale, mm."),
            CoverPort, LinkPort, BarPort, I("Bar count", "n", "Numero barre, almeno 3.", 12),
            I("Polygon sides", "Seg", "Multiplo di 4 fra 16 e 720; controllare sensibilità della discretizzazione.", 64) };
        protected override void SolveGpc(IGH_DataAccess d) => Output(d, SectionBuilders.Circle(Required<ConcreteMaterial>(d, 0), Required<SteelMaterial>(d, 1),
            Read<double>(d, 2), Read<double>(d, 3), Read<double>(d, 4), Read<double>(d, 5), Read<int>(d, 6), Read<int>(d, 7)));
    }
    public sealed class TeeRcSectionComponent : SectionBuilderComponent
    {
        public TeeRcSectionComponent() : base("T RC Section", "TRC", "Sezione a T con ala superiore, centrata nel rettangolo d'ingombro XY. Layout iniziale da verificare.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0026");
        protected override Port[] Inputs => new[] { ConcretePort, SteelPort, N("Flange width", "bf", "mm."), N("Height", "h", "mm."),
            N("Web width", "bw", "mm."), N("Flange thickness", "tf", "mm."), CoverPort, LinkPort, BarPort,
            I("Top bars", "nt", "Barre superiori nell'ala.", 6), I("Bottom bars", "nb", "Barre inferiori nell'anima.", 3),
            I("Web bars per face", "ns", "Barre aggiuntive su ciascun lato dell'anima.", 2) };
        protected override void SolveGpc(IGH_DataAccess d) => Output(d, SectionBuilders.Tee(Required<ConcreteMaterial>(d, 0), Required<SteelMaterial>(d, 1),
            Read<double>(d, 2), Read<double>(d, 3), Read<double>(d, 4), Read<double>(d, 5), Read<double>(d, 6),
            Read<double>(d, 7), Read<double>(d, 8), Read<int>(d, 9), Read<int>(d, 10), Read<int>(d, 11)));
    }
    public sealed class IRcSectionComponent : SectionBuilderComponent
    {
        public IRcSectionComponent() : base("I RC Section", "IRC", "Sezione a I con ali anche diverse. Barre sulle facce esterne delle ali e sui lati dell'anima.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0027");
        protected override Port[] Inputs => new[] { ConcretePort, SteelPort, N("Top flange width", "bt", "mm."), N("Bottom flange width", "bb", "mm."),
            N("Height", "h", "mm."), N("Web width", "bw", "mm."), N("Top flange thickness", "tt", "mm."),
            N("Bottom flange thickness", "tb", "mm."), CoverPort, LinkPort, BarPort,
            I("Top bars", "nt", "Barre superiori.", 6), I("Bottom bars", "nb", "Barre inferiori.", 6), I("Web bars per face", "ns", "Barre laterali nell'anima.", 2) };
        protected override void SolveGpc(IGH_DataAccess d) => Output(d, SectionBuilders.ISection(Required<ConcreteMaterial>(d, 0), Required<SteelMaterial>(d, 1),
            Read<double>(d, 2), Read<double>(d, 3), Read<double>(d, 4), Read<double>(d, 5), Read<double>(d, 6), Read<double>(d, 7),
            Read<double>(d, 8), Read<double>(d, 9), Read<double>(d, 10), Read<int>(d, 11), Read<int>(d, 12), Read<int>(d, 13)));
    }
    public sealed class BoxRcSectionComponent : SectionBuilderComponent
    {
        public BoxRcSectionComponent() : base("Box RC Section", "BoxRC", "Rettangolo con foro centrale. Armatura perimetrale esterna ed eventuale armatura intorno al foro.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0028");
        protected override Port[] Inputs => new[] { ConcretePort, SteelPort, N("Width", "b", "mm."), N("Height", "h", "mm."),
            N("Hole width", "bi", "mm."), N("Hole height", "hi", "mm."), CoverPort, LinkPort, BarPort,
            I("Outer bars per horizontal side", "nx", "Include gli angoli.", 6),
            I("Outer bars per vertical side", "ny", "Include gli angoli, senza duplicarli.", 8),
            I("Inner bars per horizontal side", "nix", "0 senza anello interno, altrimenti almeno 2.", 0),
            I("Inner bars per vertical side", "niy", "0 senza anello interno, altrimenti almeno 2.", 0) };
        protected override void SolveGpc(IGH_DataAccess d) => Output(d, SectionBuilders.Box(Required<ConcreteMaterial>(d, 0), Required<SteelMaterial>(d, 1),
            Read<double>(d, 2), Read<double>(d, 3), Read<double>(d, 4), Read<double>(d, 5), Read<double>(d, 6), Read<double>(d, 7),
            Read<double>(d, 8), Read<int>(d, 9), Read<int>(d, 10), Read<int>(d, 11), Read<int>(d, 12)));
    }
    public sealed class RebarLayoutComponent : ReBearComponent
    {
        public RebarLayoutComponent() : base("Rebar Path Layout", "RebarPath", "Distribuisce barre lungo la curva dei loro centri in XY, per numero o passo massimo. Preparare il percorso con il copriferro desiderato.", "6 Sections") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0029");
        protected override Port[] Inputs => new[] { C("Bar centre path", "Path", "Curva dei centri in XY, mm; aperta o chiusa."),
            G("Steel", "S", "Acciaio delle barre."), N("Diameter", "Ø", "mm.", 20),
            I("Count", "n", "0 = usa passo massimo; altrimenti numero assegnato.", 0), N("Maximum path spacing", "s", "Passo lungo la curva, mm.", 150) };
        protected override Port[] Outputs => new[] { List(G("Rebars", "R", "Barre per Reinforced Concrete Section.")),
            List(P("Centres", "P", "Centri in XY.")), N("Path spacing", "s", "Passo effettivo lungo la curva, mm.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var path = Required<Curve>(d, 0); var steel = Required<SteelMaterial>(d, 1);
            if (steel.SteelType == SteelMaterial.SteelTypes.Tendon)
                throw new ArgumentException("Usare acciaio per barre ordinarie.");
            double phi = Read<double>(d, 2), spacing = Read<double>(d, 4), length = path.GetLength();
            var box = path.GetBoundingBox(true);
            if (Math.Abs(box.Min.Z) > 1e-6 || Math.Abs(box.Max.Z) > 1e-6 || !double.IsFinite(phi) || phi <= 0 || !double.IsFinite(length) || length <= 0)
                throw new ArgumentException("Percorso valido in XY e diametro positivo richiesti.");
            int n = Read<int>(d, 3), minimum = path.IsClosed ? 3 : 2;
            if (n == 0)
            {
                if (!double.IsFinite(spacing) || spacing <= 0) throw new ArgumentException("Passo massimo positivo richiesto.");
                double count = Math.Ceiling(length / spacing) + (path.IsClosed ? 0 : 1);
                if (count > 1000) throw new ArgumentException("Il passo richiesto produce più di 1000 barre.");
                n = Math.Max(minimum, (int)count);
            }
            if (n < minimum || n > 1000) throw new ArgumentException("Numero barre non valido.");
            var points = new List<Point3d>();
            for (int i = 0; i < n; i++)
            {
                if (!path.NormalizedLengthParameter((double)i / (path.IsClosed ? n : n - 1), out double parameter))
                    throw new InvalidOperationException("Impossibile discretizzare il percorso.");
                var p = path.PointAt(parameter);
                if (points.Any(q => p.DistanceTo(q) <= phi)) throw new ArgumentException("Barre a contatto o sovrapposte lungo il percorso.");
                points.Add(p);
            }
            d.SetDataList(0, points.Select(p => new GH_Rebar(new ReinforcedConcreteRebar(new RebarSectionCircular(phi, steel), new GPC.Geometry.Point2d(p.X, p.Y)))));
            d.SetDataList(1, points); d.SetData(2, length / (path.IsClosed ? n : n - 1));
        }
    }
    public sealed class SectionGeometryCheckComponent : ReBearComponent
    {
        public SectionGeometryCheckComponent() : base("Section Geometry Check", "GeomCheck", "Controllo geometrico di barre, copriferro e interferro. Non sostituisce Reinforcement Detailing.", "6 Sections") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0030");
        protected override Port[] Inputs => new[] { G("Section", "S", "Sezione GPC."), N("Required bar cover", "c", "Copriferro geometrico minimo alle barre, mm.", 0),
            N("Required clear spacing", "s", "Interferro minimo assegnato, mm.", 0), T("Section ID", "ID", "Identificativo.", "Section") };
        protected override Port[] Outputs => new[] { G("Result", "R", "Esito dei soli vincoli geometrici assegnati."),
            N("Minimum bar cover", "c", "mm."), N("Minimum clear spacing", "s", "mm."), N("Steel area", "As", "mm².") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            double c = Read<double>(d, 1), s = Read<double>(d, 2);
            if (!double.IsFinite(c) || !double.IsFinite(s) || c < 0 || s < 0) throw new ArgumentException("Limiti geometrici non negativi richiesti.");
            var r = SectionBuilders.Inspect(Required<ReinforcedConcreteSection>(d, 0));
            bool pass = r.Bars > 0 && r.MinimumBarCover >= c && (r.Bars == 1 || r.MinimumClearSpacing > 0 && r.MinimumClearSpacing >= s) && r.SteelArea < r.Area;
            Result(d, new CheckResult(Read<string>(d, 3), "Geometry", "Geometry", CheckResults.Status(pass), null,
                Message: FormattableString.Invariant($"cbar={r.MinimumBarCover} mm ≥ {c}; interferro={r.MinimumClearSpacing} mm ≥ {s}.")));
            d.SetData(1, r.MinimumBarCover); d.SetData(2, r.MinimumClearSpacing); d.SetData(3, r.SteelArea);
        }
    }
}
