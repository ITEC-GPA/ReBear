using GPC.Model.Results;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_ResultBeamForces : GH_Goo<ResultBeamForces>
    {
        public GH_ResultBeamForces(ResultBeamForces model)
        {
            Value = model;
        }

        public GH_ResultBeamForces()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "ResultBeamForces";

        public override string TypeDescription => "ResultBeamForces";

        public override IGH_Goo Duplicate()
        {
            return new GH_ResultBeamForces(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_ResultBeamForces";
            return "Name: " + Value.Name + ", \r\n" +
                "Axial Force: " + Value.N / 1000 + "kN" + ", \r\n" +
                "Shear X: " + Value.V1 / 1000 + "kN" + ", \r\n" +
                "Shear Y: " + Value.V2 / 1000 + "kN" + ", \r\n" +
                "Torsion: " + Value.T / 1000000 + "kNm" + ", \r\n" +
                "Bending X: " + Value.M1 / 1000000 + "kNm" + ", \r\n" +
                "Bending Y: " + Value.M2 / 1000000 + "kNm";
        }
    }
}
