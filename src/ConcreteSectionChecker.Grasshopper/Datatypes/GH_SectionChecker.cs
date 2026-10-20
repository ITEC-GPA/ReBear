using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_SectionChecker : GH_Goo<SectionCheckerModelCode2010>
    {
        public GH_SectionChecker(SectionCheckerModelCode2010 model)
        {
            Value = model;
        }

        public GH_SectionChecker()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "SectionCheckerModelCode2010";

        public override string TypeDescription => "SectionCheckerModelCode2010";

        public override IGH_Goo Duplicate()
        {
            return new GH_SectionChecker(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_SectionChecker";
            return "";
        }
    }
}
