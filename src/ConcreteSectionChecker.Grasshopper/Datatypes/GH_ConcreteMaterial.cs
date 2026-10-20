using Grasshopper.Kernel.Types;
using GPC.Model.Materials;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_ConcreteMaterial : GH_Goo<ConcreteMaterialEuropeanCommon>
    {
        public GH_ConcreteMaterial(ConcreteMaterialEuropeanCommon model)
        {
            Value = model;
        }

        public GH_ConcreteMaterial()
        {

        }

        public override bool IsValid => Value is not null;

        public override string TypeName => "v";

        public override string TypeDescription => "MaterialModel";

        public override IGH_Goo Duplicate()
        {
            return new GH_ConcreteMaterial(Value);
        }

        public override string ToString()
        {
            if (Value is null) return "Empty GH_ConcreteMaterial";
            return "Name: " + Value.Name + ", \r\n" +
                "Elastic modulus: " + Value.ElasticModulusCompression + "MPa" + ", \r\n" +
                "Fck: " + Value.Fck + "MPa" + ", \r\n" +
                "Ultimate strength: " + Value.StressUCompression + "MPa" + ", \r\n" +
                "Strain at maximum strength: " + Value.StrainUCompression + "‰" + ", \r\n" +
                "Strain at yielding: " + Value.StrainYCompression + "‰" + ", \r\n" +
                "Density: " + Value.Density + "kg/m3" + ", \r\n" +
                "Thermal expansion coefficient: " + Value.AlfaThermalExpansion + "1/°C";
        }
    }
}
