using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_StressAnalysisResult : GH_Goo<StressAnalysisResult>
    {
        public GH_StressAnalysisResult(StressAnalysisResult model)
        {
            Value = model;
        }

        public GH_StressAnalysisResult()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "StressAnalysisResult";

        public override string TypeDescription => "StressAnalysisResult";

        public override IGH_Goo Duplicate()
        {
            return new GH_StressAnalysisResult(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_StressAnalysisResult";
            return "";
        }
    }
}
