using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class FailureDomain3DComponent : GpcComponent
    {
        public FailureDomain3DComponent()
            : base("3D Failure Domain Check", "FDA", "3D Failure domain Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER   )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("3D Failure Domain Analysis", "FDA", "3D Failure Domain Analysis", GH_ParamAccess.item);
            pManager.AddPointParameter("3D Failure Domain Points", "FDP", "3D Failure Domain", GH_ParamAccess.list);
            pManager.AddMeshParameter("3D Failure Domain Mesh", "FDM", "3D Failure Domain", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_SectionChecker gH_SectionChecker = null;

            if (DA.GetData(0, ref gH_SectionChecker))
            {
                GPC.Checkers.Concrete.Results.FailureDomainResult result = gH_SectionChecker.Value.GetFailureDomainResult();

                List<Rhino.Geometry.Point3d> domainPoints = new List<Rhino.Geometry.Point3d>();
                for (int i = 0; i < result.Domain.DomainPoints.Length; i++)
                {
                    for (int j = 0; j < result.Domain.DomainPoints[i].Length; j++)
                    {
                        domainPoints.Add(new Rhino.Geometry.Point3d(result.Domain.DomainPoints[i][j].Point.X / 1000000.0,
                            result.Domain.DomainPoints[i][j].Point.Y / 1000000.0,
                            result.Domain.DomainPoints[i][j].Point.Z / 1000.0));
                    }
                }

                var rhinoMesh = ConvertToRhinoMesh(result.Domain.GetMesh(out _));
                Transform transform = Transform.Scale(Rhino.Geometry.Plane.WorldXY, 1.0 / 1000000, 1.0 / 1000000, 1.0 / 1000);
                rhinoMesh.Transform(transform);

                DA.SetData(0, new GH_FailureDomain3DResult(result, gH_SectionChecker.Value));
                DA.SetDataList(1, domainPoints);
                DA.SetData(2, new GH_Mesh(rhinoMesh));
            }
        }

        //protected override Bitmap Icon => Resources.material;

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

        public override Guid ComponentGuid => new Guid("a4c3e502-6c1e-4954-a9ef-7ac9e8bab5ef");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
