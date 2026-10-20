using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_Rebar : GH_GeometricGoo<ReinforcedConcreteRebar>, IGH_PreviewData
    {
        public GH_Rebar(ReinforcedConcreteRebar model)
        {
            Value = model;
        }

        public GH_Rebar()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "ReinforcedConcreteRebar";

        public override string TypeDescription => "ReinforcedConcreteRebar";


        BoundingBox IGH_PreviewData.ClippingBox => Boundingbox;

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value is null)
                    return BoundingBox.Empty;
                else
                    return new Circle(new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2).BoundingBox;
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GH_Rebar(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_Rebar";
            return "Position: " + Value.Position.ToString() + ", \r\n" +
                "Diameter: " + Value.RebarSection.Diameter + "mm" + ", \r\n" +
                "Area: " + Value.Area + "mm2" + ", \r\n" +
                "Material: " + Value.RebarMaterial.Name + ", \r\n" +
                "Pretension: " + Value.EpsilonP * Value.RebarMaterial.E + "MPa";
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return (IGH_GeometricGoo)Duplicate();
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (!IsValid) return BoundingBox.Empty;
            var bounds = new Circle(new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2).BoundingBox;
            bounds.Transform(xform);
            return bounds;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            if (!IsValid) return null;
            var circle = new Circle(new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2);
            if (!circle.Transform(xform) || Math.Abs(circle.Normal.Z) < 1 - 1e-10 || Math.Abs(circle.Center.Z) > 1e-8)
                return null; // A section rebar must remain a circle in the XY analysis plane.
            var section = new GPC.Model.Sections.Rebar.RebarSectionCircular(circle.Diameter, Value.RebarMaterial);
            return new GH_Rebar(new ReinforcedConcreteRebar(section, new GPC.Geometry.Point2d(circle.Center.X, circle.Center.Y),
                Value.EpsilonP * Value.RebarMaterial.E, Value.Id, Value.Name));
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            return null; // A general morph cannot preserve a circular analytical rebar.
        }

        void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
        {
            if (!IsValid) return;
            args.Pipeline.DrawCircle(new Circle(new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2.0), args.Color);
        }

        void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            if (!IsValid) return;
            args.Pipeline.DrawCircle(new Circle(new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2.0), args.Material.Diffuse);
        }

        #region Casting methods

        public override bool CastTo<Q>(out Q target)
        {
            if (!IsValid) { target = default(Q); return false; }
            if (typeof(Q) == typeof(GH_Curve))
            {
                Circle circle = new Circle(Plane.WorldXY, new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2.0);
                target = (Q)Convert.ChangeType(new GH_Curve(circle.ToNurbsCurve()), typeof(Q));
                return true;
            }
            else if (typeof(Q) == typeof(GH_Circle))
            {
                Circle circle = new Circle(Plane.WorldXY, new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2.0);
                target = (Q)Convert.ChangeType(new GH_Circle(circle), typeof(Q));
                return true;
            }

            target = default(Q);
            return false;
        }

        #endregion
    }
}
