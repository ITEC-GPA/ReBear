using Grasshopper.Kernel.Types;
using GPC.Model.Materials;
using GPC.Geometry;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_CoordinateSystem : GH_Goo<CoordinateSystem>
    {
        public GH_CoordinateSystem(CoordinateSystem model)
        {
            Value = model;
        }

        public GH_CoordinateSystem()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "CoordinateSystem";

        public override string TypeDescription => "CoordinateSystem";

        public override IGH_Goo Duplicate()
        {
            return new GH_CoordinateSystem(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_CoordinateSystem";
            return "Name: " + Value.Name + ", \r\n" +
                "Origin: " + Value.Origin + ", \r\n" +
                "X " + Value.V1 + ", \r\n" +
                "Y " + Value.V2 + ", \r\n" +
                "Z " + Value.V3 + ", \r\n";
        }
    }
}
