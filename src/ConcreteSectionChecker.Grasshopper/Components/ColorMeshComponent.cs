using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ColorMeshComponent : GpcComponent
    {
        public ColorMeshComponent()
            : base("Color Mesh", "CM", "Color Mesh", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "Points as List", GH_ParamAccess.list);
            pManager.AddColourParameter("Colours", "col", "Values as List", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "msh", "Mesh as item", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Smooth", "s", "Boolean for smooth gradient", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "msh", "Output mesh", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            List<Point3d> points = new List<Point3d>();
            List<Color> colors = new List<Color>();
            Mesh mesh = new Mesh();
            bool smooth = false;
            if (DA.GetDataList(0, points) && DA.GetDataList<Color>(1, colors) && DA.GetData<Mesh>(2, ref mesh) && DA.GetData<bool>(3, ref smooth))
            {
                if (colors.Count != points.Count)
                {
                    if (colors.Count != 1)                    
                        return;
                    
                    for (int i = 0; i < points.Count - 1; i++)
                        colors.Add(colors[0]);                    
                }
                if (mesh == null)                
                    return;                

                Mesh outMesh = new Mesh();
                int num = 0;
                for (int j = 0; j < mesh.Faces.Count; j++)
                {
                    int num2 = num;
                    int[] verticesIds = new int[4];
                    MeshFace face = mesh.Faces.GetFace(j);
                    verticesIds[0] = face.A;
                    verticesIds[1] = face.B;
                    verticesIds[2] = face.C;
                    verticesIds[3] = face.D;
                    int numberOfVertices = (!face.IsQuad) ? 3 : 4;
                    Point3d centroid = Point3d.Unset;
                    try
                    {
                        centroid = mesh.Faces.GetFaceCenter(j);
                    }
                    catch(Exception)
                    { continue; }

                    for (int k = 0; k < numberOfVertices; k++)
                    {
                        Point3d verticesPoint = mesh.Vertices[verticesIds[k]];
                        outMesh.Vertices.Add(verticesPoint.X, verticesPoint.Y, verticesPoint.Z);
                        num++;

                        double[] pp = new double[points.Count];
                        Color[] cc = new Color[points.Count];

                        if (smooth)
                        {
                            for (int l = 0; l < points.Count; l++)
                            {
                                pp[l] = Math.Abs(verticesPoint.DistanceTo(points[l]));
                                cc[l] = colors[l];
                            }
                        }
                        else
                        {                            
                            for (int m = 0; m < points.Count; m++)
                            {                                
                                pp[m] = Math.Abs(centroid.DistanceTo(points[m]));
                                cc[m] = colors[m];
                            }
                        }
                        Array.Sort(pp, cc);
                        outMesh.VertexColors.Add(cc[0]);
                    }

                    face = mesh.Faces[j];

                    if (face.IsQuad)                    
                        outMesh.Faces.AddFace(num2, num2 + 1, num2 + 2, num2 + 3);                    
                    else                    
                        outMesh.Faces.AddFace(num2, num2 + 1, num2 + 2);                    
                }

                DA.SetData(0, new GH_Mesh(outMesh));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("1c72c75b-9ede-461a-8553-973173695f79");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
