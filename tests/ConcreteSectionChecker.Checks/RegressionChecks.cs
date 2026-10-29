using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Detailing;
using GPC.Checkers.Concrete.Durability;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Torsion;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;
using static Program;

/// <summary>Regression tests of the defects and findings fixed in 0.3 (B1–B5, C2–C8, performance).</summary>
internal static class RegressionChecks
{
    internal static void RunAll(Action<string, Action> run)
    {
        run("B1: estremi assenti (±MaxValue, ±∞ dopo ψ) restituiti come mancanti, mai 0 o ∞", AbsentExtremes);
        run("B2: CSV invariant con escaping anche in it-IT, un solo scrittore", InvariantCsv);
        run("B3: torsione con assi del checker ruotati di 90° (rumore di trasformazione)", RotatedTorsion);
        run("B4: Section Resistance Summary negli assi globali (MxRd+ = 508.5 kNm per la T)", GlobalSummary);
        run("B5: piano senza curvatura: nessun asse neutro, mesh non tagliata", NoCurvature);
        run("C2: conversione unica della mesh GPC e colori delle tensioni", Meshes);
        run("C3: copie indipendenti delle opzioni", OptionCopies);
        run("C4: persistenza versionata di sezione, checker, azioni e opzioni", Codec);
        run("C7: copriferro con specifica e barre reali, modificatori EN/NTC distinti", Cover);
        run("C8: combinazioni senza nome con nomi univoci", CaseNames);
        run("Prestazioni: checker Iterative, analisi ausiliaria non fessurata condivisa nel batch", Caching);
        run("Ricerca in parallelo uguale alla seriale; annullamento", ParallelSearch);
        run("Verifica del set GPC caricato rispetto a release-set.json", ReleaseSet);
    }

    private static void AbsentExtremes()
    {
        foreach (bool linear in new[] { true, false })
        {
            var c = Checker();
            if (!linear) c.SectionCheckerOptions.StressAnalysisType = SectionSolver.StressAnalysisTypes.NonLinear;
            var stress = c.GetTensionAnalysisResult(Force(-300e3, 0, 0, 60e6, 0));
            var plane = linear ? stress.CalculateStrainPlaneResult(true, 1.5, 0) : stress.CalculateStrainPlaneResult();
            var s = StressSummary.From(plane);
            Check(s.SigmaPMin == null && s.SigmaPMax == null && s.EpsilonPMin == null && s.EpsilonPMax == null, "No tendons: σp/εp absent");
            Check(s.SigmaSSMin == null && s.SigmaSSMax == null && s.EpsilonSSMin == null && s.EpsilonSSMax == null, "No steel sections: σss/εss absent");
            Check(s.SigmaCMin < 0 && s.SigmaSMax.HasValue && s.EpsilonCMin < 0 && double.IsFinite(s.EpsilonCMin.Value), "Existing extremes finite");
            Close(-300, s.N.Value, "N [kN]"); Close(60, s.Mx.Value, "Mx [kNm]");
            Check(s.Values.All(v => v == null || double.IsFinite(v.Value)), "No infinite value");
        }
        Check(Report.Finite(double.MaxValue) == null && Report.Finite(double.MinValue) == null && Report.Finite(double.PositiveInfinity) == null
            && Report.Finite(-2.5 * double.MaxValue) == null && Report.Finite(1.5) == 1.5, "Sentinel helper");
    }

