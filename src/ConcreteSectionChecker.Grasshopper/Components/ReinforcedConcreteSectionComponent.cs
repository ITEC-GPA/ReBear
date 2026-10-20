using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ReinforcedConcreteSectionComponent : GpcComponent
    {
        public ReinforcedConcreteSectionComponent()
            : base("Reinforced Concrete Section", "RCS", "Reinforced Concrete Section")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Section Name", "n", "The section name", GH_ParamAccess.item, "");
            pManager.AddBrepParameter("Surface", "S", "The section surface", GH_ParamAccess.item);
            pManager.AddGenericParameter("Concrete Material", "M", "The concrete material", GH_ParamAccess.item);
            pManager.AddGenericParameter("Rebar elements", "RE", "The rebar element list", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Reinforced Concrete Section", "RCS", "Reinforced Concrete Section", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            string name = string.Empty;
            GH_Brep brep = null;
            GH_ConcreteMaterial concreteMaterial = null;
            List<GH_Rebar> rebarElements = new List<GH_Rebar>();

            if (DA.GetData(0, ref name) && DA.GetData(1, ref brep) && DA.GetData(2, ref concreteMaterial) && DA.GetDataList(3, rebarElements))
            {
                List<ReinforcedConcreteRebar> rebars = new List<ReinforcedConcreteRebar>();
                for (int i = 0; i < rebarElements.Count; i++)
                {
                    GH_Rebar rebarElement = rebarElements[i];
                    if (rebarElement != null && rebarElement.Value != null)
                        rebars.Add(rebarElement.Value);
                }

                // Rebars use their XY coordinates. Do not rotate only the concrete outline.
                Brep b = brep.Value;
                if (b is null || b.Faces.Count != 1)
                    throw new ArgumentException("La sezione richiede una sola faccia piana, eventualmente con fori. Unire prima le superfici.");
                var face = b.Faces[0];
                if (!face.TryGetPlane(out Plane plane) || Math.Abs(plane.Normal.Z) < 1 - 1e-8)
                    throw new ArgumentException("La sezione e le barre devono essere riferite al piano XY. Portare la geometria nel piano prima della verifica.");
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
                var shape = new GPC.Geometry.Shape2d(Polygon(face.OuterLoop), holes.ToArray());
                ReinforcedConcreteSection reinforcedConcreteSection = new ReinforcedConcreteSection(shape, concreteMaterial.Value, name);
                reinforcedConcreteSection.AddRebars(rebars);

                DA.SetData(0, new GH_ReinforcedConcreteSection(reinforcedConcreteSection));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("f247cd41-e5cb-4a35-b9b8-e06b4c43afc6");

    }
}
