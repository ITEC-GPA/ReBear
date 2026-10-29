using ConcreteSectionChecker.Grasshopper;
using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_SectionChecker : ReBearGoo<SectionCheckerModelCode2010>
    {
        public GH_SectionChecker(SectionCheckerModelCode2010 model) : base(model) { }
        public GH_SectionChecker() { }
        public override string TypeName => "ReBear Checker";
        public override string TypeDescription => "Checker GPC: sezione, norma e opzioni.";
        public override IGH_Goo Duplicate() => new GH_SectionChecker(Value); // ReBear components never modify a checker
        protected override string Describe(SectionCheckerModelCode2010 c) =>
            $"Checker {c.StandardModelCode2010?.Name} · " +
            (c.SectionCheckerAttribute.Section is ReinforcedConcreteSection s ? GH_ReinforcedConcreteSection.Text(s) : c.SectionCheckerAttribute.Section?.GetType().Name) +
            " · " + GH_CheckerOptions.Describe(c.SectionCheckerOptionsModelCode2010, true);
    }
}
