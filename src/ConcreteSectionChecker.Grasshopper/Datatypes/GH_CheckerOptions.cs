using ConcreteSectionChecker.Adapters;
using ConcreteSectionChecker.Grasshopper;
using GPC.Checkers.Concrete.Checkers;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_CheckerOptions : ReBearGoo<SectionCheckerModelCode2010.SectionOptionsModelCode2010>
    {
        public GH_CheckerOptions(SectionCheckerModelCode2010.SectionOptionsModelCode2010 model) : base(model) { }
        public GH_CheckerOptions() { }
        public override string TypeName => "ReBear Checker Options";
        public override string TypeDescription => "Opzioni del checker GPC (dominio, criterio, strategia, analisi tensionale).";
        // Options are mutable and the GPC solver keeps a reference to them: a duplicate is an independent copy.
        public override IGH_Goo Duplicate() => new GH_CheckerOptions(Value is null ? null : SectionAnalyses.Copy(Value));
        protected override string Describe(SectionCheckerModelCode2010.SectionOptionsModelCode2010 o) => Describe(o, true);
        internal static string Describe(SectionCheckerModelCode2010.SectionOptionsModelCode2010 o, bool axes) =>
            $"Dominio {o.FailureDomainType}, criterio {o.FailureAnalysisType}, strategia {o.DomainPointStrategy}, θ={o.TetaDiscretization}; " +
            $"analisi {o.StressAnalysisType}, ψ barre={Persistence.Number(o.PsiCoefficientRebar)}, ψ trefoli={Persistence.Number(o.PsiCoefficientTendon)}, " +
            $"cls teso={(o.ConsiderTensileConcrete ? "sì" : "no")}" + (axes ? "; assi " + GH_CoordinateSystem.Text(o.ForceReferenceCoordinateSystem) : "");
    }
}