    private static void InvariantCsv()
    {
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            var c = Checker();
            var s = StressSummary.From(c.GetTensionAnalysisResult(Force(-300e3, 0, 0, 60e6, 0)).CalculateStrainPlaneResult(true, 1.5, 0)) with { Case = "A;\"B" };
            string csv = StressSummary.Csv(new[] { s });
            var lines = csv.Split(Environment.NewLine);
            Check(lines.Length == 2 && lines[0].Split(';').Length == StressSummary.Header.Length, "Header with ';'");
            Check(lines[1].Contains("\"A;\"\"B\"") && lines[1].Contains("\"-300\"") && !lines[1].Contains(",5\""), "Invariant, escaped");
            Check(lines[1].Contains("\"\";") || lines[1].EndsWith("\"\""), "Absent value is an empty field");
            Check(Report.Field(double.NaN) == "\"\"" && Report.Field(0.25) == "\"0.25\"", "Field format");
            using var json = System.Text.Json.JsonDocument.Parse(StressSummary.Json(new[] { s }));
            Check(json.RootElement.GetProperty("schema").GetString() == "ReBear.StressReport"
                && json.RootElement.GetProperty("results")[0].GetProperty("SigmaPMin").ValueKind == System.Text.Json.JsonValueKind.Null, "JSON null");
        }
        finally { CultureInfo.CurrentCulture = culture; }
    }

    private static void RotatedTorsion()
    {
        // Checker axes X' = global Y, Y' = −global X: a global Vy becomes V1' with round-off on V2'.
        double cos = Math.Cos(Math.PI / 2), sin = Math.Sin(Math.PI / 2); // 6.1e-17 and 1, as from a rotation by 90°
        var axes = new CoordinateSystem(Point3d.Origin, new Vector3d(cos, sin, 0), new Vector3d(-sin, cos, 0));
        var c = Checker(axes: axes);
        var shear = FeatureChecks.ShearOptions(500, 252);
        var torsion = new TorsionOptions(new TorsionGeometry(200 * 400, 1200, 100), shear.LinkSteel, shear.LongitudinalSteel,
            Math.PI * 16, 150, 800, 1.5);
        var global = new ResultBeamForces(-300e3, 0, 30e3, 5e6, 0, 0, CoordinateSystem.Global);
        var transported = global.ToCoordinateSystemWithEccentricity(axes);
        Check(transported.V2 != 0 && Math.Abs(transported.V2) < 1e-6, "The rotation leaves round-off on V2' (precondition)");
        var rotated = AdvancedShearChecks.Torsion(c, global, torsion, shear, null, "S");
        var direct = AdvancedShearChecks.Torsion(c, new ResultBeamForces(-300e3, 30e3, 0, 5e6, 0, 0, axes), torsion, shear, null, "S");
        Close(direct.Ratio.Value, rotated.Ratio.Value, "Same torsion utilization", 1e-9);
        Check(!AdvancedShearChecks.IsShearPresent(5.5e-12, 3e4) && AdvancedShearChecks.IsShearPresent(1, 3e4)
            && !AdvancedShearChecks.IsShearPresent(5e-7, 0), "Relative tolerance");
        Throws<ArgumentException>(() => AdvancedShearChecks.Torsion(c, new ResultBeamForces(-300e3, 30e3, 30e3, 5e6, 0, 0, axes), torsion, shear, null, "S"));
    }

    internal static ReinforcedConcreteSection Tee() =>
        SectionBuilders.Tee(ConcreteMaterialEN1992Data.C30_37, SteelMaterialEN1992Data.B450C, 800, 700, 300, 180, 30, 8, 20, 6, 3, 2);

    private static void GlobalSummary()
    {
        var section = Tee(); var standard = new StandardNTC2018Concrete();
        var s = ResistanceChecks.Summary(section, standard, 0);
        // 29/10 probe (Intersection): 0.2 reported MxRd+ = 430.62 kNm, the global +Mx resistance is 508.48 kNm.
        Close(508.48, s.MxPlusPlastic, "MxRd+ pl [kNm]", 2e-3);
        Close(-430.62, s.MxMinusPlastic, "MxRd- pl [kNm]", 2e-3);
        Check(s.MyPlusPlastic > 0 && s.MyMinusPlastic < 0 && Math.Abs(s.MyPlusPlastic + s.MyMinusPlastic) < 1e-3 * s.MyPlusPlastic, "Symmetric My");
        Check(s.MxPlusElastic > 0 && s.MxPlusElastic < s.MxPlusPlastic && s.MxMinusElastic < 0, "Elastic within plastic");
        // Independent: a +Mx demand about the global axes through the centroid compresses the top flange.
        var axes = new CoordinateSystem(new Point3d(section.Centroid.X, section.Centroid.Y, 0), Vector3d.XAxis, Vector3d.YAxis);
        var c = new SectionCheckerModelCode2010(new GPC.Checkers.Concrete.Attributes.SectionCheckerAttribute(section),
            new SectionCheckerModelCode2010.SectionOptionsModelCode2010 { ForceReferenceCoordinateSystem = axes, FailureAnalysisType = SectionSolver.FailureAnalysisTypes.ConstantN }, standard);
        var point = ResistanceChecks.RequirePoint(c, new ResultBeamForces(0, 0, 0, 0, 1e8, 0, axes));
        Close(point.M1.Value / 1e6, s.MxPlusPlastic, "Same as a global +Mx point", 1e-9);
        Check(s.Values.Any(v => Math.Abs(v * 100 - Math.Round(v * 100)) > 1e-6), "No rounding to 2 decimals");
    }

    private static void NoCurvature()
    {
        var c = Checker();
        foreach (var force in new[] { Force(0, 0, 0, 0, 0), Force(-300e3, 0, 0, 0, 0) })
        {
            var stress = c.GetTensionAnalysisResult(force);
            Check(SectionMeshes.NeutralAxis(stress.StrainPlane, (ReinforcedConcreteSection)stress.ConcreteSection) == null, "No neutral axis");
            var mesh = SectionMeshes.StressMesh(stress, false, out var axis);
            Check(!axis.HasValue && mesh.Faces.Count > 0, "Uncut mesh");
            var coloured = SectionMeshes.StressColours(stress, true, out axis);
            Check(!axis.HasValue && coloured.Colours.Count == coloured.Vertices.Count, "Coloured mesh without axis");
        }
        var bent = c.GetTensionAnalysisResult(Force(-100e3, 0, 0, 80e6, 0));
        var line = SectionMeshes.NeutralAxis(bent.StrainPlane, (ReinforcedConcreteSection)bent.ConcreteSection);
        Check(line.HasValue && Math.Abs(bent.StrainPlane.GetStrain(line.Value.Start)) < 1e-12 && Math.Abs(bent.StrainPlane.GetStrain(line.Value.End)) < 1e-12, "Axis on zero strain");
        Check(line.Value.Start.DistanceTo(line.Value.End) > 500, "Axis spans the section");
    }

    private static void Meshes()
    {
        var c = Checker(); var section = (ReinforcedConcreteSection)c.SectionCheckerAttribute.Section;
        var gpc = section.Mesh; var mesh = SectionMeshes.From(gpc);
        Check(mesh.Faces.Count == gpc.GetFaces().Length && mesh.Vertices.Count <= gpc.GetVertices().Length, "Faces preserved");
        Check(mesh.Faces.SelectMany(f => f).Distinct().Count() == mesh.Vertices.Count, "No unreferenced vertex");
        Close(150000, mesh.Faces.Sum(f => Area(mesh, f)), "Mesh area [mm²]", 1e-9);
        var stress = c.GetTensionAnalysisResult(Force(-100e3, 0, 0, 80e6, 0));
        var cut = SectionMeshes.StressMesh(stress, false, out var axis);
        Check(axis.HasValue && cut.Faces.Count > mesh.Faces.Count, "Cut along the axis");
        Close(150000, cut.Faces.Sum(f => Area(cut, f)), "Cut mesh area [mm²]", 1e-9);
        var fcd = section.ConcreteMaterial.CalculateDesignCompressiveStrength(stress.Standard);
        var fctd = section.ConcreteMaterial.CalculateDesignTensileStrength(stress.Standard);
        Check(SectionMeshes.GradientColour(0, fcd, fctd) == Rgb.White && SectionMeshes.GradientColour(Math.Abs(fctd), fcd, fctd) == Rgb.Blue
            && SectionMeshes.GradientColour(-Math.Abs(fcd), fcd, fctd) == Rgb.Magenta && SectionMeshes.GradientColour(-2 * Math.Abs(fcd), fcd, fctd) == Rgb.Black,
            "Gradient end points");
        Check(SectionMeshes.SignColour(-1) == Rgb.Red && SectionMeshes.SignColour(1) == Rgb.Blue && SectionMeshes.SignColour(0) == Rgb.White, "Sign colours");
        var sign = SectionMeshes.StressColours(stress, false, out _);
        Check(sign.Colours.Contains(Rgb.Red) && sign.Colours.Contains(Rgb.White) && !sign.Colours.Contains(Rgb.Black), "Compressed and cracked faces");
        double z = SectionMeshes.ConcreteStress(stress, 0, 250), y = SectionMeshes.ConcreteStress(stress, 0, -250);
        // GPC convention: a positive Mx compresses the fibres with negative y.
        Check(y < 0 && z == 0, $"Bottom compressed, cracked top: σ(top)={z}, σ(bottom)={y}");
        var nearest = SectionMeshes.NearestColours(mesh, new[] { (0d, 1000d, 0d), (0d, -1000d, 0d) }, new[] { Rgb.Red, Rgb.Blue }, false);
        Check(nearest.Colours.Count == nearest.Vertices.Count && nearest.Colours.Contains(Rgb.Red) && nearest.Colours.Contains(Rgb.Blue), "Nearest colours");
        Throws<ArgumentException>(() => SectionMeshes.NearestColours(mesh, new[] { (0d, 0d, 0d) }, new[] { Rgb.Red, Rgb.Blue }, true));
    }
    private static double Area(MeshData m, int[] f)
    {
        double a = 0;
        for (int i = 0; i < f.Length; i++)
        {
            var p = m.Vertices[f[i]]; var q = m.Vertices[f[(i + 1) % f.Length]];
            a += p.X * q.Y - q.X * p.Y;
        }
        return Math.Abs(a) / 2;
    }

    private static void OptionCopies()
    {
        var c = Checker(); var source = c.SectionCheckerOptionsModelCode2010;
        var copy = SectionAnalyses.Copy(source);
        copy.PsiCoefficientRebar = 7; copy.ForceReferenceCoordinateSystem = new CoordinateSystem(new Point3d(1, 2, 3), Vector3d.XAxis, Vector3d.YAxis);
        Check(source.PsiCoefficientRebar == 0 && source.ForceReferenceCoordinateSystem.Origin.X == 0, "Source untouched");
        var same = SectionAnalyses.Prepared(c, SectionSolver.FailureAnalysisTypes.ConstantN, c.SectionCheckerOptions.TetaDiscretization);
        Check(ReferenceEquals(same, c), "No copy when the options already match");
        var other = SectionAnalyses.Prepared(c, SectionSolver.FailureAnalysisTypes.ConstantN, 16);
        Check(!ReferenceEquals(other, c) && other.SectionCheckerOptions.TetaDiscretization == 16 && c.SectionCheckerOptions.TetaDiscretization == 64, "Copy only to change options");
    }

    private static void Codec()
    {
        var template = Checker(new StandardEN1992p11(), axes: new CoordinateSystem(new Point3d(10, 20, 0), Vector3d.YAxis, new Vector3d(-1, 0, 0), "R"));
        var options = SectionAnalyses.Copy(template.SectionCheckerOptionsModelCode2010, angles: 48); options.PsiCoefficientRebar = 1.25;
        var c = new SectionCheckerModelCode2010(new GPC.Checkers.Concrete.Attributes.SectionCheckerAttribute(template.SectionCheckerAttribute.Section),
            options, template.StandardModelCode2010);
        var restored = (SectionCheckerModelCode2010)ModelCodec.Read(ModelCodec.Write(c));
        var a = (ReinforcedConcreteSection)c.SectionCheckerAttribute.Section; var b = (ReinforcedConcreteSection)restored.SectionCheckerAttribute.Section;
        Close(a.Area, b.Area, "Area"); Close(a.AreaRebars, b.AreaRebars, "As"); Close(a.Jxx, b.Jxx, "Jxx");
        Check(b.Rebars.Count() == 4 && b.Rebars.First().RebarMaterial.Name == "B450C" && b.ConcreteMaterial.Name == "C30/37", "Bars and materials");
        Check(restored.StandardModelCode2010 is StandardEN1992p11, "Standard");
        var o = restored.SectionCheckerOptions;
        Check(o.TetaDiscretization == 48 && o.PsiCoefficientRebar == 1.25 && o.DomainPointStrategy == SectionSolver.DomainPointStrategyTypes.Iterative
            && o.ForceReferenceCoordinateSystem.Origin.X == 10 && Math.Abs(o.ForceReferenceCoordinateSystem.V1.Y - 1) < 1e-12, "Options and axes");
        var f = Force(); f = new ResultBeamForces(f.N, f.V1, f.V2, 3e6, f.M1, f.M2, f.CoordinateSystem, 4, "SLU 1");
        Close(ResistanceChecks.Evaluate(c, f).Utilization.Value, ResistanceChecks.Evaluate(restored, f).Utilization.Value, "Same engine result");
        var g = (ResultBeamForces)ModelCodec.Read(ModelCodec.Write(f));
        Check(g.Name == "SLU 1" && g.Id == 4 && g.T == 3e6 && g.M2 == f.M2, "Forces");
        var hardening = SteelMaterialEN1992Data.B500C; hardening.StressStrainCurve = GPC.Model.Materials.SteelMaterial.StressStrainCurveType.ElasticHardening;
        var steel = (GPC.Model.Materials.SteelMaterialEN1992)ModelCodec.Read(ModelCodec.Write(hardening));
        Check(steel.StressStrainCurve == hardening.StressStrainCurve && steel.Name == "B500C", "Steel law");
        foreach (object value in new object[] { FeatureChecks.ShearOptions(), new SleOptions(1, "XD1", true, false, true, 38, .3, null, 40, true),
            new DetailingOptions(MemberDetailingKind.Slab, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, true, false, true, new PlateDetailingData(500, 200, true)),
            new TorsionOptions(new TorsionGeometry(1, 2, 3), SteelMaterialEN1992Data.B450C, SteelMaterialEN1992Data.B500B, 4, 5, 6, 7, true) })
        {
            var back = ModelCodec.Read(ModelCodec.Write(value));
            Check(back.GetType() == value.GetType() && ModelCodec.Write(back) == ModelCodec.Write(value), value.GetType().Name + " round trip");
        }
        Check(!ModelCodec.CanWrite(CrackedState()), "Results are not persisted");
        Throws<System.IO.InvalidDataException>(() => ModelCodec.Read(ModelCodec.Write(c).Replace("\"version\":1", "\"version\":2")));
        Throws<System.IO.InvalidDataException>(() => ModelCodec.Read(ModelCodec.Write(SteelMaterialEN1992Data.B450C).Replace("\"fyk\":450", "\"fyk\":460")));
    }

    private static void Cover()
    {
        var en = new StandardEN1992p11();
        CoverInput Input(double bar = 20, bool slab = false, bool quality = false, bool plate = false, bool ntcQuality = false) =>
            new(new[] { "XC4" }, 30, 50, false, slab, quality, bar, 20, 10, false, 0, 0, plate, ntcQuality);
        var unknown = DetailingChecks.Cover(en, Input(), 60, null, "S");
        Check(unknown.Result.Status == CheckStatus.NotEvaluated && unknown.Assessment.Required == 40, "Actual bars unknown: pending, not satisfied");
        var insufficient = DetailingChecks.Cover(en, Input(), 35, null, "S");
        Check(insufficient.Result.Status == CheckStatus.NotSatisfied, "Known insufficiency still fails");
        var large = DetailingChecks.Cover(en, Input(), 40, 32, "S");
        Close(42, large.Assessment.Required, "Ø32 bond governs: max(32, 30) + 10 mm");
        Check(large.Result.Status == CheckStatus.NotSatisfied && large.Result.Demand == 42, "Actual bars requirement used");
        Check(DetailingChecks.Cover(en, Input(), 40, 20, "S").Result.Status == CheckStatus.Satisfied, "Both requirements met");
        // EN modifiers and NTC modifiers are independent ports now.
        Check(CoverRequirements.Calculate(DurabilityProfiles.Resolve(en), Input(slab: true)).Nominal < CoverRequirements.Calculate(DurabilityProfiles.Resolve(en), Input()).Nominal, "EN slab modifier");
        Close(CoverRequirements.Calculate(DurabilityProfiles.Resolve(en), Input()).Nominal,
            CoverRequirements.Calculate(DurabilityProfiles.Resolve(en), Input(plate: true, ntcQuality: true)).Nominal, "NTC flags do not change EN");
    }

    private static void CaseNames()
    {
        var unnamed = Force();
        Check(LoadEnvelopes.Name(unnamed, 0) == "Caso 1" && LoadEnvelopes.Name(unnamed, 4) == "Caso 5", "Position-based names");
        var a = SectionChecks.Resistance(Checker(), unnamed, "S", 0); var b = SectionChecks.Resistance(Checker(), unnamed, "S", 1);
        Check(a.Case != b.Case, "Distinct iterations give distinct names");
        var shear = AdvancedShearChecks.Shear(Checker(), unnamed, FeatureChecks.ShearOptions(), 1, "S", 2);
        Check(shear.Case == "Caso 3", "Shear case name");
    }

    private static void Caching()
    {
        var c = Checker();
        var stress = c.GetTensionAnalysisResult(Force(-300000, 0, 0, 0, 0));
        var cache = new UncrackedCheckers();
        var first = ServiceabilityChecks.Cracking(stress, 30, 0, 12, 0, 0, 1, cache: cache);
        var second = ServiceabilityChecks.Cracking(c.GetTensionAnalysisResult(Force(-200000, 0, 0, 0, 0)), 30, 0, 12, 0, 0, 1, cache: cache);
        Check(cache.Count == 1 && first.UncrackedMaximumStress < 0 && second.UncrackedMaximumStress < 0, "One auxiliary checker for the batch");
        Close(ServiceabilityChecks.Cracking(stress, 30, 0, 12, 0, 0, 1).UncrackedMaximumStress.Value, first.UncrackedMaximumStress.Value, "Same uncracked stress");
        Check(new SectionCheckerModelCode2010.SectionOptionsModelCode2010().DomainPointStrategy == SectionSolver.DomainPointStrategyTypes.Iterative, "GPC default");
        var analyses = SectionAnalyses.Stress(c, new[] { Force(-300e3, 0, 0, 30e6, 0), Force(-100e3, 0, 0, 80e6, 0) });
        Close(c.GetTensionAnalysisResult(Force(-100e3, 0, 0, 80e6, 0)).StrainPlane.Chi, analyses[1].StrainPlane.Chi, "Stress list without rebuilding the checker");
    }

    private static void ParallelSearch()
    {
        var sections = new[] { 16, 20, 24, 20 }.Select(d => SectionBuilders.Rectangle(ConcreteMaterialEN1992Data.C30_37, SteelMaterialEN1992Data.B450C,
            300, 500, 30, 8, d, 4, 4, 0)).ToArray();
        var candidates = sections.Select(s => new SectionCheckerModelCode2010(new GPC.Checkers.Concrete.Attributes.SectionCheckerAttribute(s),
            new SectionCheckerModelCode2010.SectionOptionsModelCode2010 { FailureAnalysisType = SectionSolver.FailureAnalysisTypes.ConstantN }, new StandardNTC2018Concrete())).ToList();
        candidates.Add(candidates[0]); // the same checker twice must stay on one thread
        var forces = new[] { Force(-300e3, 0, 0, 180e6, 5e6), Force(-200e3, 0, 0, 150e6, 10e6) };
        var parallel = SectionAnalyses.Search(candidates, forces, .9);
        var isolated = SectionAnalyses.Search(candidates, forces, .9, isolate: true);
        Check(parallel.Index == 1 && isolated.Index == 1 && parallel.Results.Count == 10, "Ø20 selected, results per candidate and case");
        for (int i = 0; i < candidates.Count; i++)
        {
            var serial = SectionChecks.Batch(SectionAnalyses.Detached(candidates[i]), forces, "Candidate " + i, true);
            var found = parallel.Results.Where(r => r.Section == "Candidate " + i).ToArray();
            for (int k = 0; k < serial.Count; k++) Close(serial[k].Ratio.Value, found[k].Ratio.Value, "Candidate " + i + " case " + k, 1e-12);
        }
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        Throws<OperationCanceledException>(() => SectionAnalyses.Search(candidates, forces, .9, cancelled.Token));
        Throws<OperationCanceledException>(() => SectionChecks.Batch(Checker(), forces, "S", true, token: cancelled.Token));
        Throws<OperationCanceledException>(() => SectionAnalyses.Slice(Checker(), -300e3, 16, cancelled.Token));
    }

    private static void ReleaseSet()
    {
        var manifest = new[] { new ReleaseSetCheck.Entry("GPCModel.dll", "5.21.0.1", "AA"), new ReleaseSetCheck.Entry("release-set.json", "", "") };
        string Hash(string path) => path == "same" ? "aa" : "BB";
        Check(ReleaseSetCheck.Compare(manifest, new[] { ("GPCModel", "5.21.0.1", "same") }, Hash).Count == 0, "Same set");
        Check(ReleaseSetCheck.Compare(manifest, new[] { ("GPCModel", "5.23.0.2", "same") }, Hash).Single().Reason.Contains("versione"), "Other version");
        Check(ReleaseSetCheck.Compare(manifest, new[] { ("GPCModel", "5.21.0.1", "other") }, p => "BB").Count == 1, "Other content");
        Check(ReleaseSetCheck.Compare(manifest, new[] { ("Other", "1.0.0.0", "x") }, Hash).Count == 0, "Assemblies outside the set ignored");
        // The real process: the test runner loads the GPC set of lib/GPC.
        var real = ReleaseSetCheck.CompareLoaded(System.IO.Path.Combine(RepositoryRoot, "lib/GPC/release-set.json"));
        Check(real.Count == 0, "Loaded GPC assemblies match the release set: " + string.Join("; ", real));
    }
}
