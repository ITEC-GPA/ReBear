using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_ReinforcedConcreteSection : GH_Goo<ReinforcedConcreteSection>
    {
        public GH_ReinforcedConcreteSection(ReinforcedConcreteSection model)
        {
            Value = model;
        }

        public GH_ReinforcedConcreteSection()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "v";

        public override string TypeDescription => "MaterialModel";

        public override IGH_Goo Duplicate()
        {
            return new GH_ReinforcedConcreteSection(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_ReinforcedConcreteSection";
            return "Name: " + Value.Name+ ", \r\n" +
                "Concrete material: " + Value.ConcreteMaterial.Name + ", \r\n" +
                "Area: " + Value.Area + "mm2" + ", \r\n" +
                "Rebars: " + Value.Rebars.Count() + ", \r\n" +
                "Moment of inertia x-x: " + Value.Jxx + "mm4" + ", \r\n" +
                "Moment of inertia y-y: " + Value.Jyy + "mm4" + ", \r\n" +
                "Moment of inertia x-y: " + Value.Jxy + "mm4" + ", \r\n" +
                "Moment of inertia 1-1: " + Value.J11 + "mm4" + ", \r\n" +
                "Moment of inertia 2-2: " + Value.J22 + "mm4" + ", \r\n" +
                "Angle: " + double.RadiansToDegrees(Value.AngleX1) + "°" + ", \r\n" +
                "Centroid X: " + Value.Centroid.X + "mm" + ", \r\n" +
                "Centroid Y: " + Value.Centroid.Y + "mm" + ", \r\n" +
                "Rebars: " + Value.Rebars.Count();
        }
    }
}
