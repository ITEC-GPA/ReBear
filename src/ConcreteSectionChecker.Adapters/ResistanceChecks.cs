using System;
using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Results;

namespace ConcreteSectionChecker.Adapters
{
    public static class ResistanceChecks
    {
        public static SectionResistanceResponse Evaluate(SectionCheckerModelCode2010 checker, ResultBeamForces forces)
        {
            var result = new LegacySectionCalculation(checker).SolveResistance(new SectionAnalysisInput(forces));
            if (result.Diagnostics.Status != CalculationStatus.Completed)
                throw new InvalidOperationException("Resistenza non convergente: " + result.Diagnostics.Message);
            return result;
        }

        public static SectionResistanceResponse RequirePoint(SectionCheckerModelCode2010 checker, ResultBeamForces forces)
        {
            var result = Evaluate(checker, forces);
            if (result.IsAxialForceOutsideDomain || !result.N.HasValue || !result.M1.HasValue || !result.M2.HasValue)
                throw new InvalidOperationException("Punto resistente non disponibile: " + result.Diagnostics.Message);
            return result;
        }
    }
}
