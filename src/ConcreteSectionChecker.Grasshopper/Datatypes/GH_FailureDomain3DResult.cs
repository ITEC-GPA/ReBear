using System.Linq;
using ConcreteSectionChecker.Grasshopper;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Results;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_FailureDomain3DResult : ReBearGoo<FailureDomainResult>
    {
        /// <summary>The checker that produced the domain: the demand points are solved with the same engine.</summary>
        public SectionCheckerModelCode2010 Checker { get; }
        public GH_FailureDomain3DResult(FailureDomainResult model, SectionCheckerModelCode2010 checker = null) : base(model) { Checker = checker; }
        public GH_FailureDomain3DResult() { }
        public override string TypeName => "ReBear Failure Domain";
        public override string TypeDescription => "Dominio resistente 3D GPC (non salvato nel file: ricalcolare).";
        public override IGH_Goo Duplicate() => new GH_FailureDomain3DResult(Value, Checker);
        protected override string Describe(FailureDomainResult value)
        {
            var points = value.Domain?.DomainPoints?.SelectMany(p => p).ToArray();
            if (points is null || points.Length == 0) return "Dominio 3D senza punti";
            return $"Dominio 3D: {points.Length} punti; N ∈ [{Persistence.Number(points.Min(p => p.Point.Z) / 1000)}; {Persistence.Number(points.Max(p => p.Point.Z) / 1000)}] kN";
        }
    }
}
