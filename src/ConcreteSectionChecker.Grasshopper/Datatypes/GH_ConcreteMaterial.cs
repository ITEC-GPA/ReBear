using ConcreteSectionChecker.Grasshopper;
using GPC.Model.Materials;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_ConcreteMaterial : ReBearGoo<ConcreteMaterialEuropeanCommon>
    {
        public GH_ConcreteMaterial(ConcreteMaterialEuropeanCommon model) : base(model) { }
        public GH_ConcreteMaterial() { }
        public override string TypeName => "ReBear Concrete";
        public override string TypeDescription => "Calcestruzzo del database GPC.";
        public override IGH_Goo Duplicate() => new GH_ConcreteMaterial(Value); // database materials are created per output and never modified
        protected override string Describe(ConcreteMaterialEuropeanCommon c) =>
            $"{c.Name}: fck={Persistence.Number(c.Fck)} MPa, Ecm={Persistence.Number(c.Ecm)} MPa, fctm={Persistence.Number(c.Fctm)} MPa, " +
            $"εc2={Persistence.Number(c.StrainYCompression)}, εcu={Persistence.Number(c.StrainUCompression)}, ρ={Persistence.Number(c.Density)} kg/m³";
    }
}
