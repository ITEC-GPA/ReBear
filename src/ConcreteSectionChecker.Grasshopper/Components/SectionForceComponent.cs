using System;
using ConcreteSectionChecker.Adapters;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Geometry;
using GPC.Model.Results;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class SectionForceComponent : ReBearComponent
    {
        public SectionForceComponent() : base("Force component", "FC",
            "Combinazione di azioni nel riferimento assegnato; Units 1 = kN/kNm (default), 0 = N/Nmm. Compressione negativa.") { }
        public override Guid ComponentGuid => new("ddd3c603-f931-4db5-ab11-6d1da9ffd01e");
        protected override Port[] Inputs => new[] { T("Name", "N", "Nome univoco della combinazione; vuoto = \"Caso k\" (k = iterazione).", ""),
            N("Axial Force", "N", "Sforzo normale, compressione negativa.", 0), N("Shear X", "VX", "Taglio lungo X.", 0),
            N("Shear Y", "VY", "Taglio lungo Y.", 0), N("Torsion", "T", "Momento torcente.", 0),
            N("Bending X", "MX", "Momento attorno a X.", 0), N("Bending Y", "MY", "Momento attorno a Y.", 0),
            G("Coordinate System", "CS", "Riferimento delle azioni."),
            I("Units", "U", "0 = N, Nmm; 1 = kN, kNm.", 1, Named("Nmm", "kNm")) };
        protected override Port[] Outputs => new[] { G("Force", "F", "Azioni GPC in N e Nmm.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var axes = Required<CoordinateSystem>(d, 7);
            double force = Choice(d, 8, 2) == 1 ? 1000 : 1, moment = force == 1 ? 1 : 1e6;
            var v = new double[6];
            for (int i = 0; i < 6; i++) v[i] = Finite(d, i + 1);
            string name = Read(d, 0, "");
            if (string.IsNullOrWhiteSpace(name))
            {
                name = LoadEnvelopes.Name(null, d.Iteration);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Combinazione senza nome: assegnato \"{name}\". Usare nomi univoci per gli inviluppi.");
            }
            d.SetData(0, new GH_ResultBeamForces(new ResultBeamForces(v[0] * force, v[1] * force, v[2] * force, v[3] * moment,
                v[4] * moment, v[5] * moment, axes, -1, name)));
        }
    }
}
