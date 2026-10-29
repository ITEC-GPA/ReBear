using System;
using GPC.Model.Materials;
using GPC.Model.Standards;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class ConcreteDesignValue : ReBearComponent
    {
        public ConcreteDesignValue() : base("Concrete Material Design Value", "CD", "Valori caratteristici e di progetto del calcestruzzo per la norma; MPa, moduli positivi.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000006");
        protected override Port[] Inputs => new[] { G("Concrete Material", "CM", "Calcestruzzo GPC."), G("Standard", "S", "Norma GPC.") };
        protected override Port[] Outputs => new[] { T("Name", "Name", "Classe."), N("fck", "fck", "Resistenza caratteristica cilindrica, MPa."),
            N("fcm", "fcm", "Resistenza media, MPa."), N("Elastic modulus", "E", "Modulo elastico in compressione, MPa."),
            N("εc2", "εc2", "Deformazione al limite elastico in compressione (adimensionale, modulo)."),
            N("εcu", "εcu", "Deformazione ultima in compressione (adimensionale, modulo)."),
            N("fcd", "fcd", "Resistenza di progetto a compressione, MPa."), N("fctd", "fctd", "Resistenza di progetto a trazione, MPa.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var c = Required<ConcreteMaterialEuropeanCommon>(d, 0); var s = Required<StandardModelCode2010>(d, 1);
            object[] values = { c.Name, Math.Abs(c.Fck), Math.Abs(c.Fcm), Math.Abs(c.ElasticModulusCompression), Math.Abs(c.StrainYCompression),
                Math.Abs(c.StrainUCompression), Math.Abs(c.CalculateFcd(s)), Math.Abs(c.CalculateFctd(s)) };
            for (int i = 0; i < values.Length; i++) d.SetData(i, values[i]);
        }
    }

    public sealed class SteelDesignValue : ReBearComponent
    {
        public SteelDesignValue() : base("Steel Material Design Value", "SD", "Valori caratteristici e di progetto dell'acciaio per la norma; MPa, deformazioni adimensionali.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000007");
        protected override Port[] Inputs => new[] { G("Steel Material", "SM", "Acciaio GPC."), G("Standard", "S", "Norma GPC.") };
        protected override Port[] Outputs => new[] { T("Name", "Name", "Qualità."), N("Elastic modulus", "E", "Modulo elastico, MPa."),
            N("fyk", "fyk", "Snervamento caratteristico, MPa."), N("fu", "fu", "Resistenza a rottura, MPa."), N("Et", "Et", "Modulo incrudente, MPa."),
            N("εy", "εy", "Deformazione di snervamento caratteristica."), N("εu", "εu", "Deformazione ultima caratteristica."),
            N("fyd", "fyd", "Snervamento di progetto, MPa."), N("fud", "fud", "Tensione di progetto alla deformazione ultima, MPa."),
            N("εyd", "εyd", "Deformazione di snervamento di progetto."), N("εud", "εud", "Deformazione ultima di progetto.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var m = Required<SteelMaterialEN1992>(d, 0); var s = Required<StandardModelCode2010>(d, 1);
            object[] values = { m.Name, m.ElasticModulusTension, m.Fyk, m.Fu, m.Et, m.StrainYTension, m.StrainUTension,
                m.CalculateDesignYieldingStressTension(s), m.CalculateDesignStress(s, m.StrainUTension),
                m.CalculateDesignYieldingStrainTension(s), m.CalculateDesignUltimateStrainTension(s) };
            for (int i = 0; i < values.Length; i++) d.SetData(i, values[i]);
        }
    }
}
