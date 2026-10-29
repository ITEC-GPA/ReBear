using System;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    /// <summary>Steel from the GPC database with the selected stress-strain law; every output is a new material instance.</summary>
    public abstract class SteelDatabaseComponent : ReBearComponent
    {
        protected SteelDatabaseComponent(string name, string nick, string description) : base(name, nick, description) { }
        // Read while the base constructor registers the ports: derived classes return static data.
        protected abstract (string Name, Func<SteelMaterialEN1992> Create)[] Grades { get; }
        protected abstract int DefaultGrade { get; }
        protected abstract (string Name, string Nick) Output { get; }
        protected override Port[] Inputs => new[] {
            I("Type", "T", "Qualità dell'acciaio (indice della lista).", DefaultGrade, Named(Array.ConvertAll(Grades, g => g.Name))),
            I("Behaviour", "B", "0 elastico-perfettamente plastico; 1 elastico-incrudente.", 0, Named("Elasto-Plastic", "Elasto-Hardening")) };
        protected override Port[] Outputs => new[] { G(Output.Name, Output.Nick, "Acciaio GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var steel = Grades[Choice(d, 0, Grades.Length)].Create();
            if (Choice(d, 1, 2) == 1) steel.StressStrainCurve = SteelMaterial.StressStrainCurveType.ElasticHardening;
            d.SetData(0, new GH_SteelMaterial(steel));
        }
    }

    public sealed class RebarMaterialComponent : SteelDatabaseComponent
    {
        private static readonly (string, Func<SteelMaterialEN1992>)[] Items =
        {
            ("B450A", () => SteelMaterialEN1992Data.B450A), ("B450C", () => SteelMaterialEN1992Data.B450C),
            ("B500A", () => SteelMaterialEN1992Data.B500A), ("B500B", () => SteelMaterialEN1992Data.B500B),
            ("B500C", () => SteelMaterialEN1992Data.B500C)
        };
        public RebarMaterialComponent() : base("Database Rebar Steel", "RS", "Acciaio per barre ordinarie del database GPC.") { }
        public override Guid ComponentGuid => new("606ebfd0-d9e7-407e-aaa3-eb8579fe2f6b");
        protected override (string Name, Func<SteelMaterialEN1992> Create)[] Grades => Items;
        protected override int DefaultGrade => 1;
        protected override (string Name, string Nick) Output => ("Rebar Steel Material", "RS");
    }

    public sealed class BarMaterialComponent : SteelDatabaseComponent
    {
        private static readonly (string, Func<SteelMaterialEN1992>)[] Items =
        {
            ("Y1180C", () => SteelMaterialEN1992Data.Y1180C), ("Y1240C", () => SteelMaterialEN1992Data.Y1240C),
            ("Y1280C", () => SteelMaterialEN1992Data.Y1280C), ("Y1370C", () => SteelMaterialEN1992Data.Y1370C),
            ("Y1420C", () => SteelMaterialEN1992Data.Y1420C), ("Y1470C", () => SteelMaterialEN1992Data.Y1470C)
        };
        public BarMaterialComponent() : base("Database Bar Steel", "DBS", "Acciaio per barre da precompressione del database GPC.") { }
        public override Guid ComponentGuid => new("23875d38-d43a-4051-b19c-a6ae39139f7b");
        protected override (string Name, Func<SteelMaterialEN1992> Create)[] Grades => Items;
        protected override int DefaultGrade => 0;
        protected override (string Name, string Nick) Output => ("Bar Steel Material", "BS");
    }

    public sealed class TendonMaterialComponent : SteelDatabaseComponent
    {
        private static readonly (string, Func<SteelMaterialEN1992>)[] Items =
        {
            ("Y1570C", () => SteelMaterialEN1992Data.Y1570C), ("Y1620C", () => SteelMaterialEN1992Data.Y1620C),
            ("Y1670C", () => SteelMaterialEN1992Data.Y1670C), ("Y1770C", () => SteelMaterialEN1992Data.Y1770C),
            ("Y1860C", () => SteelMaterialEN1992Data.Y1860C), ("Y1960C", () => SteelMaterialEN1992Data.Y1960C),
            ("Y2060C", () => SteelMaterialEN1992Data.Y2060C), ("Y2160C", () => SteelMaterialEN1992Data.Y2160C),
            ("Y2260C", () => SteelMaterialEN1992Data.Y2260C), ("Y2360C", () => SteelMaterialEN1992Data.Y2360C)
        };
        public TendonMaterialComponent() : base("Database Tendon Steel", "DTS", "Acciaio per trefoli del database GPC.") { }
        public override Guid ComponentGuid => new("c709c2fa-5e63-4cf6-b3a2-bd505a149a4d");
        protected override (string Name, Func<SteelMaterialEN1992> Create)[] Grades => Items;
        protected override int DefaultGrade => 4;
        protected override (string Name, string Nick) Output => ("Tendon Steel Material", "TS");
    }
}
