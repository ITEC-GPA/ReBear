using System;
using System.Threading;
using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;

namespace ConcreteSectionChecker.Adapters
{
    /// <summary>Plastic and elastic resisting moments about the global axes through the centroid; kNm, signed.</summary>
    public sealed record ResistanceSummary(double MxPlusPlastic, double MyPlusPlastic, double MxMinusPlastic, double MyMinusPlastic,
        double MxPlusElastic, double MyPlusElastic, double MxMinusElastic, double MyMinusElastic)
    {
        public double[] Values => new[] { MxPlusPlastic, MyPlusPlastic, MxMinusPlastic, MyMinusPlastic,
            MxPlusElastic, MyPlusElastic, MxMinusElastic, MyMinusElastic };
    }

    public static class ResistanceChecks
    {
        public static SectionResistanceResponse Evaluate(SectionCheckerModelCode2010 checker, ResultBeamForces forces, CancellationToken token = default)
        {
            var result = new LegacySectionCalculation(checker).SolveResistance(new SectionAnalysisInput(forces), token);
            if (result.Diagnostics.Status != CalculationStatus.Completed)
                throw new InvalidOperationException("Resistenza non convergente: " + result.Diagnostics.Message);
            return result;
        }

        public static SectionResistanceResponse RequirePoint(SectionCheckerModelCode2010 checker, ResultBeamForces forces, CancellationToken token = default)
        {
            var result = Evaluate(checker, forces, token);
            if (result.IsAxialForceOutsideDomain || !result.N.HasValue || !result.M1.HasValue || !result.M2.HasValue)
                throw new InvalidOperationException("Punto resistente non disponibile: " + result.Diagnostics.Message);
            return result;
        }

        /// <summary>
        /// Resisting moments at constant N (kN, compression negative) about the global X/Y directions through the centroid of
        /// the concrete. The 0.2 summary used the solver axes (−X, −Y), which exchanged MxRd+ with MxRd− (and MyRd+ with MyRd−).
        /// Iterative domain-point strategy, no rounding.
        /// </summary>
        public static ResistanceSummary Summary(ReinforcedConcreteSection section, StandardModelCode2010 standard, double axialForce)
        {
            if (section is null || standard is null) throw new ArgumentException("Sezione e norma richieste.");
            if (!double.IsFinite(axialForce)) throw new ArgumentException("Sforzo normale finito richiesto.");
            var axes = new CoordinateSystem(new Point3d(section.Centroid.X, section.Centroid.Y, 0), Vector3d.XAxis, Vector3d.YAxis, "Centroid");
            SectionCheckerModelCode2010 Checker(SectionSolver.FailureDomainTypes domain) => new(new SectionCheckerAttribute(section),
                new SectionCheckerModelCode2010.SectionOptionsModelCode2010
                {
                    FailureDomainType = domain, FailureAnalysisType = SectionSolver.FailureAnalysisTypes.ConstantN,
                    DomainPointStrategy = SectionSolver.DomainPointStrategyTypes.Iterative, ForceReferenceCoordinateSystem = axes
                }, standard);
            double[] Moments(SectionCheckerModelCode2010 checker)
            {
                double n = axialForce * 1000;
                double Point(double mx, double my, bool x)
                {
                    var r = RequirePoint(checker, new ResultBeamForces(n, 0, 0, 0, mx, my, axes));
                    return (x ? r.M1.Value : r.M2.Value) / 1e6;
                }
                return new[] { Point(1e8, 0, true), Point(0, 1e8, false), Point(-1e8, 0, true), Point(0, -1e8, false) };
            }
            var plastic = Moments(Checker(SectionSolver.FailureDomainTypes.Plastic));
            var elastic = Moments(Checker(SectionSolver.FailureDomainTypes.Elastic));
            return new(plastic[0], plastic[1], plastic[2], plastic[3], elastic[0], elastic[1], elastic[2], elastic[3]);
        }
    }
}
