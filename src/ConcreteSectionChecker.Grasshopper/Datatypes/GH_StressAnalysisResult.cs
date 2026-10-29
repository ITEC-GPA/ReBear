using ConcreteSectionChecker.Adapters;
using ConcreteSectionChecker.Grasshopper;
using GPC.Checkers.Concrete.Results;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_StressAnalysisResult : ReBearGoo<StressAnalysisResult>
    {
        public GH_StressAnalysisResult(StressAnalysisResult model) : base(model) { }
        public GH_StressAnalysisResult() { }
        public override string TypeName => "ReBear Stress Analysis";
        public override string TypeDescription => "Analisi tensionale GPC (non salvata nel file: ricalcolare).";
        public override IGH_Goo Duplicate() => new GH_StressAnalysisResult(Value);
        protected override string Describe(StressAnalysisResult r) =>
            $"Analisi {(r.LinearElasticAnalysis ? "lineare ψ=" + Persistence.Number(r.PsiRebar ?? 0) : "non lineare")} '{LoadEnvelopes.Name(r.Force)}': " +
            $"N={Persistence.Number(r.Force.N / 1000)} kN, Mx={Persistence.Number(r.Force.M1 / 1e6)} kNm, My={Persistence.Number(r.Force.M2 / 1e6)} kNm, " +
            $"χ={Persistence.Number((r.StrainPlane?.Chi ?? double.NaN) * 1000)} 1/m";
    }
}
