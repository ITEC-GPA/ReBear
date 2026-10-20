using GPC.Checker.Results.ResultType;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_StrainPlaneResult : GH_Goo<StrainPlaneResult>
    {
        public GH_StrainPlaneResult(StrainPlaneResult model)
        {
            Value = model;
        }

        public GH_StrainPlaneResult()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "StrainPlaneResult";

        public override string TypeDescription => "StrainPlaneResult";

        public override IGH_Goo Duplicate()
        {
            return new GH_StrainPlaneResult(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_StrainPlaneResult";
            return "";
        }
    }
}
