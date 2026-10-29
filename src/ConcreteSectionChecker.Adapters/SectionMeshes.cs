using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Checkers.Concrete.Results;
using GPC.Geometry;
using GPC.Model.Sections.Concrete;
using GpcMesh = GPC.Geometry.Meshes.Mesh;

namespace ConcreteSectionChecker.Adapters
{
    /// <summary>Colour as RGB bytes, independent of the drawing library.</summary>
    public readonly record struct Rgb(byte R, byte G, byte B)
    {
        public static readonly Rgb White = new(255, 255, 255), Black = new(0, 0, 0), Blue = new(0, 0, 255),
            Orange = new(255, 165, 0), Red = new(255, 0, 0), Magenta = new(255, 0, 255);
    }

    /// <summary>Plain mesh: vertex coordinates (mm), faces of 3 or 4 vertex indices, optional colour per vertex.</summary>
    public sealed class MeshData
    {
        public List<(double X, double Y, double Z)> Vertices { get; } = new();
        public List<int[]> Faces { get; } = new();
        public List<Rgb> Colours { get; } = new();
        public (double X, double Y, double Z) Centroid(int face)
        {
            var f = Faces[face];
            return (f.Average(i => Vertices[i].X), f.Average(i => Vertices[i].Y), f.Average(i => Vertices[i].Z));
        }
    }

    /// <summary>
    /// Section meshes, stress sampling, neutral axis and colour scales of the stress results, testable without Rhino.
    /// Components only convert <see cref="MeshData"/> to Rhino geometry.
    /// </summary>
    public static class SectionMeshes
    {
        /// <summary>The one GPC → plain mesh conversion; unreferenced vertices are dropped (as Rhino Compact).</summary>
        public static MeshData From(GpcMesh mesh)
        {
            if (mesh is null) throw new ArgumentNullException(nameof(mesh));
            var result = new MeshData();
            var vertices = mesh.GetVertices().ToDictionary(v => v.Id);
            var indices = mesh.Vertices.GetElementIdMap();
            var map = new Dictionary<int, int>();
            int Index(int id)
            {
                // Faces refer to vertex identifiers; resolve them through the identifier map of the collection.
                var vertex = indices.TryGetValue(id, out var list) && list.Count > 0 ? mesh.Vertices.GetElementByIndex(list[0]) : vertices[id];
                if (!map.TryGetValue(vertex.Id, out int index))
                {
                    map[vertex.Id] = index = result.Vertices.Count;
                    result.Vertices.Add((vertex.Point.X, vertex.Point.Y, vertex.Point.Z));
                }
                return index;
            }
            foreach (var face in mesh.GetFaces())
                result.Faces.Add(face.IsQuad ? new[] { Index(face.A), Index(face.B), Index(face.C), Index(face.D) }
                    : new[] { Index(face.A), Index(face.B), Index(face.C) });
            return result;
        }

        /// <summary>Strain variations across the section below this value are round-off of a plane without curvature.</summary>
        public const double CurvatureTolerance = 1e-12;

        /// <summary>
        /// Neutral axis in global XY, spanning the bounding box of the section with a 10% margin; null when the plane has no
        /// curvature (for example all actions zero or a uniform strain, also with round-off below <see cref="CurvatureTolerance"/>
        /// across the section): there is no neutral axis to draw or cut.
        /// </summary>
        public static (Point2d Start, Point2d End)? NeutralAxis(StrainPlane plane, ReinforcedConcreteSection section)
        {
            if (plane is null || section is null) return null;
            var points = section.ConcreteShape.Fill2d.Points;
            double minX = points.Min(p => p.X), maxX = points.Max(p => p.X), minY = points.Min(p => p.Y), maxY = points.Max(p => p.Y);
            double diagonal = Math.Sqrt((maxX - minX) * (maxX - minX) + (maxY - minY) * (maxY - minY));
            if (!(Math.Sqrt(plane.ChiX * plane.ChiX + plane.ChiY * plane.ChiY) * diagonal > CurvatureTolerance)) return null;
            var line = plane.GetNeutralAxis();
            if (line is null) return null;
            double dx = line.End.X - line.Start.X, dy = line.End.Y - line.Start.Y, length = Math.Sqrt(dx * dx + dy * dy);
            if (!(length > 0) || !double.IsFinite(length)) return null;
            dx /= length; dy /= length;
            double cx = (minX + maxX) / 2, cy = (minY + maxY) / 2, half = 0.55 * diagonal;
            // Foot of the perpendicular from the box centre, then ± half the box diagonal along the axis.
            double t = (cx - line.Start.X) * dx + (cy - line.Start.Y) * dy;
            double fx = line.Start.X + t * dx, fy = line.Start.Y + t * dy;
            return (new Point2d(fx - half * dx, fy - half * dy), new Point2d(fx + half * dx, fy + half * dy));
        }

        /// <summary>Concrete stress of the analysis at a point (MPa, compression negative), with the ψ of a linear analysis.</summary>
        public static double ConcreteStress(StressAnalysisResult result, double x, double y)
        {
            var point = new Point2d(x, y);
            return result.LinearElasticAnalysis ? result.GetConcreteTension(result.PsiRebar ?? 0, point) : result.GetConcreteTension(point);
        }

        public static IReadOnlyList<(double X, double Y, double Stress)> ConcreteVertexStresses(StressAnalysisResult result) =>
            (result.LinearElasticAnalysis ? result.GetConcreteVerticesTension(result.PsiRebar ?? 0) : result.GetConcreteVerticesTension())
            .Select(p => (p.point.X, p.point.Y, p.tension)).ToArray();

