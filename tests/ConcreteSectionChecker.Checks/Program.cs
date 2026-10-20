using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.RegularExpressions;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Cracking;
using GPC.Checkers.Concrete.Results;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Serviceability;
using GPC.Checkers.Concrete.Shear;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Standards;

internal static class Program
{
    private static int passed, failed;
    private static readonly string Root = FindRoot();
    private static int Main(string[] args)
    {
        if (args.Contains("--write-catalog")) { GrasshopperSmoke.WriteCatalog(); return 0; }
        if (args.Contains("--only-grasshopper"))
        {
            Run("Caricamento assembly e 61 componenti Grasshopper", GrasshopperSmoke.Run);
            Run("Porte Grasshopper: unità, taglio X/Y, errori e SLE non applicabili", ComponentBoundary.Run);
            Console.WriteLine($"TOTAL: {passed} PASS, {failed} FAIL");
            return failed == 0 ? 0 : 1;
        }
        Run("32 GUID e registrazioni delle porte identici a 66555b", Compatibility);
        Run("Sei DLL GPC identiche al set di rilascio", LibraryHashes);
        Run("Taglio senza staffe: confronto analitico NTC", AnalyticalShear);
        foreach (var standard in new StandardModelCode2010[] { new StandardNTC2018Concrete(), new StandardEN1992p11(), new StandardUNIEN1992p11() })
        {
            var selected = standard;
            Run("Taglio X/Y, N/Nmm e materiali: " + standard.Name, () => RectangularMapping(selected));
        }
        Run("Taglio in trazione NTC: non valutato", () =>
        {
            var r = ShearChecks.WithoutStirrups(Checker(), Force(100000), 500, 300, 20, 4, 30, 150000);
            Check(r.Verdict == ShearVerdict.NotEvaluated && r.Ratio == null, "Axial tension must not become compression.");
        });
        Run("Taglio: area esplicita usata", () =>
        {
            var a = ShearChecks.WithoutStirrups(Checker(), Force(-300000), 500, 300, 20, 4, 30, 150000);
            var b = ShearChecks.WithoutStirrups(Checker(), Force(-300000), 500, 300, 20, 4, 30, 120000);
            Check(b.VRd > a.VRd, "Explicit gross area must affect axial stress.");
        });
        Run("Taglio circolare: braccio GPC e risultante V", Circular);
        Run("Taglio: ingressi non validi e modello circolare fuori campo", InvalidShear);
        Run("Combinazioni SLE: mappatura esplicita degli ID storici", () =>
        {
            Check(ServiceabilityChecks.Combination(0) == ServiceabilityCombination.QuasiPermanent, "0");
            Check(ServiceabilityChecks.Combination(1) == ServiceabilityCombination.Frequent, "1");
            Check(ServiceabilityChecks.Combination(2) == ServiceabilityCombination.Characteristic, "2");
            Throws<ArgumentOutOfRangeException>(() => ServiceabilityChecks.Combination(3));
        });
        Run("SLE tensioni: compressione uniforme con soluzione analitica", StressChecks);
        Run("SLE fessure: durata, classe ambientale e confronto GPC", CrackChecks);
        Run("SLE fessure: input non valido e analisi non lineare non valutabile", InvalidCrack);
        Run("SLE decompressione: analisi ausiliaria GPC", Decompression);
        Run("SLU: domanda nulla e punto resistente convergente", Resistance);
        FeatureChecks.RunAll(Run);
        if (args.Contains("--grasshopper"))
        {
            Run("Caricamento assembly e 61 componenti Grasshopper", GrasshopperSmoke.Run);
            Run("Porte Grasshopper: unità, taglio X/Y, errori e SLE non applicabili", ComponentBoundary.Run);
        }
        Console.WriteLine($"TOTAL: {passed} PASS, {failed} FAIL");
        return failed == 0 ? 0 : 1;
    }

