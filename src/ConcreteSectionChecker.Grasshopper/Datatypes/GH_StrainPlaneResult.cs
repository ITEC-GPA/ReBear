using ConcreteSectionChecker.Adapters;
using ConcreteSectionChecker.Grasshopper;
using GPC.Checker.Results.ResultType;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_StrainPlaneResult : ReBearGoo<StrainPlaneResult>
    {
        public GH_StrainPlaneResult(StrainPlaneResult model) : base(model) { }
        public GH_StrainPlaneResult() { }
        public override string TypeName => "ReBear Strain Plane";
        public override string TypeDescription => "Estremi tensionali e deformativi di un'analisi GPC (non salvati nel file: ricalcolare).";
        public override IGH_Goo Duplicate() => new GH_StrainPlaneResult(Value);
        protected override string Describe(StrainPlaneResult value) => StressSummary.From(value).ToString();
    }
}
