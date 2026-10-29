using System;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Standards;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class StandardComponent : ReBearComponent
    {
        public StandardComponent() : base("Standard", "S", "Profilo normativo GPC: 0 NTC2018, 1 EN 1992-1-1, 2 UNI EN 1992-1-1.") { }
        public override Guid ComponentGuid => new("3def8fa3-e056-4075-8661-bd51575c00ee");
        protected override Port[] Inputs => new[] { I("Type", "T", "Norma: 0 NTC2018, 1 EN 1992-1-1, 2 UNI EN 1992-1-1.", 0,
            Named("NTC2018", "EN 1992-1-1", "UNI EN 1992-1-1")) };
        protected override Port[] Outputs => new[] { G("Standard", "S", "Norma GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            StandardModelCode2010 standard = Choice(d, 0, 3) switch
            {
                0 => new StandardNTC2018Concrete(), 1 => new StandardEN1992p11(), _ => new StandardUNIEN1992p11()
            };
            d.SetData(0, new GH_Standard(standard));
        }
    }
}
