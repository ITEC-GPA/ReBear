using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ColorGradientMeshComponent : GpcComponent
    {
        public ColorGradientMeshComponent()
            : base("Result Color Mesh", "CM", "Color Mesh")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis Results", "SA", "Stress Analysis Results", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Smooth", "s", "Boolean for smooth gradient", GH_ParamAccess.item, true);            
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "msh", "Output mesh", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_StressAnalysisResult gH_StressAnalysisResult = null;
            bool smooth = true;

            if (DA.GetData(0, ref gH_StressAnalysisResult) && DA.GetData(1, ref smooth))
            {
                ReinforcedConcreteSection section = (ReinforcedConcreteSection)gH_StressAnalysisResult.Value.ConcreteSection;
                GPC.Checkers.Concrete.Results.StressAnalysisResult result = gH_StressAnalysisResult.Value;
                GPC.Checkers.Concrete.Results.StrainPlane strainPlane = gH_StressAnalysisResult.Value.StrainPlane;

                int cifresignificativeStress = 2;

                (GPC.Geometry.Point2d point, double tension)[] concreteResults;
                if (gH_StressAnalysisResult.Value.LinearElasticAnalysis)
                    concreteResults = result.GetConcreteVerticesTension(gH_StressAnalysisResult.Value.PsiRebar.Value);
                else
                    concreteResults = result.GetConcreteVerticesTension();

                (ReinforcedConcreteRebar rebar, double tension)[] rebarResults;
                if (gH_StressAnalysisResult.Value.LinearElasticAnalysis)
                    rebarResults = result.GetRebarsTension(gH_StressAnalysisResult.Value.PsiRebar.Value, gH_StressAnalysisResult.Value.PsiTendon.Value);
                else
                    rebarResults = result.GetRebarsTension();

                List<Point2d> concretePoints = new List<Point2d>();
                List<double> concreteValues = new List<double>();
                List<Point2d> rebarPoints = new List<Point2d>();
                List<double> rebarValues = new List<double>();

                for (int i = 0; i < concreteResults.Length; i++)
                {
                    concretePoints.Add(new Point2d(concreteResults[i].point.X, concreteResults[i].point.Y));
                    concreteValues.Add(Math.Round(concreteResults[i].tension, cifresignificativeStress));
                }

                for (int i = 0; i < rebarResults.Length; i++)
                {
                    rebarPoints.Add(new Point2d(rebarResults[i].rebar.Position.X, rebarResults[i].rebar.Position.Y));
                    rebarValues.Add(Math.Round(rebarResults[i].tension, cifresignificativeStress));
                }

                GPC.Geometry.Line2d neutralAxis = strainPlane.GetNeutralAxisRespectReferencePoint();
                neutralAxis.Move(result.StrainPlane.ReferencePoint.X, result.StrainPlane.ReferencePoint.Y);
                LineCurve lineCurve = new LineCurve(new Point2d(neutralAxis.Start.X, neutralAxis.Start.Y), new Point2d(neutralAxis.End.X, neutralAxis.End.Y));

                GPC.Geometry.Meshes.Mesh mesh = (GPC.Geometry.Meshes.Mesh)section.Mesh.Clone();

                Rhino.Geometry.Line neutralAxisRhino = new Rhino.Geometry.Line(new Rhino.Geometry.Point3d(neutralAxis.Start.X, neutralAxis.Start.Y, 0), new Rhino.Geometry.Point3d(neutralAxis.End.X, neutralAxis.End.Y, 0));

                GPC.Geometry.Line2d cutLine = new GPC.Geometry.Line2d(neutralAxis);
                Rhino.Geometry.Curve cutLineRhino = lineCurve.Extend(Rhino.Geometry.CurveEnd.Both, 10, Rhino.Geometry.CurveExtensionStyle.Line);
                cutLine = new GPC.Geometry.Line2d(new GPC.Geometry.Point2d(cutLineRhino.PointAtStart.X, cutLineRhino.PointAtStart.Y), new GPC.Geometry.Point2d(cutLineRhino.PointAtEnd.X, cutLineRhino.PointAtEnd.Y));

                mesh.Cut(cutLine);
                mesh.Clean();
                mesh.Refine();

                Mesh rhinoMesh = ConvertToRhinoMesh(mesh);
                if (!smooth)
                {
                    rhinoMesh.Unweld(0.0, true); // duplica vertici per faccia
                    rhinoMesh.Normals.ComputeNormals();
                }

                Mesh outMesh = new Mesh();
                int num = 0;
                for (int j = 0; j < rhinoMesh.Faces.Count; j++)
                {
                    int num2 = num;
                    int[] verticesIds = new int[4];
                    MeshFace face = rhinoMesh.Faces.GetFace(j);
                    verticesIds[0] = face.A;
                    verticesIds[1] = face.B;
                    verticesIds[2] = face.C;
                    verticesIds[3] = face.D;
                    int numberOfVertices = (!face.IsQuad) ? 3 : 4;
                    Point3d centroid = Point3d.Unset;
                    try
                    {
                        centroid = rhinoMesh.Faces.GetFaceCenter(j);
                    }
                    catch (Exception)
                    { continue; }

                    for (int k = 0; k < numberOfVertices; k++)
                    {
                        Point3d verticesPoint = rhinoMesh.Vertices[verticesIds[k]];
                        outMesh.Vertices.Add(verticesPoint.X, verticesPoint.Y, verticesPoint.Z);
                        num++;

                        Color cc = Color.Black;
                        GPC.Geometry.Point2d point = null;

                        if (smooth)                        
                            point = new GPC.Geometry.Point2d(verticesPoint.X, verticesPoint.Y);                        
                        else                        
                            point = new GPC.Geometry.Point2d(centroid.X, centroid.Y);
                        
                        double concreteStress = 0;
                        if (result.LinearElasticAnalysis)
                            concreteStress = result.GetConcreteTension(result.PsiRebar.Value, point);
                        else
                            concreteStress = result.GetConcreteTension(point);

                        if (smooth)
                        {
                            cc = GetColor(concreteStress, section.ConcreteMaterial.CalculateDesignCompressiveStrength(result.Standard), 
                                section.ConcreteMaterial.CalculateDesignTensileStrength(result.Standard));
                        }
                        else
                        {
                            if (concreteStress == 0)
                                cc = Color.White;
                            else if (concreteStress < 0)
                                cc = Color.Red;
                            else if (concreteStress > 0)
                                cc = Color.Blue;
                        }

                        outMesh.VertexColors.Add(cc);
                    }

                    face = rhinoMesh.Faces[j];

                    if (face.IsQuad)
                        outMesh.Faces.AddFace(num2, num2 + 1, num2 + 2, num2 + 3);
                    else
                        outMesh.Faces.AddFace(num2, num2 + 1, num2 + 2);
                }


                DA.SetData(0, new GH_Mesh(outMesh));
            }
        }

        private Mesh ConvertToRhinoMesh(GPC.Geometry.Meshes.Mesh mesh)
        {
            Mesh rhinoMesh = new Mesh();

            Dictionary<int, int> idRhinoIndexMap = new Dictionary<int, int>();
            GPC.Geometry.Meshes.MeshVertex[] vertices = mesh.GetVertices();
            GPC.Geometry.Meshes.MeshFace[] faces = mesh.GetFaces();

            Dictionary<int, List<int>> indexMap = mesh.Vertices.GetElementIdMap();

            for (int v = 0; v < vertices.Length; v++)
            {
                int tag = rhinoMesh.Vertices.Add(vertices[v].Point.X, vertices[v].Point.Y, vertices[v].Point.Z);
                idRhinoIndexMap.Add(vertices[v].Id, tag);
            }

            for (int f = 0; f < faces.Length; f++)
            {
                if (faces[f].IsQuad)
                {
                    rhinoMesh.Faces.AddFace(
                        idRhinoIndexMap[mesh.Vertices.GetElementByIndex(indexMap[faces[f].A].FirstOrDefault()).Id],
                        idRhinoIndexMap[mesh.Vertices.GetElementByIndex(indexMap[faces[f].B].FirstOrDefault()).Id],
                        idRhinoIndexMap[mesh.Vertices.GetElementByIndex(indexMap[faces[f].C].FirstOrDefault()).Id],
                        idRhinoIndexMap[mesh.Vertices.GetElementByIndex(indexMap[faces[f].D].FirstOrDefault()).Id]
                    );
                }
                else
                {
                    rhinoMesh.Faces.AddFace(
                        idRhinoIndexMap[mesh.Vertices.GetElementByIndex(indexMap[faces[f].A].FirstOrDefault()).Id],
                        idRhinoIndexMap[mesh.Vertices.GetElementByIndex(indexMap[faces[f].B].FirstOrDefault()).Id],
                        idRhinoIndexMap[mesh.Vertices.GetElementByIndex(indexMap[faces[f].C].FirstOrDefault()).Id]
                    );
                }
            }

            rhinoMesh.Compact();

            return rhinoMesh;
        }


        public Color GetColor(double currentValue, double maxCompression, double maxTension)
        {
            // Definizione punti chiave
            double v00 = maxTension;
            double v01 = 0.0;
            double v02 = maxCompression / 3.0;
            double v03 = 2.0 * maxCompression / 3.0;
            double v04 = maxCompression;

            Color c01 = Color.White;
            Color c00 = Color.Blue;
            Color c02 = Color.Orange;
            Color c03 = Color.Red;
            Color c04 = Color.Magenta;

            // Pezzature
            if (currentValue <= v00 && currentValue >= v01)
                return LerpColor(c01, c00, (currentValue - v01) / (v00 - v01));

            else if (currentValue <= v01 && currentValue >= v02)
                return LerpColor(c02, c01, (currentValue - v02) / (v01 - v02));

            else if (currentValue <= v02 && currentValue >= v03)
                return LerpColor(c03, c02, (currentValue - v03) / (v02 - v03));

            else if (currentValue <= v03 && currentValue >= v04)
                return LerpColor(c04, c03, (currentValue - v04) / (v03 - v04));

            return Color.Black;
        }

        static Color LerpColor(Color a, Color b, double t)
        {
            t = Math.Max(0.0, Math.Min(1.0, t));

            int r = (int)(a.R + (b.R - a.R) * t);
            int g = (int)(a.G + (b.G - a.G) * t);
            int bC = (int)(a.B + (b.B - a.B) * t);

            return Color.FromArgb(r, g, bC);
        }


        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("b62a91c6-b7bb-4e99-800f-1fafb45707ad");

    }
}
