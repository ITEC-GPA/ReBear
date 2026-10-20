using System;
using System.Globalization;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Detailing;
using GPC.Checkers.Concrete.Durability;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Torsion;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Results;
using GPC.Model.Standards;
using static Program;

internal static class FeatureChecks
{
    internal static void RunAll(Action<string, Action> run)
    {
        run("Inviluppi: concomitanza, trasporto, esiti incompleti e report", Envelopes);
        run("Taglio v2 e torsione: assi, materiali, interazione, input mancanti", ShearTorsion);
        run("Batch: ogni combinazione conserva anche gli esiti non valutati", Batch);
        run("Copriferro, ancoraggio e dettagli: minimi, mancati dati e verso di confronto", Detailing);
        run("Builder: aree analitiche, fori, copriferro, interferenze e casi invalidi", Geometry);
        run("Dominio e momento-curvatura: equilibrio assiale e isolamento opzioni", Response);
        run("Ricerca armature: selezione candidato e nessun candidato idoneo", Search);
    }
    private static void Reject(Action action)
    {
        try { action(); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException) { return; }
        throw new Exception("Expected invalid input rejection.");
    }
    private static void Envelopes()
    {
        var a = new ResultBeamForces(-200e3, 10e3, 80e3, 2e6, 30e6, 15e6, CoordinateSystem.Global, name: "A");
        var b = new ResultBeamForces(-400e3, -20e3, 50e3, -4e6, 80e6, 10e6, CoordinateSystem.Global, name: "B");
        var r = LoadEnvelopes.Calculate(new[] { a, b });
        Check(r.Count == 12 && r.Single(x => x.Component == "N" && !x.Maximum).Index == 1, "N min case");
        var maxM = r.Single(x => x.Component == "Mx" && x.Maximum);
        Close(b.V2, maxM.Force.V2, "Concomitant Vy retained");
        Check(maxM.Force.Name == "B", "Case identity retained");
        var axes = new CoordinateSystem(new Point3d(100, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
        var eccentric = new ResultBeamForces(-100e3, 0, 0, 0, 0, 0, axes);
        var transformed = LoadEnvelopes.Calculate(new[] { a, eccentric }).Single(x => x.Component == "My" && !x.Maximum);
        // (100,0,0) x (0,0,-100000) = (0,10000000,0).
        var all = LoadEnvelopes.Calculate(new[] { new ResultBeamForces(0,0,0,0,0,0,CoordinateSystem.Global), eccentric });
        Close(10e6, all.Single(x => x.Component == "My" && x.Maximum).Value, "Eccentricity [Nmm]");
        Reject(() => LoadEnvelopes.Calculate(new[] { Force(double.NaN) }));
        var ok = new CheckResult("S", "OK", "V", CheckStatus.Satisfied, .7);
        var pending = ok with { Case = "Missing", Status = CheckStatus.NotEvaluated, Ratio = null };
        var failed = ok with { Case = "Outside", Status = CheckStatus.NotSatisfied, Ratio = null };
        Check(CheckResults.Envelope(new[] { ok, pending }).Single().Status == CheckStatus.NotEvaluated, "Missing cannot pass");
        var env = CheckResults.Envelope(new[] { ok, pending, failed }).Single();
        Check(env.Status == CheckStatus.NotSatisfied && env.Governing == failed && env.NotEvaluated == 1, "Failure without ratio governs");
        Check(CheckResults.Envelope(new[] { ok with { Status = CheckStatus.NotRequired, Ratio = null } }).Single().Status == CheckStatus.NotRequired, "Not required");
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
            var escaped = ok with { Section = "A;\"B", Ratio = .75 };
            Check(CheckResults.Csv(new[] { escaped }).Contains("\"A;\"\"B\";") && CheckResults.Csv(new[] { escaped }).Contains("\"0.75\""), "Invariant escaped CSV");
            using var json = System.Text.Json.JsonDocument.Parse(CheckResults.Json(new[] { pending, failed }));
            Check(json.RootElement.GetProperty("results")[0].GetProperty("Ratio").ValueKind == System.Text.Json.JsonValueKind.Null, "Missing JSON ratio");
        }
        finally { CultureInfo.CurrentCulture = culture; }
    }
    internal static ShearOptions ShearOptions(double width = 300, double depth = 460) =>
        new(SteelMaterialEN1992Data.B450C, SteelMaterialEN1992Data.B450C, width, depth, 400 * Math.PI,
            2 * Math.PI * 8 * 8 / 4, 150, CotTheta: 1.5);
    private static void ShearTorsion()
    {
        var c = Checker(); var f = Force(); var p = ShearOptions();
        var x = AdvancedShearChecks.Shear(c, f, p, 0, "S");
        var y = AdvancedShearChecks.Shear(c, f, p, 1, "S");
        Close(60, x.Demand.Value, "Vx [kN]"); Close(90, y.Demand.Value, "Vy [kN]");
        Check(y.Ratio > x.Ratio, "Different demands");
        var lower = AdvancedShearChecks.Shear(c, f, p with { LinkSteel = SteelMaterialEN1992Data.B500C }, 1, "S");
        Check(lower.Capacity > y.Capacity, "Independent stirrup steel");
        var noLinks = AdvancedShearChecks.Shear(c, Force(100e3), p with { LinkArea = 0 }, 1, "S");
        Check(noLinks.Status == CheckStatus.NotEvaluated && noLinks.Ratio == null, "Axial tension remains pending");
        Reject(() => AdvancedShearChecks.Shear(c, f, p with { Depth = -1 }, 0, "S"));
        var torsion = new TorsionOptions(new TorsionGeometry(200 * 400, 1200, 100),
            p.LinkSteel, p.LongitudinalSteel, Math.PI * 8 * 8 / 4, 150, 800, 1.5);
        var ft = new ResultBeamForces(-300e3, 20e3, 30e3, 5e6, 45e6, 15e6, CoordinateSystem.Global);
        var combined = AdvancedShearChecks.Torsion(c, ft, torsion, p, p, "S");
        var pure = AdvancedShearChecks.Torsion(c, new ResultBeamForces(-300e3,0,0,5e6,45e6,15e6,CoordinateSystem.Global), torsion, null, null, "S");
        Check(combined.Ratio >= pure.Ratio && combined.Capacity > 0, "Concomitant shear cannot improve torsion utilization");
        Reject(() => AdvancedShearChecks.Torsion(c, ft, torsion, p, null, "S"));
        Reject(() => AdvancedShearChecks.Torsion(c, ft, torsion, p with { Spacing = 200 }, p, "S"));
    }
    private static void Batch()
    {
        var r = SectionChecks.Batch(Checker(), new[] { Force(), Force(double.NaN), Force(-1e10) }, "S", true,
            y: ShearOptions(), sle: new SleOptions(0, "XC4", false, false, true, 30));
        Check(r.Count == 12 && r.Count(x => x.Case == "Caso 2") == 4, "Every requested case/check retained");
        Check(r.Where(x => x.Case == "Caso 2").All(x => x.Status == CheckStatus.NotEvaluated), "Invalid loads never pass");
        Check(r.Single(x => x.Case == "Caso 3" && x.Check == "N-Mx-My").Status == CheckStatus.NotSatisfied, "Outside axial domain fails");
    }
    private static void Detailing()
    {
        var standard = new StandardEN1992p11();
        var input = new CoverInput(new[] { "XC4" }, 30, 50, false, false, false, 20, 20, 10);
        var cover = DetailingChecks.Cover(standard, input, 35, "S");
        Close(40, cover.Native.Nominal, "EN S4 XC4: cnom 30+10 mm");
        Check(cover.Result.Status == CheckStatus.NotSatisfied && cover.Result.Ratio == null, "Insufficient cover must fail");
        var anchorage = DetailingChecks.Anchorage(standard, ConcreteMaterialEN1992Data.C30_37, SteelMaterialEN1992Data.B450C,
            20, 300, 1000, true, false, 100, 0, "S");
        double fbd = 2.25 * .7 * .3 * Math.Pow(30, 2d / 3) / 1.5;
        Close(fbd, anchorage.Native.Fbd, "fbd analytical [MPa]");
        Close(20 * 300 / (4 * fbd), anchorage.Native.BasicLength, "lb,rqd analytical [mm]");
        Check(anchorage.Result.Status == CheckStatus.Satisfied, "Long anchorage passes");
        Reject(() => DetailingChecks.Anchorage(standard, ConcreteMaterialEN1992Data.C30_37, SteelMaterialEN1992Data.B450C,
            20, 1e5, 1000, true, false, 100, 0, "S"));
        var p = new DetailingOptions(MemberDetailingKind.Column, 300, 300, 300, 8, 150, 2, 30, 20, 10, null, false, false, false);
        var detail = DetailingChecks.Member(Checker(), p, -300e3, "S");
        Check(detail.Count > 5 && detail.Any(x => x.Status == CheckStatus.NotEvaluated), "Missing detailing confirmations remain pending");
        var slab = p with { Kind = MemberDetailingKind.Slab };
        Reject(() => DetailingChecks.Member(Checker(), slab, 0, "S"));
    }
    private static void Geometry()
    {
        var c = ConcreteMaterialEN1992Data.C30_37; var s = SteelMaterialEN1992Data.B450C;
        var rect = SectionBuilders.Rectangle(c, s, 300, 500, 30, 8, 20, 4, 4, 0);
        Close(150000, rect.Area, "Rectangle Ac"); Close(800 * Math.PI, rect.AreaRebars, "Rectangle As");
        Close(38, SectionBuilders.Inspect(rect).MinimumBarCover, "Cover to longitudinal bars");
        var circle = SectionBuilders.Circle(c,s,600,40,10,20,12,64);
        Close(64 * .5 * 300 * 300 * Math.Sin(2*Math.PI/64), circle.Area, "Inscribed polygon area");
        Check(SectionBuilders.Inspect(circle).MinimumBarCover >= 50 - 1e-6, "Polygon cover conservative");
        var tee = SectionBuilders.Tee(c,s,800,700,300,180,30,8,20,6,3,2);
        Close(800 * 180 + 300 * 520, tee.Area, "T area");
        var shapeI = SectionBuilders.ISection(c,s,600,500,800,240,180,180,30,8,20,5,4,2);
        Close(600*180 + 500*180 + 240*440, shapeI.Area, "I area");
        var box = SectionBuilders.Box(c,s,1000,1200,500,700,35,10,20,6,8,4,6);
        Close(1000*1200-500*700, box.Area, "Box net area");
        Check(box.ConcreteShape.Holes2d.Length == 1 && SectionBuilders.Inspect(box).MinimumBarCover >= 45-1e-6, "Inner cover");
        Reject(() => SectionBuilders.Rectangle(c,s,100,100,40,10,20,4,4,0));
        Reject(() => SectionBuilders.Rectangle(c,s,300,500,30,8,20,100,4,0));
        Reject(() => SectionBuilders.Circle(c,s,600,40,10,20,12,17));
        Reject(() => SectionBuilders.Box(c,s,600,700,590,690,30,8,20,4,4,0,0));
    }
    private static void Response()
    {
        var c = Checker(); var p = c.SectionCheckerOptions; int n = p.TetaDiscretization;
        var slice = SectionAnalyses.Slice(c, -300e3, 8);
        Check(slice.Count == 8 && slice.All(x => double.IsFinite(x.Mx) && double.IsFinite(x.My)), "Finite domain");
        foreach (var x in slice) Close(-300, x.N, "Slice N [kN]", 1e-3);
        Check(p.TetaDiscretization == n, "Domain sampling does not mutate source options");
        var nonlinear = SectionAnalyses.Detached(c);
        nonlinear.SectionCheckerOptions.StressAnalysisType = SectionSolver.StressAnalysisTypes.NonLinear;
        var r = SectionAnalyses.MomentCurvature(nonlinear, -300e3, 0, 10, .8, 1000);
        Check(r.Points.Count >= 2 && Math.Abs(r.AxialResidual) <= 1000, "M-chi equilibrium");
        Check(r.Points.All(x => double.IsFinite(x.Curvature) && double.IsFinite(x.Moment)), "M-chi finite");
        Reject(() => SectionAnalyses.MomentCurvature(c, -300e3, 0, 10, .8, 1000));
    }
    private static void Search()
    {
        var c = Checker();
        var r = SectionAnalyses.Search(new[] { c, SectionAnalyses.Detached(c) }, new[] { Force(-300e3,0,0,20e6,0) }, 1);
        Check(r.Index == 0 && r.Results.Count == 2, "Stable tie: first admissible candidate");
        Check(SectionAnalyses.Search(new[] { c }, new[] { Force(-1e10,0,0,20e6,0) }, 1).Index == -1, "No admissible candidate");
    }
}
