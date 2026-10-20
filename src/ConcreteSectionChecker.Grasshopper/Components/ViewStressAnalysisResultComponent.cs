using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ViewStressAnalysisResultComponent : GpcComponent
    {
        public ViewStressAnalysisResultComponent()
            : base("View Strain Analysis Results", "ISAR", "Strain Analysis Results")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis Results", "SA", "Stress Analysis Results", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Concrete Points", "CP", "Concrete Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Concrete Values", "CV", "Concrete Values", GH_ParamAccess.list);
            pManager.AddPointParameter("Rebar Points", "RP", "Rebar Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rebar Values", "RV", "Rebar Values", GH_ParamAccess.list);
            pManager.AddCurveParameter("Neutral Axis", "NA", "Neutral Axis", GH_ParamAccess.item);
            pManager.AddTextParameter("Combo Name", "CN", "Combo Name", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "M", "Mesh", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_StressAnalysisResult gH_StressAnalysisResult = null;

            if (DA.GetData(0, ref gH_StressAnalysisResult))
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
                //mesh.Refine();

                Mesh rhinoMesh = ConvertToRhinoMesh(mesh);
                rhinoMesh.CollapseFacesByArea(1, double.MaxValue);

                var count = 0;
                DA.SetDataList(count++, concretePoints);
                DA.SetDataList(count++, concreteValues);
                DA.SetDataList(count++, rebarPoints);
                DA.SetDataList(count++, rebarValues);
                DA.SetData(count++, new GH_Curve(lineCurve));
                DA.SetData(count++, $"{result.Force.Name}");
                DA.SetData(count++, new GH_Mesh(rhinoMesh));
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

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("65801921-575b-466e-8001-a7e833dbacb6");

    }
}