        public static IReadOnlyList<(double X, double Y, double Stress)> BarStresses(StressAnalysisResult result) =>
            (result.LinearElasticAnalysis ? result.GetRebarsTension(result.PsiRebar ?? 0, result.PsiTendon ?? 0) : result.GetRebarsTension())
            .Select(p => (p.rebar.Position.X, p.rebar.Position.Y, p.tension)).ToArray();

        /// <summary>
        /// Mesh of the section split along the neutral axis (when there is one), so that the colours change on the axis.
        /// <paramref name="refine"/> subdivides the faces for a smoother gradient.
        /// </summary>
        public static MeshData StressMesh(StressAnalysisResult result, bool refine, out (Point2d Start, Point2d End)? axis)
        {
            var section = result?.ConcreteSection as ReinforcedConcreteSection
                ?? throw new NotSupportedException("Sezione in calcestruzzo armato richiesta.");
            axis = NeutralAxis(result.StrainPlane, section);
            var mesh = (GpcMesh)section.Mesh.Clone();
            if (axis.HasValue) mesh.Cut(new Line2d(axis.Value.Start, axis.Value.End));
            mesh.Clean();
            if (refine) mesh.Refine();
            return From(mesh);
        }

        /// <summary>Unwelded copy with one colour per face vertex: at the vertex (smooth) or at the face centroid.</summary>
        public static MeshData Coloured(MeshData mesh, Func<double, double, double, Rgb> colour, bool smooth)
        {
            var result = new MeshData();
            for (int f = 0; f < mesh.Faces.Count; f++)
            {
                var face = mesh.Faces[f]; var centre = mesh.Centroid(f);
                var indices = new int[face.Length];
                for (int k = 0; k < face.Length; k++)
                {
                    var v = mesh.Vertices[face[k]];
                    indices[k] = result.Vertices.Count;
                    result.Vertices.Add(v);
                    result.Colours.Add(smooth ? colour(v.X, v.Y, v.Z) : colour(centre.X, centre.Y, centre.Z));
                }
                result.Faces.Add(indices);
            }
            return result;
        }

        /// <summary>Stress mesh: gradient between fctd and fcd when smooth, sign colours at the face centroids otherwise.</summary>
        public static MeshData StressColours(StressAnalysisResult result, bool smooth, out (Point2d Start, Point2d End)? axis)
        {
            var mesh = StressMesh(result, true, out axis);
            var concrete = ((ReinforcedConcreteSection)result.ConcreteSection).ConcreteMaterial;
            double compression = concrete.CalculateDesignCompressiveStrength(result.Standard);
            double tension = concrete.CalculateDesignTensileStrength(result.Standard);
            return Coloured(mesh, (x, y, _) =>
            {
                double stress = ConcreteStress(result, x, y);
                return smooth ? GradientColour(stress, compression, tension) : SignColour(stress);
            }, smooth);
        }

        /// <summary>
        /// White at zero, blue at fctd (tension, positive); orange, red and magenta at 1/3, 2/3 and 1 of fcd (compression,
        /// negative). Black outside [fcd, fctd]: the design strength is exceeded.
        /// </summary>
        public static Rgb GradientColour(double stress, double compressionStrength, double tensionStrength)
        {
            double c = -Math.Abs(compressionStrength), t = Math.Abs(tensionStrength);
            double v02 = c / 3, v03 = 2 * c / 3;
            if (!double.IsFinite(stress)) return Rgb.Black;
            if (stress <= t && stress >= 0) return Lerp(Rgb.White, Rgb.Blue, t == 0 ? 0 : stress / t);
            if (stress <= 0 && stress >= v02) return Lerp(Rgb.Orange, Rgb.White, (stress - v02) / (0 - v02));
            if (stress <= v02 && stress >= v03) return Lerp(Rgb.Red, Rgb.Orange, (stress - v03) / (v02 - v03));
            if (stress <= v03 && stress >= c) return Lerp(Rgb.Magenta, Rgb.Red, (stress - c) / (v03 - c));
            return Rgb.Black;
        }

        /// <summary>White at zero, red in compression, blue in tension.</summary>
        public static Rgb SignColour(double stress) => stress == 0 ? Rgb.White : stress < 0 ? Rgb.Red : Rgb.Blue;

        public static Rgb Lerp(Rgb a, Rgb b, double t)
        {
            t = double.IsFinite(t) ? Math.Max(0, Math.Min(1, t)) : 0;
            return new((byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));
        }

        /// <summary>Colour of the nearest sample point (at the vertex when smooth, at the face centroid otherwise).</summary>
        public static MeshData NearestColours(MeshData mesh, IReadOnlyList<(double X, double Y, double Z)> points, IReadOnlyList<Rgb> colours, bool smooth)
        {
            if (points is null || colours is null || points.Count == 0) throw new ArgumentException("Punti e colori richiesti.");
            if (colours.Count != points.Count && colours.Count != 1) throw new ArgumentException("Un colore per punto oppure un solo colore.");
            Rgb Nearest(double x, double y, double z)
            {
                int best = 0; double distance = double.PositiveInfinity;
                for (int i = 0; i < points.Count; i++)
                {
                    double d = (points[i].X - x) * (points[i].X - x) + (points[i].Y - y) * (points[i].Y - y) + (points[i].Z - z) * (points[i].Z - z);
                    if (d < distance) { distance = d; best = i; }
                }
                return colours.Count == 1 ? colours[0] : colours[best];
            }
            return Coloured(mesh, Nearest, smooth);
        }
    }
}
