using ConcreteSectionChecker.Grasshopper;
using GPC.Model.Standards;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_Standard : ReBearGoo<StandardModelCode2010>
    {
        public GH_Standard(StandardModelCode2010 model) : base(model) { }
        public GH_Standard() { }
        public override string TypeName => "ReBear Standard";
        public override string TypeDescription => "Profilo normativo GPC.";
        public override IGH_Goo Duplicate() => new GH_Standard(Value); // created per output, never modified
        protected override string Describe(StandardModelCode2010 s) =>
            $"{s.Name}: γc={Persistence.Number(s.GammaC)}, γs={Persistence.Number(s.GammaS)}, γs,p={Persistence.Number(s.GammaSPrestress)}, " +
            $"αcc={Persistence.Number(s.AlphaCC)}, αct={Persistence.Number(s.AlphaCT)}, γc,acc={Persistence.Number(s.GammaCAccidental)}, γs,acc={Persistence.Number(s.GammaSAccidental)}";
    }
}
