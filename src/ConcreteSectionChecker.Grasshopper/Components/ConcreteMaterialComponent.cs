using System;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Data.Concrete;
using GPC.Model.Materials;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class ConcreteMaterialComponent : ReBearComponent
    {
        private static readonly (string Name, Func<ConcreteMaterialEN1992> Create)[] Classes =
        {
            ("C20/25", () => ConcreteMaterialEN1992Data.C20_25), ("C25/30", () => ConcreteMaterialEN1992Data.C25_30),
            ("C28/35", () => ConcreteMaterialEN1992Data.C28_35), ("C30/37", () => ConcreteMaterialEN1992Data.C30_37),
            ("C32/40", () => ConcreteMaterialEN1992Data.C32_40), ("C35/45", () => ConcreteMaterialEN1992Data.C35_45),
            ("C40/50", () => ConcreteMaterialEN1992Data.C40_50), ("C45/55", () => ConcreteMaterialEN1992Data.C45_55),
            ("C50/60", () => ConcreteMaterialEN1992Data.C50_60), ("C55/67", () => ConcreteMaterialEN1992Data.C55_67),
            ("C60/75", () => ConcreteMaterialEN1992Data.C60_75), ("C70/85", () => ConcreteMaterialEN1992Data.C70_85),
            ("C80/95", () => ConcreteMaterialEN1992Data.C80_95), ("C90/105", () => ConcreteMaterialEN1992Data.C90_105),
        };
        public ConcreteMaterialComponent() : base("Database Concrete", "DCM", "Calcestruzzo del database GPC (EN 1992-1-1); una nuova istanza per ogni uscita.") { }
        public override Guid ComponentGuid => new("e2aa1e8b-3fa9-49a5-8d5e-991d4eac5bc6");
        protected override Port[] Inputs => new[] { I("Type", "T", "Classe di resistenza, 0 = C20/25 … 13 = C90/105.", 1,
            Array.ConvertAll(Classes, c => c.Name) is var names ? Named(names) : null) };
        protected override Port[] Outputs => new[] { G("Concrete Material", "CM", "Calcestruzzo GPC.") };
        protected override void SolveGpc(IGH_DataAccess d) => d.SetData(0, new GH_ConcreteMaterial(Classes[Choice(d, 0, Classes.Length)].Create()));
    }
}
