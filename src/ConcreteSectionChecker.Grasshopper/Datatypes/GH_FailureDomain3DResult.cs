using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_FailureDomain3DResult : GH_Goo<FailureDomainResult>
    {
        public SectionCheckerModelCode2010 Checker { get; }

        public GH_FailureDomain3DResult(FailureDomainResult model, SectionCheckerModelCode2010 checker = null)
        {
            Value = model;
            Checker = checker;
        }

        public GH_FailureDomain3DResult()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "FailureDomainResult";

        public override string TypeDescription => "FailureDomainResult";

        public override IGH_Goo Duplicate()
        {
            return new GH_FailureDomain3DResult(Value, Checker);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_FailureDomain3DResult";
            return "";
        }
    }
}
