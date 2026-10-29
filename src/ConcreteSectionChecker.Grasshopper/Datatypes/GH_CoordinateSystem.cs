using ConcreteSectionChecker.Grasshopper;
using GPC.Geometry;
using Grasshopper.Kernel.Types;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_CoordinateSystem : ReBearGoo<CoordinateSystem>
    {
        public GH_CoordinateSystem(CoordinateSystem model) : base(model) { }
        public GH_CoordinateSystem() { }
        public override string TypeName => "ReBear Coordinate System";
        public override string TypeDescription => "Riferimento delle azioni: origine e assi, mm.";
        public override IGH_Goo Duplicate() => new GH_CoordinateSystem(Value is null ? null : new CoordinateSystem(Value));
        protected override string Describe(CoordinateSystem value) => Text(value);
        internal static string Text(CoordinateSystem a) => a is null ? "—" :
            $"'{a.Name}' O=({P(a.Origin.X)}, {P(a.Origin.Y)}, {P(a.Origin.Z)}) X=({P(a.V1.X)}, {P(a.V1.Y)}, {P(a.V1.Z)}) Y=({P(a.V2.X)}, {P(a.V2.Y)}, {P(a.V2.Z)})";
        private static string P(double v) => Persistence.Number(v);
    }
}
