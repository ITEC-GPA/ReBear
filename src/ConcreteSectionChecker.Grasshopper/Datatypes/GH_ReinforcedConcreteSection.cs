using System;
using System.Linq;
using ConcreteSectionChecker.Grasshopper;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_ReinforcedConcreteSection : ReBearGoo<ReinforcedConcreteSection>
    {
        public GH_ReinforcedConcreteSection(ReinforcedConcreteSection model) : base(model) { }
        public GH_ReinforcedConcreteSection() { }
        public override string TypeName => "ReBear RC Section";
        public override string TypeDescription => "Sezione GPC in calcestruzzo armato, mm.";
        public override IGH_Goo Duplicate() => new GH_ReinforcedConcreteSection(Value); // sections are never modified after construction
        protected override string Describe(ReinforcedConcreteSection s) => Text(s);
        internal static string Text(ReinforcedConcreteSection s) =>
            $"Sezione '{s.Name}' {s.ConcreteMaterial?.Name}: Ac={Persistence.Number(s.Area)} mm², {s.Rebars.Count()} barre, As={Persistence.Number(s.AreaRebars)} mm², " +
            $"G=({Persistence.Number(s.Centroid.X)}, {Persistence.Number(s.Centroid.Y)}) mm, Jxx={Persistence.Number(s.Jxx)} mm⁴, Jyy={Persistence.Number(s.Jyy)} mm⁴, " +
            $"θ1={Persistence.Number(s.AngleX1 * 180 / Math.PI)}°";
    }
}
