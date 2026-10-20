using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;

namespace ConcreteSectionChecker.Adapters
{
    public sealed record SectionGeometryInfo(double Area, double SteelArea, double MinimumBarCover, double MinimumClearSpacing, int Bars);

    /// <summary>Geometrical builders only. All dimensions mm. Cover is measured to the outside of the link.</summary>
    public static class SectionBuilders
    {
        internal static void Positive(params double[] values)
        {
            if (values.Any(v => !double.IsFinite(v) || v <= 0)) throw new ArgumentException("Dimensioni finite e positive richieste.");
        }
        private static double AxisDistance(double cover, double link, double bar)
        {
            if (!double.IsFinite(cover) || !double.IsFinite(link) || cover < 0 || link < 0)
                throw new ArgumentException("Copriferro e diametro staffa devono essere finiti e non negativi.");
            Positive(bar); return cover + link + bar / 2;
        }
        private static void Count(int count, int minimum = 0)
        {
            if (count < minimum || count > 1000) throw new ArgumentException($"Numero barre fra {minimum} e 1000 richiesto.");
        }
        private static (double X, double Y)[] Outline(double b, double h) =>
            new[] { (-b / 2, -h / 2), (b / 2, -h / 2), (b / 2, h / 2), (-b / 2, h / 2) };
        private static void Row(List<Point2d> points, int count, double x0, double x1, double y)
        {
            Count(count); if (count == 0) return;
            if (x1 <= x0) throw new ArgumentException("Spazio insufficiente per la fila di barre.");
            for (int i = 0; i < count; i++) points.Add(new Point2d(count == 1 ? (x0 + x1) / 2 : x0 + (x1 - x0) * i / (count - 1), y));
        }
        private static void Sides(List<Point2d> points, int count, double x, double low, double high)
        {
            Count(count); if (count == 0) return;
            if (x <= 0 || high <= low) throw new ArgumentException("Spazio insufficiente per le barre laterali.");
            for (int i = 1; i <= count; i++)
            {
                double y = low + (high - low) * i / (count + 1);
                points.Add(new Point2d(-x, y)); points.Add(new Point2d(x, y));
            }
        }
        private static ReinforcedConcreteSection Create(IEnumerable<(double X, double Y)> outline,
            IEnumerable<IEnumerable<(double X, double Y)>> holes, IEnumerable<Point2d> positions, ConcreteMaterial concrete,
            SteelMaterial steel, double diameter, double requiredCover, string name)
        {
            if (concrete == null || steel == null) throw new ArgumentException("Materiali richiesti.");
            if (steel.SteelType == SteelMaterial.SteelTypes.Tendon) throw new ArgumentException("Usare acciaio per armature ordinarie.");
            var shape = new Shape2d(new Polygon2d(outline.Select(v => new Point2d(v.X, v.Y))),
                holes.Select(h => new Polygon2d(h.Reverse().Select(v => new Point2d(v.X, v.Y)))).ToArray());
            var section = new ReinforcedConcreteSection(shape, concrete, name);
            section.AddRebars(positions.Select(p => new ReinforcedConcreteRebar(new RebarSectionCircular(diameter, steel), p)));
            var info = Inspect(section);
            if (info.Bars == 0 || info.SteelArea >= info.Area) throw new ArgumentException("Specificare armature con area inferiore all'area lorda.");
            if (info.MinimumBarCover < requiredCover - 1e-6) throw new ArgumentException("Barre fuori dal calcestruzzo o copriferro insufficiente rispetto al layout richiesto.");
            if (info.MinimumClearSpacing <= 1e-6) throw new ArgumentException("Barre sovrapposte o a contatto.");
            return section;
        }
        public static SectionGeometryInfo Inspect(ReinforcedConcreteSection section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            var bars = section.Rebars.ToArray(); var outline = section.ConcreteShape.Fill2d;
            double cover = double.PositiveInfinity, clear = double.PositiveInfinity;
            for (int i = 0; i < bars.Length; i++)
            {
                var p = bars[i].Position; bool inside = outline.IsPointInside(p, 0);
                double distance = outline.DistanceTo(p);
                foreach (var hole in section.ConcreteShape.Holes2d ?? Array.Empty<Polygon2d>())
                { distance = Math.Min(distance, hole.DistanceTo(p)); if (hole.IsPointInside(p, 0)) inside = false; }
                cover = Math.Min(cover, (inside ? distance : -distance) - bars[i].RebarSection.Diameter / 2);
                for (int j = 0; j < i; j++) clear = Math.Min(clear, p.DistanceTo(bars[j].Position)
                    - (bars[i].RebarSection.Diameter + bars[j].RebarSection.Diameter) / 2);
            }
            return new(section.Area, section.AreaRebars, bars.Length == 0 ? double.NaN : cover,
                bars.Length < 2 ? double.NaN : clear, bars.Length);
        }
        public static ReinforcedConcreteSection Rectangle(ConcreteMaterial c, SteelMaterial s, double b, double h,
            double cover, double link, double bar, int top, int bottom, int sides)
        {
            Positive(b, h); double a = AxisDistance(cover, link, bar);
            var points = new List<Point2d>(); Row(points, top, -b / 2 + a, b / 2 - a, h / 2 - a);
            Row(points, bottom, -b / 2 + a, b / 2 - a, -h / 2 + a); Sides(points, sides, b / 2 - a, -h / 2 + a, h / 2 - a);
            return Create(Outline(b, h), Array.Empty<(double, double)[]>(), points, c, s, bar, cover + link, "Rectangular RC");
        }
        public static ReinforcedConcreteSection Circle(ConcreteMaterial c, SteelMaterial s, double diameter, double cover,
            double link, double bar, int count, int segments)
        {
            Positive(diameter); Count(count, 3);
            if (segments < 16 || segments > 720 || segments % 4 != 0) throw new ArgumentException("Discretizzazione: multiplo di 4 fra 16 e 720.");
            double a = AxisDistance(cover, link, bar), radius = diameter / 2, barRadius = radius * Math.Cos(Math.PI / segments) - a;
            Positive(barRadius);
            var outline = Enumerable.Range(0, segments).Select(i => (radius * Math.Cos(2 * Math.PI * i / segments), radius * Math.Sin(2 * Math.PI * i / segments)));
            var bars = Enumerable.Range(0, count).Select(i => new Point2d(barRadius * Math.Cos(2 * Math.PI * i / count), barRadius * Math.Sin(2 * Math.PI * i / count)));
            return Create(outline, Array.Empty<(double, double)[]>(), bars, c, s, bar, cover + link, "Circular RC");
        }
        public static ReinforcedConcreteSection Tee(ConcreteMaterial c, SteelMaterial s, double bf, double h, double bw, double tf,
            double cover, double link, double bar, int top, int bottom, int sides)
        {
            Positive(bf, h, bw, tf); if (bw >= bf || tf >= h) throw new ArgumentException("T: bw < bf e tf < h richiesti.");
            double a = AxisDistance(cover, link, bar), low = -h / 2, high = h / 2, joint = high - tf;
            var outline = new[] { (-bw / 2, low), (bw / 2, low), (bw / 2, joint), (bf / 2, joint),
                (bf / 2, high), (-bf / 2, high), (-bf / 2, joint), (-bw / 2, joint) };
            var bars = new List<Point2d>(); Row(bars, top, -bf / 2 + a, bf / 2 - a, high - a);
            Row(bars, bottom, -bw / 2 + a, bw / 2 - a, low + a); Sides(bars, sides, bw / 2 - a, low + a, joint - a);
            return Create(outline, Array.Empty<(double, double)[]>(), bars, c, s, bar, cover + link, "T RC");
        }
        public static ReinforcedConcreteSection ISection(ConcreteMaterial c, SteelMaterial s, double bt, double bb, double h,
            double bw, double tt, double tb, double cover, double link, double bar, int top, int bottom, int sides)
        {
            Positive(bt, bb, h, bw, tt, tb);
            if (bw >= Math.Min(bt, bb) || tt + tb >= h) throw new ArgumentException("I: anima più stretta delle ali e altezza libera positiva richieste.");
            double a = AxisDistance(cover, link, bar), low = -h / 2, high = h / 2, yl = low + tb, yu = high - tt;
            var outline = new[] { (-bb / 2, low), (bb / 2, low), (bb / 2, yl), (bw / 2, yl), (bw / 2, yu), (bt / 2, yu),
                (bt / 2, high), (-bt / 2, high), (-bt / 2, yu), (-bw / 2, yu), (-bw / 2, yl), (-bb / 2, yl) };
            var bars = new List<Point2d>(); Row(bars, top, -bt / 2 + a, bt / 2 - a, high - a);
            Row(bars, bottom, -bb / 2 + a, bb / 2 - a, low + a); Sides(bars, sides, bw / 2 - a, yl + a, yu - a);
            return Create(outline, Array.Empty<(double, double)[]>(), bars, c, s, bar, cover + link, "I RC");
        }
        public static ReinforcedConcreteSection Box(ConcreteMaterial c, SteelMaterial s, double b, double h, double bi, double hi,
            double cover, double link, double bar, int nx, int ny, int innerNx, int innerNy)
        {
            Positive(b, h, bi, hi); Count(nx, 2); Count(ny, 2);
            if (bi >= b || hi >= h) throw new ArgumentException("Foro interno non contenuto nella sezione.");
            double a = AxisDistance(cover, link, bar);
            var bars = new List<Point2d>();
            void Ring(double x, double y, int xCount, int yCount)
            { Row(bars, xCount, -x, x, y); Row(bars, xCount, -x, x, -y); Sides(bars, yCount - 2, x, -y, y); }
            Ring(b / 2 - a, h / 2 - a, nx, ny);
            if (innerNx != 0 || innerNy != 0) { Count(innerNx, 2); Count(innerNy, 2); Ring(bi / 2 + a, hi / 2 + a, innerNx, innerNy); }
            return Create(Outline(b, h), new[] { Outline(bi, hi) }, bars, c, s, bar, cover + link, "Box RC");
        }
    }
}
