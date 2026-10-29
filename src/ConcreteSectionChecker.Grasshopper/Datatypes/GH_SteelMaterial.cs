using ConcreteSectionChecker.Grasshopper;
using GPC.Model.Materials;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_SteelMaterial : ReBearGoo<SteelMaterialEN1992>
    {
        public GH_SteelMaterial(SteelMaterialEN1992 model) : base(model) { }
        public GH_SteelMaterial() { }
        public override string TypeName => "ReBear Steel";
        public override string TypeDescription => "Acciaio per barre o trefoli del database GPC.";
        public override IGH_Goo Duplicate() => new GH_SteelMaterial(Value); // database materials are created per output and never modified
        protected override string Describe(SteelMaterialEN1992 s) =>
            $"{s.Name} ({s.SteelType}, {s.StressStrainCurve}): Es={Persistence.Number(s.E)} MPa, fyk={Persistence.Number(s.Fyk)} MPa, " +
            $"fu={Persistence.Number(s.Fu)} MPa, εuk={Persistence.Number(s.StrainUTension)}";
    }
}
