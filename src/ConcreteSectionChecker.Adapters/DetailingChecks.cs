using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Cracking;
using GPC.Checkers.Concrete.Detailing;
using GPC.Checkers.Concrete.Durability;
using GPC.Model.Materials;
using GPC.Model.Standards;
using GPC.Model.Sections.Concrete;

namespace ConcreteSectionChecker.Adapters
{
    public sealed record DetailingOptions(MemberDetailingKind Kind, double TopWidth, double BottomWidth, double WebWidth,
        double LinkDiameter, double LinkSpacing, int LinkLegs, double Cover, double Aggregate, double Deviation,
        double? DurabilityCover, bool LapZone, bool Restrained, bool EndZones, PlateDetailingData Plate = null);

    public static class DetailingChecks
    {
        /// <summary>
        /// GPC cover assessment: the requirement of the specification (bar diameter of <paramref name="specification"/>, e.g. the
        /// material sheet) and, when known, the requirement for the actual bars (<paramref name="actualBarDiameter"/>).
        /// Without the actual bars the check stays NotEvaluated unless the adopted cover is already insufficient.
        /// Native is the requirement for the actual bars when given, otherwise that of the specification.
        /// </summary>
        public static (CheckResult Result, CoverResult Native, CoverAssessment Assessment) Cover(Standard standard, CoverInput specification,
            double adopted, double? actualBarDiameter, string section)
        {
            if (standard is null || specification is null) throw new ArgumentException("Norma e dati di durabilità richiesti.");
            var profile = DurabilityProfiles.Resolve(standard);
            var material = CoverRequirements.Calculate(profile, specification);
            var actual = actualBarDiameter.HasValue ? CoverRequirements.Calculate(profile, specification.WithBarDiameter(actualBarDiameter.Value)) : null;
            var assessment = CoverAssessment.Evaluate(adopted, material.Nominal, actual?.Nominal);
            var r = actual ?? material;
            string message = actual is null
                ? "Diametro effettivo delle barre non assegnato: requisito delle barre reali da completare."
                : "Domanda = copriferro nominale richiesto (specifica e barre reali); capacità = copriferro disponibile.";
            return (new(section, "Durability", "Concrete cover", CheckResults.Status(assessment.Passed), null,
                assessment.Required, adopted, "mm", message, r.Reference, FormattableString.Invariant(
                    $"cnom,specifica={material.Nominal} mm (Ø={specification.BarDiameter}); cnom,barre={actual?.Nominal} mm (Ø={actualBarDiameter}); cmin,b={r.Bond}; cmin,dur={r.Durability}; cmin={r.Minimum} mm")), r, assessment);
        }

        public static (CheckResult Result, AnchorageResult Native) Anchorage(StandardModelCode2010 standard,
            ConcreteMaterialEuropeanCommon concrete, SteelMaterial steel, double diameter, double stress,
            double length, bool goodBond, bool lap, double lapPercent, double lapClear, string section)
        {
            if (standard is null || concrete is null || steel is null) throw new ArgumentException("Norma e materiali richiesti.");
            if (!double.IsFinite(stress) || stress < 0 || stress > Math.Abs(steel.CalculateFyd(standard)))
                throw new ArgumentException("Tensione di ancoraggio: 0 ≤ σsd ≤ fyd.");
            if (steel.SteelType == SteelMaterial.SteelTypes.Tendon) throw new NotSupportedException("Ancoraggio di barre ordinarie ad aderenza migliorata richiesto.");
            var r = AnchorageCalculator.Calculate(DetailingProfiles.Resolve(standard),
                new AnchorageInput(diameter, stress, AnchorageCalculator.BondFctk05(Math.Abs(concrete.Fck)),
                    standard.GammaC, goodBond, length, lap, lapPercent, lapClear, true, standard.AlphaCT));
            return (new(section, "Detailing", lap ? "Lap" : "Anchorage", CheckResults.Status(r.Passed), null,
                r.RequiredLength, r.AvailableLength, "mm", r.Expression, r.Reference,
                FormattableString.Invariant($"fbd={r.Fbd} MPa; lb,rqd={r.BasicLength} mm; lmin={r.MinimumLength} mm; lap clearance passed={r.LapClearDistancePassed}")), r);
        }

        public static IReadOnlyList<CheckResult> Member(SectionCheckerModelCode2010 checker, DetailingOptions p, double minimumN, string sectionId)
        {
            AdvancedShearChecks.Ordinary(checker);
            if (p is null || !Enum.IsDefined(typeof(MemberDetailingKind), p.Kind) || !double.IsFinite(minimumN))
                throw new ArgumentException("Opzioni dei dettagli o sforzo normale non validi.");
            var section = checker.SectionCheckerAttribute.Section as ReinforcedConcreteSection
                ?? throw new NotSupportedException("Sezione in calcestruzzo armato richiesta.");
            var c = section.ConcreteMaterial as ConcreteMaterialEuropeanCommon ?? throw new NotSupportedException("Calcestruzzo europeo richiesto.");
            var bars = section.Rebars.ToArray();
            if (bars.Length == 0) throw new ArgumentException("Sezione priva di barre.");
            var steel = bars[0].RebarMaterial; var standard = checker.StandardModelCode2010;
            if (bars.Any(b => b.RebarMaterial.Fyk != steel.Fyk || b.RebarMaterial.E != steel.E))
                throw new NotSupportedException("Dettagli: questo componente richiede un unico acciaio per le armature.");
            var input = new MemberDetailingInput(p.Kind, CrackSectionGeometry.From(section), section.Area, Math.Abs(c.Fck), c.Fctm,
                Math.Abs(steel.Fyk), Math.Abs(steel.CalculateFyd(standard)), p.TopWidth, p.BottomWidth, p.WebWidth,
                Math.Max(0, -minimumN), p.LinkDiameter > 0, p.LinkDiameter, p.LinkSpacing, p.LinkLegs, p.Aggregate,
                p.Cover, p.DurabilityCover, p.Deviation, p.LapZone, p.Restrained, p.EndZones, 0, 0,
                new MemberDetailingOptions(p.Plate, false));
            var r = MemberDetailingCalculator.Calculate(DetailingProfiles.Resolve(standard), input);
            return r.Checks.Select(line => new CheckResult(sectionId, "Detailing", "Detailing / " + line.Key,
                CheckResults.Status(line.Passed), null, line.Actual, line.Limit, line.Unit, line.Explanation, line.Reference,
                "Actual e Limit mantengono il verso di confronto della regola GPC; consultare Message.")).ToArray();
        }
    }
}