    internal static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
    internal static void Close(double expected, double actual, string message, double tolerance = 1e-9)
        => Check(double.IsFinite(actual) && Math.Abs(expected - actual) <= tolerance * Math.Max(1, Math.Abs(expected)),
            $"{message}: expected {expected:G17}, actual {actual:G17}");
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
    private static void Run(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e); }
    }
    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ConcreteSectionChecker.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
    internal static string RepositoryRoot => Root;
    private static void Compatibility()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "tests/Compatibility/components-66555b.json")));
        var entries = manifest.RootElement.GetProperty("components").EnumerateArray().ToArray();
        Check(entries.Length == 32, "Expected 32 historical components.");
        var guids = new HashSet<Guid>();
        var files = Directory.GetFiles(Path.Combine(Root, "src/ConcreteSectionChecker.Grasshopper/Components"), "*.cs");
        Check(files.Length == entries.Length, "Unexpected component count.");
        foreach (var entry in entries)
        {
            string path = Path.Combine(Root, "src/ConcreteSectionChecker.Grasshopper/Components", entry.GetProperty("file").GetString());
            string source = File.ReadAllText(path);
            var guid = Guid.Parse(Regex.Match(source, "ComponentGuid\\s*=>\\s*new Guid\\(\"([^\"]+)\"\\)").Groups[1].Value);
            Check(guid == Guid.Parse(entry.GetProperty("guid").GetString()) && guids.Add(guid), path + " GUID");
            foreach (string direction in new[] { "Input", "Output" })
            {
                string registration = Regex.Match(source, @"(?s)protected override void Register" + direction + @"Params.*?\{(.*?)\n        \}").Groups[1].Value.Trim();
                string expected = entry.GetProperty(direction.ToLowerInvariant() + "Registration").GetString();
                Check(registration.Replace("\r\n", "\n") == expected.Replace("\r\n", "\n"), path + " " + direction + " registration changed.");
            }
        }
    }
    private static void LibraryHashes()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "lib/GPC/release-set.json")));
        Check(!manifest.RootElement.GetProperty("candidate").GetBoolean(), "Candidate set.");
        var files = Directory.GetFiles(Path.Combine(Root, "lib/GPC"), "*.dll");
        Check(files.Length == 6, "Unexpected GPC dependencies.");
        foreach (var file in files)
        {
            var entry = manifest.RootElement.GetProperty("files").EnumerateArray().Single(e => e.GetProperty("file").GetString() == Path.GetFileName(file));
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file)));
            Check(hash.Equals(entry.GetProperty("sha256").GetString(), StringComparison.OrdinalIgnoreCase), file + " hash mismatch");
        }
    }
    internal static SectionCheckerModelCode2010 Checker(StandardModelCode2010 standard = null, bool tensile = false)
    {
        var shape = new Shape2d(new Polygon2d(new[] { new Point2d(-150, -250), new Point2d(150, -250), new Point2d(150, 250), new Point2d(-150, 250) }));
        var section = new ReinforcedConcreteSection(shape, ConcreteMaterialEN1992Data.C30_37);
        var bar = new RebarSectionCircular(20, SteelMaterialEN1992Data.B450C);
        section.AddRebars(new[] { new Point2d(-110, -210), new Point2d(110, -210), new Point2d(110, 210), new Point2d(-110, 210) }
            .Select(p => new ReinforcedConcreteRebar(bar, p)));
        return new SectionCheckerModelCode2010(new SectionCheckerAttribute(section),
            new SectionCheckerModelCode2010.SectionOptionsModelCode2010
            {
                ForceReferenceCoordinateSystem = CoordinateSystem.Global,
                StressAnalysisType = SectionSolver.StressAnalysisTypes.Linear,
                FailureAnalysisType = SectionSolver.FailureAnalysisTypes.ConstantN,
                DomainPointStrategy = SectionSolver.DomainPointStrategyTypes.Intersection,
                ConsiderTensileConcrete = tensile
            }, standard ?? new StandardNTC2018Concrete());
    }
    internal static ResultBeamForces Force(double n = -300000, double v1 = 60000, double v2 = 90000, double m1 = 45e6, double m2 = 15e6)
        => new ResultBeamForces(n, v1, v2, 0, m1, m2, CoordinateSystem.Global);
    private static void AnalyticalShear()
    {
        var r = ShearChecks.WithoutStirrups(Checker(), Force(), 500, 300, 20, 4, 30, 150000);
        // Independent scalar check of the mapped inputs: d=460 mm, As=4*pi*20²/4, sigmaCp=2 MPa.
        double k = 1 + Math.Sqrt(200d / 460), rho = 400 * Math.PI / (300 * 460);
        double expected = (.18 / 1.5 * k * Math.Pow(100 * rho * 30, 1d / 3) + .15 * 2) * 300 * 460;
        Close(expected, r.VRsd, "VRsd [N]");
        Close(.035 * Math.Pow(k, 1.5) * Math.Sqrt(30) * 300 * 460 + .15 * 2 * 300 * 460, r.VRcd, "VRcd [N]");
        Close(90000 / Math.Max(r.VRsd, r.VRcd), r.Ratio.Value, "Ratio");
    }
    private static void RectangularMapping(StandardModelCode2010 standard)
    {
        var checker = Checker(standard);
        var r = ShearChecks.Rectangular(checker, Force(), 500, 300, 8, 2, 4, 150, 60, 90, 16, 20, 3, 4, 30, 150000);
        var concrete = (ConcreteMaterialEuropeanCommon)checker.SectionCheckerAttribute.Section.ConcreteMaterial;
        var steel = checker.SectionCheckerAttribute.Section.Rebars.First().RebarMaterial;
        for (int i = 0; i < 4; i++)
        {
            bool x = i % 2 == 0;
            var direct = SectionShearCalculator.Calculate(new SectionShearInput(standard, -300000, x ? 60000 : 90000,
                x ? 15e6 : 45e6, 150000, x ? 500 : 300, x ? 262 : 460,
                x ? 3 * Math.PI * 64 : 400 * Math.PI, 30, Math.Abs(concrete.CalculateFcd(standard)),
                steel.CalculateFyd(standard), standard.GammaC, steel.E,
                i < 2 ? 0 : (x ? 2 : 4) * Math.PI * 16, i < 2 ? 1 : 150, i < 2 ? 90 : x ? 60 : 90));
            Close(direct.VRd, r[i].VRd, "Axis " + i);
            Close(direct.Ratio.Value, r[i].Ratio.Value, "Ratio " + i);
        }
        var noLinks = ShearChecks.Rectangular(checker, Force(), 500, 300, 0, 0, 0, 0, 0, 0, 16, 20, 3, 4, 30, 150000);
        Check(noLinks[2] == null && noLinks[3] == null, "Absent links must not produce a passed check.");
    }
    private static void Circular()
    {
        var r = ShearChecks.Circular(Checker(), Force(), 600, 10, 2, 150, 90, 20, 40);
        Close(Math.Sqrt(60000d * 60000 + 90000d * 90000), r.Demand, "Resultant");
        Close(.75 * 540, r.Details.Single(d => d.Symbol == "z").Value, "GPC lever arm");
    }
    private static void InvalidShear()
    {
        Throws<ArgumentException>(() => ShearChecks.WithoutStirrups(Checker(), Force(), 500, 300, 20, 3.5, 30, 150000));
        Throws<ArgumentException>(() => ShearChecks.WithoutStirrups(Checker(), Force(), 500, 300, 20, 4, 600, 150000));
        Throws<ArgumentException>(() => ShearChecks.Circular(Checker(new StandardEN1992p11()), Force(), 600, 10, 2, 150, 90, 20, 40));
        Throws<ArgumentException>(() => ShearChecks.Circular(Checker(), Force(), 600, 10, 2, 0, 90, 20, 40));
        Throws<ArgumentException>(() => ShearChecks.WithoutStirrups(Checker(), Force(), 500, 300, 20, 4, 30, double.NaN));
    }
    private static void StressChecks()
    {
        var checker = Checker();
        var force = Force(-300000, 0, 0, 0, 0);
        var stress = checker.GetTensionAnalysisResult(force);
        var c = (ConcreteMaterialEuropeanCommon)checker.SectionCheckerAttribute.Section.ConcreteMaterial;
        double n = SteelMaterialEN1992Data.B450C.E / c.E;
        double area = 300 * 500 + (n - 1) * 400 * Math.PI;
        var characteristic = ServiceabilityChecks.Stress(stress, 2);
        Close(-300000 / area, characteristic.ConcreteMinStress, "Uniform concrete compression", 1e-6);
        Close(Math.Abs(n * -300000 / area), characteristic.SteelMaxStress, "Uniform steel compression", 1e-6);
        Close(18, characteristic.ConcreteLimit.Value, "Characteristic concrete limit");
        Close(13.5, ServiceabilityChecks.Stress(stress, 0).ConcreteLimit.Value, "Quasi-permanent concrete limit");
        Check(ServiceabilityChecks.Stress(stress, 1).Ratio == null, "Frequent has no stress utilization.");
    }
    private static StressAnalysisResult CrackedState()
    {
        var checker = Checker();
        return checker.GetTensionAnalysisResult(Force(-100000, 0, 0, 80e6, 0));
    }
    private static void CrackChecks()
    {
        var stress = CrackedState();
        var section = (ReinforcedConcreteSection)stress.ConcreteSection;
        var concrete = (ConcreteMaterialEuropeanCommon)section.ConcreteMaterial;
        foreach (int duration in new[] { 0, 1 })
        {
            var result = ServiceabilityChecks.Cracking(stress, 30, 0, 5, 1, 0, duration);
            var direct = SectionCrackCheck.Evaluate(new SectionCrackInput(stress.Standard, ServiceabilityCombination.QuasiPermanent,
                "XC4", false, null, CrackSectionGeometry.From(section), stress.StrainPlane,
                SectionCrackInput.OrdinaryBarStresses(stress, section), true, false, false, section.Rebars.First().RebarMaterial.E,
                concrete.Ecm, concrete.Fctm, duration == 0, true, 30));
            Check(result.Width.HasValue && result.Width > 0, result.Status);
            Close(direct.Width.Value, result.Width.Value, "Width [mm]");
            Close(.2, result.Limit.Value, "XC4 quasi-permanent limit");
            Close(duration == 0 ? .6 : .4, result.Details.Single(d => d.Symbol == "kt").Value, "GPC kt");
        }
        var noCheck = ServiceabilityChecks.Cracking(stress, 30, 2, 5, 1, 0, 1);
        Check(noCheck.Verdict == CrackVerdict.NotRequired && noCheck.Ratio == null, "Characteristic is not required.");
    }
    private static void InvalidCrack()
    {
        var stress = CrackedState();
        Throws<ArgumentOutOfRangeException>(() => ServiceabilityChecks.Cracking(stress, 30, 0, 99, 1, 0, 0));
        Throws<ArgumentOutOfRangeException>(() => ServiceabilityChecks.Cracking(stress, double.NaN, 0, 0, 1, 0, 0));
        var nonlinear = new StressAnalysisResult(stress.ConcreteSection, stress.Force, stress.StrainPlane,
            stress.SectionSolver, stress.Standard, false);
        var result = ServiceabilityChecks.Cracking(nonlinear, 30, 0, 0, 1, 0, 0);
        Check(result.Verdict == CrackVerdict.NotEvaluated && result.Ratio == null, result.Status);
    }
    private static void Decompression()
    {
        var checker = Checker();
        var stress = checker.GetTensionAnalysisResult(Force(-300000, 0, 0, 0, 0));
        var result = ServiceabilityChecks.Cracking(stress, 30, 0, 12, 0, 0, 1);
        Check(result.Requirement.Criterion == CrackCriterion.Decompression && result.Passed == true, result.Status);
        Check(result.Width == null && result.Ratio == null && result.UncrackedMaximumStress < 0, "Decompression is not wk/0.");
    }
    private static void Resistance()
    {
        var checker = Checker();
        var zero = ResistanceChecks.Evaluate(checker, Force(0, 0, 0, 0, 0));
        Check(zero.IsZeroDemand && zero.Utilization == 0, "Zero demand");
        var point = ResistanceChecks.RequirePoint(checker, Force(-300000, 0, 0, 100e6, 0));
        Check(point.Diagnostics.Status == CalculationStatus.Completed && point.M1 > 0, "Converged point");
        Close(-300000, point.N.Value, "Constant axial force", 1e-3);
        var outside = ResistanceChecks.Evaluate(checker, Force(-1e10, 0, 0, 100e6, 0));
        Check(outside.IsAxialForceOutsideDomain, "Axial force outside domain must be identified.");
    }
}
