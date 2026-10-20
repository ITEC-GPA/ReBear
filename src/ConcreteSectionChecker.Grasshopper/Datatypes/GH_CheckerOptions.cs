using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_CheckerOptions : GH_Goo<SectionCheckerModelCode2010.SectionOptionsModelCode2010>
    {
        public GH_CheckerOptions(SectionCheckerModelCode2010.SectionOptionsModelCode2010 model)
        {
            Value = model;
        }

        public GH_CheckerOptions()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "v";

        public override string TypeDescription => "SectionOptions";

        public override IGH_Goo Duplicate()
        {
            return new GH_CheckerOptions(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_CheckerOptions";
            return "";
        }
    }
}
