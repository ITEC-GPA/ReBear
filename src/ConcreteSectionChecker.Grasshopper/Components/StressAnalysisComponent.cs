using System;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Results;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class StressAnalysisComponent : ReBearComponent
    {
        public StressAnalysisComponent() : base("Stress Analysis", "SA",
            "Analisi tensionale GPC (lineare o non lineare secondo il checker) di ogni combinazione della lista.") { }
        public override Guid ComponentGuid => new("2c7939e3-5f53-4d53-989b-a56b65b773e4");
        protected override Port[] Inputs => new[] { G("Concrete Checker", "CC", "Checker GPC."), List(G("Forces", "F", "Combinazioni.")) };
        protected override Port[] Outputs => new[] { List(G("Stress Analysis Results", "SAR", "Analisi tensionali, una per combinazione.")),
            List(G("Strain Plane Results", "SPR", "Estremi tensionali e deformativi, uno per combinazione.")) };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var checker = Required<SectionCheckerModelCode2010>(d, 0);
            var forces = ReadList<ResultBeamForces>(d, 1);
            // Identify each case by its list position; unnamed cases become "Caso k" for the downstream reports.
            var cases = forces.Select((f, i) => new ResultBeamForces(f.N, f.V1, f.V2, f.T, f.M1, f.M2, f.CoordinateSystem, i, LoadEnvelopes.Name(f, i))).ToArray();
            var results = SectionAnalyses.Stress(checker, cases);
            var planes = results.Select((r, i) =>
            {
                var response = LegacySectionCalculation.Capture(r);
                if (response.Diagnostics.Status != CalculationStatus.Completed)
                    throw new InvalidOperationException($"Analisi '{cases[i].Name}': {response.Diagnostics.Message}");
                return r.LinearElasticAnalysis ? r.CalculateStrainPlaneResult(true, r.PsiRebar ?? 0, r.PsiTendon ?? 0) : r.CalculateStrainPlaneResult();
            }).ToArray();
            d.SetDataList(0, results.Select(r => new GH_StressAnalysisResult(r)));
            d.SetDataList(1, planes.Select(r => new GH_StrainPlaneResult(r)));
        }
    }
}
