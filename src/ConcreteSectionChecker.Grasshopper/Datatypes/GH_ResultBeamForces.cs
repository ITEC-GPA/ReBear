using ConcreteSectionChecker.Grasshopper;
using GPC.Model.Results;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_ResultBeamForces : ReBearGoo<ResultBeamForces>
    {
        public GH_ResultBeamForces(ResultBeamForces model) : base(model) { }
        public GH_ResultBeamForces() { }
        public override string TypeName => "ReBear Forces";
        public override string TypeDescription => "Combinazione di azioni (interne GPC in N e Nmm), compressione negativa.";
        public override IGH_Goo Duplicate() => new GH_ResultBeamForces(Value); // GPC forces are never modified by ReBear
        protected override string Describe(ResultBeamForces f) =>
            $"'{f.Name}': N={Persistence.Number(f.N / 1000)} kN, Vx={Persistence.Number(f.V1 / 1000)} kN, Vy={Persistence.Number(f.V2 / 1000)} kN, " +
            $"T={Persistence.Number(f.T / 1e6)} kNm, Mx={Persistence.Number(f.M1 / 1e6)} kNm, My={Persistence.Number(f.M2 / 1e6)} kNm; assi {GH_CoordinateSystem.Text(f.CoordinateSystem)}";
    }
}
