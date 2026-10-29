using System.Drawing;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using Rhino.Geometry;

namespace ConcreteSectionChecker.Grasshopper
{
    /// <summary>The only conversion between plain meshes (<see cref="MeshData"/>) and Rhino meshes.</summary>
    internal static class RhinoMeshes
    {
        public static Mesh ToRhino(MeshData data)
        {
            var mesh = new Mesh();
            foreach (var (x, y, z) in data.Vertices) mesh.Vertices.Add(x, y, z);
            foreach (var f in data.Faces)
                if (f.Length == 4) mesh.Faces.AddFace(f[0], f[1], f[2], f[3]); else mesh.Faces.AddFace(f[0], f[1], f[2]);
            if (data.Colours.Count == data.Vertices.Count)
                foreach (var c in data.Colours) mesh.VertexColors.Add(Color.FromArgb(c.R, c.G, c.B));
            mesh.Normals.ComputeNormals();
            mesh.Compact();
            return mesh;
        }

        public static MeshData FromRhino(Mesh mesh)
        {
            var data = new MeshData();
            data.Vertices.AddRange(mesh.Vertices.Select(v => ((double)v.X, (double)v.Y, (double)v.Z)));
            foreach (var f in mesh.Faces) data.Faces.Add(f.IsQuad ? new[] { f.A, f.B, f.C, f.D } : new[] { f.A, f.B, f.C });
            return data;
        }

        public static Rgb Rgb(Color c) => new(c.R, c.G, c.B);
    }
}
