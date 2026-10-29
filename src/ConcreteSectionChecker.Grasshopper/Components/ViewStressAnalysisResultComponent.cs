using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Results;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    internal static class StressDisplay
    {
        /// <summary>Neutral axis output, or a warning when the plane has no curvature (B5: no axis instead of an exception).</summary>
        public static LineCurve Axis(GH_Component component, (GPC.Geometry.Point2d Start, GPC.Geometry.Point2d End)? axis)
        {
            if (axis.HasValue) return new LineCurve(new Point3d(axis.Value.Start.X, axis.Value.Start.Y, 0), new Point3d(axis.Value.End.X, axis.Value.End.Y, 0));
            component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Piano di deformazione senza curvatura (per esempio azioni nulle o compressione uniforme): asse neutro assente, mesh non suddivisa.");
            return null;
        }
    }

    public sealed class ViewStressAnalysisResultComponent : ReBearComponent
    {
        public ViewStressAnalysisResultComponent() : base("View Strain Analysis Results", "VSAR",
            "Tensioni ai vertici del calcestruzzo e nelle barre (MPa), asse neutro e mesh della sezione suddivisa sull'asse.") { }
        public override Guid ComponentGuid => new("65801921-575b-466e-8001-a7e833dbacb6");
        protected override Port[] Inputs => new[] { G("Stress Analysis Results", "SA", "Analisi tensionale GPC.") };
        protected override Port[] Outputs => new[] { List(P("Concrete Points", "CP", "Vertici del contorno, mm.")),
            List(N("Concrete Values", "CV", "Tensioni del calcestruzzo, MPa.")), List(P("Rebar Points", "RP", "Centri delle barre, mm.")),
            List(N("Rebar Values", "RV", "Tensioni delle barre, MPa.")), C("Neutral Axis", "NA", "Asse neutro esteso alla sezione; assente senza curvatura."),
            T("Combo Name", "CN", "Nome della combinazione."), M("Mesh", "M", "Mesh della sezione suddivisa sull'asse neutro.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var result = Required<StressAnalysisResult>(d, 0);
            var concrete = SectionMeshes.ConcreteVertexStresses(result); var bars = SectionMeshes.BarStresses(result);
            var mesh = RhinoMeshes.ToRhino(SectionMeshes.StressMesh(result, false, out var axis));
            mesh.CollapseFacesByArea(1, double.MaxValue); // slivers below 1 mm² left by the cut
            d.SetDataList(0, concrete.Select(p => new Point3d(p.X, p.Y, 0))); d.SetDataList(1, concrete.Select(p => p.Stress));
            d.SetDataList(2, bars.Select(p => new Point3d(p.X, p.Y, 0))); d.SetDataList(3, bars.Select(p => p.Stress));
            d.SetData(4, StressDisplay.Axis(this, axis)); d.SetData(5, LoadEnvelopes.Name(result.Force)); d.SetData(6, new GH_Mesh(mesh));
        }
    }

    public sealed class ColorGradientMeshComponent : ReBearComponent
    {
        public ColorGradientMeshComponent() : base("Result Color Mesh", "RCM",
            "Mesh colorata delle tensioni del calcestruzzo: gradiente fra fctd (blu) e fcd (magenta), nero oltre; senza Smooth colori per segno.") { }
        public override Guid ComponentGuid => new("b62a91c6-b7bb-4e99-800f-1fafb45707ad");
        protected override Port[] Inputs => new[] { G("Stress Analysis Results", "SA", "Analisi tensionale GPC."),
            B("Smooth", "s", "True gradiente per vertice; false colore per faccia secondo il segno.", true) };
        protected override Port[] Outputs => new[] { M("Mesh", "msh", "Mesh colorata, mm.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var result = Required<StressAnalysisResult>(d, 0);
            var mesh = SectionMeshes.StressColours(result, Read(d, 1, true), out var axis);
            if (!axis.HasValue) StressDisplay.Axis(this, axis);
            d.SetData(0, new GH_Mesh(RhinoMeshes.ToRhino(mesh)));
        }
    }

    public sealed class ColorMeshComponent : ReBearComponent
    {
        public ColorMeshComponent() : base("Color Mesh", "CM", "Colora una mesh con il colore del punto più vicino (al vertice se Smooth, al baricentro della faccia altrimenti).") { }
        public override Guid ComponentGuid => new("1c72c75b-9ede-461a-8553-973173695f79");
        protected override Port[] Inputs => new[] { List(P("Points", "pts", "Punti campione.")), List(Colour("Colours", "col", "Un colore per punto, oppure uno solo.")),
            M("Mesh", "msh", "Mesh da colorare."), B("Smooth", "s", "True colore per vertice; false per faccia.", true) };
        protected override Port[] Outputs => new[] { M("Mesh", "msh", "Mesh colorata.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var points = ReadList<Point3d>(d, 0).Select(p => (p.X, p.Y, p.Z)).ToArray();
            var colours = ReadList<Color>(d, 1).Select(RhinoMeshes.Rgb).ToArray();
            var mesh = Required<Mesh>(d, 2);
            d.SetData(0, new GH_Mesh(RhinoMeshes.ToRhino(SectionMeshes.NearestColours(RhinoMeshes.FromRhino(mesh), points, colours, Read(d, 3, true)))));
        }
    }

    public sealed class ViewStressAnalysisResultMeshComponent : ReBearComponent
    {
        public ViewStressAnalysisResultMeshComponent() : base("Inspect Strain Analysis Results", "ISAR", "Tensione del calcestruzzo nei punti assegnati, MPa.") { }
        public override Guid ComponentGuid => new("8ae4225f-e39f-4304-bbdb-2610c34dd354");
        protected override Port[] Inputs => new[] { G("Stress Analysis Results", "SA", "Analisi tensionale GPC."), List(P("Concrete Points", "CP", "Punti in XY, mm.")) };
        protected override Port[] Outputs => new[] { List(N("Concrete Stress", "CS", "Tensioni, MPa (compressione negativa).")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var result = Required<StressAnalysisResult>(d, 0);
            d.SetDataList(0, ReadList<Point3d>(d, 1).Select(p => SectionMeshes.ConcreteStress(result, p.X, p.Y)));
        }
    }
}
