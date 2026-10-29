using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

// Editable vector primitives are the single source for SVG and raster resources.
// No Rhino/Grasshopper dependency; PNGs are rendered at their actual output size.
internal static class Program
{
    internal const string Ink = "#283E4A", Teal = "#008B8B", Steel = "#D99235", Grey = "#DDE5E8",
        Blue = "#3D83BD", Red = "#D65852", White = "#FFFFFF";
    private static int Main(string[] args)
    {
        string root = args.Length == 1 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
        string assets = Path.Combine(root, "assets"), resources = Path.Combine(root, "src/ConcreteSectionChecker.Grasshopper/Resources/Icons");
        Directory.CreateDirectory(Path.Combine(assets, "icons"));
        Directory.CreateDirectory(Path.Combine(assets, "branding"));
        Directory.CreateDirectory(resources);
        // Every concrete component class of the plugin (Components and Features), one icon each.
        var names = new[] { "Components", "Features" }
            .SelectMany(folder => Directory.GetFiles(Path.Combine(root, "src/ConcreteSectionChecker.Grasshopper", folder), "*.cs"))
            .SelectMany(file => System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(file),
                @"public sealed class (\w+) : ").Select(m => m.Groups[1].Value))
            .Distinct().OrderBy(n => n).ToArray();
        foreach (string name in names)
        {
            using var icon = new VectorIcon(96);
            Draw(icon, name);
            icon.Save(Path.Combine(assets, "icons", name));
            using var small = new VectorIcon(24);
            Draw(small, name);
            small.Bitmap.Save(Path.Combine(resources, name + ".png"), ImageFormat.Png);
        }
        using (var logo = new VectorIcon(512))
        {
            Logo(logo);
            logo.Save(Path.Combine(assets, "branding", "symbol"));
        }
        using (var logo = new VectorIcon(24))
        {
            Logo(logo);
            logo.Bitmap.Save(Path.Combine(resources, "Plugin.png"), ImageFormat.Png);
        }
        Gallery(assets, names);
        Console.WriteLine($"Generated {names.Length} icons (SVG, PNG 96px, embedded PNG 24px), logo and gallery.");
        return 0;
    }

    internal static void Logo(VectorIcon p)
    {
        p.Circle(5, 5, 3.4f, Ink);
        p.Circle(19, 5, 3.4f, Ink);
        p.Circle(5, 5, 1.8f, Teal);
        p.Circle(19, 5, 1.8f, Teal);
        p.Poly(new[] { (5f, 5f), (19f, 5f), (22f, 11f), (20f, 19f), (16f, 22f),
            (8f, 22f), (4f, 19f), (2f, 11f) }, Ink, "none", 0);
        p.Poly(new[] { (8f, 12f), (16f, 12f), (18f, 16f), (16f, 20f), (8f, 20f), (6f, 16f) }, Grey, "none", 0);
        p.Rect(6, 11, 12, 9, "none", Teal, 1.5f);
        foreach (var (x, y) in new[] { (6f, 11f), (18f, 11f), (6f, 20f), (18f, 20f) }) p.Circle(x, y, 1.3f, Steel);
        p.Circle(8.5f, 9.2f, .8f, White); p.Circle(15.5f, 9.2f, .8f, White);
        p.Poly(new[] { (9.5f, 14f), (14.5f, 14f), (12f, 16.5f) }, Ink, "none", 0);
        p.Line(12, 16, 12, 18, Ink, 1);
    }

    internal static void SectionCheck(VectorIcon p)
    {
        p.Rect(3, 3, 16, 18, Grey, Ink, 1.6f);
        p.Rect(6, 6, 10, 12, "none", Teal, 1.1f);
        foreach (var (x, y) in new[] { (6f, 6f), (16f, 6f), (6f, 18f), (16f, 18f) }) p.Circle(x, y, 1.55f, Steel, Ink, .8f);
        p.Line(11, 12, 15, 16, White, 4);
        p.Line(15, 16, 22, 7, White, 4);
        p.Line(11, 12, 15, 16, Teal, 2.4f);
        p.Line(15, 16, 22, 7, Teal, 2.4f);
    }
    private static void Section(VectorIcon p, bool circular = false, bool link = true)
    {
        if (circular) p.Circle(11, 12, 8, Grey, Ink, 1.5f);
        else p.Rect(3, 3, 16, 18, Grey, Ink, 1.5f);
        if (link)
        {
            if (circular) p.Circle(11, 12, 5.5f, "none", Teal, 1.2f);
            else p.Rect(6, 6, 10, 12, "none", Teal, 1.2f);
        }
        foreach (var (x, y) in circular ? new[] { (7f, 8f), (15f, 8f), (7f, 16f), (15f, 16f) }
            : new[] { (6f, 6f), (16f, 6f), (6f, 18f), (16f, 18f) }) p.Circle(x, y, 1.3f, Steel, Ink, .6f);
    }
    private static void Arrow(VectorIcon p, float x1, float y1, float x2, float y2, string color)
    {
        p.Line(x1, y1, x2, y2, color, 1.6f);
        float dx = x2 - x1, dy = y2 - y1, n = MathF.Sqrt(dx * dx + dy * dy);
        dx /= n; dy /= n;
        p.Poly(new[] { (x2 - 3 * dx + 1.7f * dy, y2 - 3 * dy - 1.7f * dx), (x2, y2),
            (x2 - 3 * dx - 1.7f * dy, y2 - 3 * dy + 1.7f * dx) }, "none", color, 1.5f, false);
    }
    private static void Tick(VectorIcon p, float x = 13, float y = 18)
        => p.Poly(new[] { (x - 2, y), (x + 1, y + 3), (x + 7, y - 4) }, "none", Teal, 2.2f, false);
    private static void Page(VectorIcon p)
    {
        p.Poly(new[] { (4f, 2f), (15f, 2f), (20f, 7f), (20f, 22f), (4f, 22f) }, White, Ink, 1.3f);
        p.Poly(new[] { (15f, 2f), (15f, 7f), (20f, 7f) }, Grey, Ink, 1, false);
    }
    private static void Domain(VectorIcon p)
    {
        p.Poly(new[] { (12f, 2f), (20f, 8f), (17f, 19f), (12f, 22f), (5f, 18f), (3f, 8f) }, "#BEE3E3", Ink, 1.2f);
        p.Poly(new[] { (12f, 2f), (8f, 10f), (12f, 22f), (16f, 10f), (12f, 2f) }, "none", Teal, 1);
        p.Poly(new[] { (3f, 8f), (8f, 10f), (16f, 10f), (20f, 8f) }, "none", Teal, 1, false);
        p.Line(5, 18, 17, 19, Teal, 1);
    }
    private static void Mesh(VectorIcon p, bool gradient)
    {
        p.Poly(new[] { (3f, 4f), (13f, 3f), (10f, 13f) }, gradient ? Red : "#9ACDCB", Ink, .8f);
        p.Poly(new[] { (13f, 3f), (21f, 7f), (10f, 13f) }, Steel, Ink, .8f);
        p.Poly(new[] { (3f, 4f), (10f, 13f), (3f, 20f) }, gradient ? "#E4B759" : "#9ACDCB", Ink, .8f);
        p.Poly(new[] { (10f, 13f), (21f, 7f), (20f, 21f) }, gradient ? "#8DC9BA" : Steel, Ink, .8f);
        p.Poly(new[] { (3f, 20f), (10f, 13f), (20f, 21f) }, Blue, Ink, .8f);
    }
    private static void Draw(VectorIcon p, string name)
    {
        switch (name)
        {
            case "LoadEnvelopeComponent":
                p.Line(3, 21, 21, 21, Ink, 1); p.Line(3, 3, 3, 21, Ink, 1);
                p.Poly(new[] { (3f, 15f), (9f, 5f), (15f, 9f), (21f, 3f) }, "none", Red, 2, false);
                p.Poly(new[] { (3f, 19f), (9f, 12f), (15f, 17f), (21f, 9f) }, "none", Blue, 2, false); break;
            case "CheckEnvelopeComponent":
                Draw(p, "LoadEnvelopeComponent"); Tick(p, 12, 18); break;
            case "CheckResultComponent": Page(p); Tick(p, 10, 13); break;
            case "CheckReportComponent": Page(p);
                for (int i = 0; i < 3; i++) { p.Line(7, 11 + i * 4, 17, 11 + i * 4, Teal, 1); p.Circle(8, 11 + i * 4, 1, Steel); } break;
            case "ShearOptionsComponent": Draw(p, "CheckerOptionsComponent"); Arrow(p, 3, 22, 21, 22, Blue); break;
            case "RectangularShearOptionsComponent": Section(p); Arrow(p, 21, 21, 21, 3, Blue); Arrow(p, 2, 22, 18, 22, Blue); break;
            case "CircularShearOptionsComponent": Section(p, true); Arrow(p, 21, 21, 21, 3, Blue); Arrow(p, 2, 22, 18, 22, Blue); break;
            case "ShearCheckComponent": Section(p); Arrow(p, 21, 21, 21, 3, Blue); Tick(p, 12, 18); break;
            case "TorsionOptionsComponent": Section(p); p.Text("T", 8, 6, 12, Ink); break;
            case "TorsionShearCheckComponent": Section(p); p.Text("T", 8, 6, 12, Ink); Arrow(p, 21, 21, 21, 3, Blue); break;
            case "ResistanceCheckComponent": SectionCheck(p); p.Text("N", 7, 1, 8, Ink); break;
            case "SleOptionsComponent": Draw(p, "CheckerOptionsComponent"); p.Text("S", 17, 13, 10, Blue); break;
            case "ServiceabilityCheckComponent": Draw(p, "ServiceabilityCrackWidthControlCheckComponent"); Tick(p, 12, 18); break;
            case "BatchSectionChecksComponent":
                p.Rect(7, 2, 15, 15, Grey, Ink, 1); p.Rect(4, 5, 15, 15, White, Ink, 1); p.Rect(1, 8, 15, 15, Grey, Ink, 1); Tick(p, 11, 16); break;
            case "ConcreteCoverComponent": Section(p); p.Line(2, 12, 7, 12, Red, 2.5f); break;
            case "AnchorageLapComponent":
                p.Rect(2, 3, 20, 18, Grey, Ink, 1); p.Poly(new[] { (3f, 8f), (17f, 8f), (19f, 11f), (19f, 17f) }, "none", Steel, 3, false);
                p.Line(4, 19, 16, 19, Teal, 1.5f); break;
            case "PlateDetailingOptionsComponent":
                p.Rect(2, 7, 20, 10, Grey, Ink, 1); for (int i = 0; i < 5; i++) p.Circle(4 + i * 4, 14, 1, Steel); break;
            case "DetailingOptionsComponent": Section(p); p.Text("d", 16, 12, 11, Blue); break;
            case "ReinforcementDetailingComponent": Section(p); Tick(p, 12, 18); break;
            case "MomentCurvatureComponent":
                p.Line(3, 21, 22, 21, Ink, 1); p.Line(3, 21, 3, 2, Ink, 1);
                p.Poly(new[] { (3f, 21f), (8f, 7f), (17f, 4f), (21f, 6f) }, "none", Teal, 2, false); p.Circle(8, 7, 1.7f, Steel); break;
            case "DomainSliceComponent": Draw(p, "FailureDomainSummaryComponent"); p.Line(1, 12, 23, 12, Red, 2); break;
            case "EffectiveCrackedSectionComponent":
                Section(p); p.Rect(4, 15, 14, 5, "#BEE3E3", Teal, 1); p.Line(10, 14, 13, 21, Red, 1.5f); break;
            case "HomogenizationOptionsComponent": p.Text("φ", 1, 0, 18, Teal); p.Text("n", 15, 10, 13, Ink); break;
            case "ReinforcementSearchComponent":
                Section(p); p.Circle(16, 15, 5, White, Blue, 1.7f); p.Line(19, 19, 23, 23, Blue, 2); break;
            case "RectangularRcSectionComponent": Section(p); p.Line(1, 23, 20, 23, Blue, 1); break;
            case "CircularRcSectionComponent": Section(p, true); break;
            case "TeeRcSectionComponent":
                p.Poly(new[] { (2f, 3f), (22f, 3f), (22f, 8f), (15f, 8f), (15f, 22f), (9f, 22f), (9f, 8f), (2f, 8f) }, Grey, Ink, 1.3f);
                foreach (var (x,y) in new[] { (5f,5.5f), (19f,5.5f), (11f,19f), (13f,19f) }) p.Circle(x,y,1,Steel); break;
            case "IRcSectionComponent":
                p.Poly(new[] { (3f, 2f), (21f, 2f), (21f, 7f), (15f, 7f), (15f, 17f), (21f, 17f), (21f, 22f),
                    (3f,22f), (3f,17f), (9f,17f), (9f,7f), (3f,7f) }, Grey, Ink, 1.3f);
                foreach (var (x,y) in new[] { (6f,4.5f), (18f,4.5f), (6f,19.5f), (18f,19.5f) }) p.Circle(x,y,1,Steel); break;
            case "BoxRcSectionComponent":
                Section(p); p.Rect(8, 8, 6, 8, White, Ink, 1); break;
            case "RebarLayoutComponent":
                p.Poly(new[] { (3f,20f), (3f,6f), (12f,3f), (21f,6f), (21f,20f) }, "none", Teal, 1, false);
                foreach (var (x,y) in new[] { (3f,20f), (3f,13f), (3f,6f), (12f,3f), (21f,6f), (21f,13f), (21f,20f) }) p.Circle(x,y,1.7f,Steel,Ink,.6f); break;
            case "SectionGeometryCheckComponent": SectionCheck(p); p.Line(1, 23, 22, 23, Blue, 1); break;
            case "CheckerComponent": SectionCheck(p); break;
            case "ReinforcedConcreteSectionComponent": Section(p); break;
            case "CircularSectionShearCheckComponent":
                Section(p, true); Arrow(p, 2, 22, 21, 22, Blue); break;
            case "RectangularSectionShearCheckComponent":
                Section(p); Arrow(p, 21, 21, 21, 3, Blue); break;
            case "RectangularSectionShearCheckNoStirrupComponent":
                Section(p, link: false); Arrow(p, 21, 21, 21, 3, Blue); break;
            case "CheckerOptionsComponent":
                for (int i = 0; i < 3; i++)
                {
                    int y = 5 + 7 * i;
                    p.Line(3, y, 21, y, Ink, 1.5f);
                    p.Circle(i == 1 ? 16 : 7, y, 2.5f, i == 1 ? Steel : Teal, White, 1);
                }
                break;
            case "StandardComponent":
                Page(p); p.Line(7, 10, 15, 10, Ink, 1.3f); p.Line(7, 14, 13, 14, Ink, 1.3f); Tick(p, 12, 18); break;
            case "ConcreteMaterialComponent":
            case "ConcreteDesignValue":
                p.Poly(new[] { (3f, 7f), (14f, 3f), (21f, 7f), (11f, 11f) }, Grey, Ink, 1.2f);
                p.Poly(new[] { (3f, 7f), (11f, 11f), (11f, 22f), (3f, 18f) }, "#B6C7CE", Ink, 1.2f);
                p.Poly(new[] { (11f, 11f), (21f, 7f), (21f, 18f), (11f, 22f) }, "#E9EDEF", Ink, 1.2f);
                if (name == "ConcreteDesignValue") Tick(p, 13, 17);
                else { p.Circle(6, 13, .8f, Ink); p.Circle(17, 12, 1, Ink); p.Circle(14, 18, .8f, Ink); }
                break;
            case "BarMaterialComponent":
                p.Rect(4, 4, 7, 16, Steel, Ink, 1.3f); p.Line(13, 5, 20, 5, Ink, 1.8f);
                p.Line(13, 10, 20, 10, Ink, 1.8f); p.Line(13, 15, 20, 15, Ink, 1.8f); p.Line(13, 20, 20, 20, Ink, 1.8f); break;
            case "RebarMaterialComponent":
            case "SteelDesignValue":
                p.Poly(new[] { (3f, 7f), (18f, 3f), (21f, 7f), (6f, 12f), (3f, 7f) }, Steel, Ink, 1.3f);
                p.Poly(new[] { (3f, 15f), (18f, 11f), (21f, 15f), (6f, 20f), (3f, 15f) }, Steel, Ink, 1.3f);
                if (name == "SteelDesignValue") Tick(p, 12, 19);
                else for (int i = 0; i < 4; i++) p.Line(7 + i * 3, 5 + (3 - i) * .6f, 9 + i * 3, 10 - i * .8f, Ink, .8f);
                break;
            case "TendonMaterialComponent":
                for (int i = 0; i < 3; i++)
                    p.Poly(new[] { (3f, 7f + i * 5), (8f, 5f + i * 5), (16f, 9f + i * 5), (21f, 7f + i * 5) }, "none", Steel, 2, false);
                break;
            case "RebarComponent":
                p.Circle(12, 10, 6, Steel, Ink, 1.5f); p.Circle(12, 10, 2, "#F7D291");
                p.Line(5, 20, 19, 20, Ink, 1.3f); p.Line(5, 18, 5, 22, Ink, 1.3f); p.Line(19, 18, 19, 22, Ink, 1.3f); break;
            case "TendonComponent":
                foreach (var (x, y) in new[] { (12f, 12f), (7f, 9f), (12f, 6f), (17f, 9f), (17f, 15f), (12f, 18f), (7f, 15f) })
                    p.Circle(x, y, 2.7f, Steel, Ink, .8f);
                break;
            case "CoordinateSystemComponent":
                Arrow(p, 5, 19, 21, 19, Red); Arrow(p, 5, 19, 5, 3, Blue); p.Circle(5, 19, 2, Teal); break;
            case "CoordinateSystemSectionComponent":
                p.Rect(4, 3, 15, 18, Grey, Ink, 1); Arrow(p, 11, 13, 22, 13, Red); Arrow(p, 11, 13, 11, 2, Blue); p.Circle(11, 13, 1.8f, Teal); break;
            case "ConcreteMechanicalPropertiesComponent":
            case "HomogenizedMechanicalPropertiesComponent":
                p.Rect(4, 3, 16, 18, Grey, Ink, 1.3f);
                if (name == "HomogenizedMechanicalPropertiesComponent") { p.Rect(5, 4, 4, 16, Steel, "none"); p.Rect(15, 4, 4, 16, Steel, "none"); }
                p.Line(2, 12, 22, 12, Teal, 1); p.Line(12, 1, 12, 23, Teal, 1); p.Circle(12, 12, 2.5f, White, Teal, 1.6f); break;
            case "SectionForceComponent":
                p.Rect(8, 6, 8, 12, Grey, Ink, 1.3f);
                Arrow(p, 12, 1, 12, 8, Red); Arrow(p, 12, 23, 12, 16, Red);
                Arrow(p, 1, 12, 8, 12, Blue); Arrow(p, 23, 12, 16, 12, Blue); break;
            case "PsiFactorComponent":
                p.Text("φ", 3, 0, 21, Teal); p.Line(3, 22, 21, 22, Ink, 1); break;
            case "StressAnalysisComponent":
                p.Rect(2, 4, 7, 17, Grey, Ink, 1.3f);
                p.Poly(new[] { (12f, 3f), (21f, 3f), (12f, 12f) }, Red, Red, .7f);
                p.Poly(new[] { (12f, 12f), (21f, 21f), (12f, 21f) }, "#9BD8D5", Teal, .7f); p.Line(12, 2, 12, 22, Ink, 1.1f); break;
            case "ServiceabilityStressCheckComponent":
                p.Text("σ", 1, 0, 21, Ink); Tick(p, 12, 18); break;
            case "ServiceabilityCrackWidthControlCheckComponent":
                p.Rect(3, 3, 18, 18, Grey, Ink, 1.3f);
                p.Poly(new[] { (13f, 3f), (10f, 8f), (14f, 11f), (10f, 15f), (12f, 21f) }, "none", Red, 1.8f, false);
                p.Line(4, 12, 8, 12, Teal, 1.4f); p.Line(16, 12, 20, 12, Teal, 1.4f); break;
            case "FailureDomain3DComponent": Domain(p); break;
            case "PostProcessFailureDomain3DComponent": Domain(p); Tick(p, 12, 18); break;
            case "FailureDomainSummaryComponent":
                p.Line(3, 12, 21, 12, Ink, 1); p.Line(12, 3, 12, 21, Ink, 1);
                p.Poly(new[] { (12f, 3f), (19f, 8f), (20f, 15f), (12f, 21f), (4f, 16f), (3f, 8f) }, "none", Teal, 2);
                foreach (var (x, y) in new[] { (12f, 3f), (20f, 12f), (12f, 21f), (3f, 12f) }) p.Circle(x, y, 1.6f, Steel); break;
            case "PostProcessStrainResultComponent":
                Page(p); p.Text("ε", 6, 6, 15, Teal); p.Line(7, 19, 17, 19, Ink, 1.2f); break;
            case "ViewStressAnalysisResultComponent":
                p.Rect(3, 3, 18, 18, Grey, Ink, 1.3f);
                p.Line(4, 9, 20, 9, Blue, 1.5f); p.Line(4, 15, 20, 15, Red, 1.5f);
                p.Circle(7, 6, 1.7f, Blue); p.Circle(17, 18, 1.7f, Red); break;
            case "ViewStressAnalysisResultMeshComponent":
                Mesh(p, true); p.Circle(12, 12, 3.5f, White, Ink, 1); p.Circle(12, 12, 1.5f, Ink); break;
            case "ColorGradientMeshComponent": Mesh(p, true); break;
            case "ColorMeshComponent": Mesh(p, false); break;
            default: throw new ArgumentException("Missing icon for " + name);
        }
    }

    private static void Gallery(string assets, string[] names)
    {
        using var board = new Bitmap(1280, 150 + ((names.Length + 3) / 4) * 96);
        using var g = Graphics.FromImage(board);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(ColorTranslator.FromHtml("#F6F8FA"));
        using var ink = new SolidBrush(ColorTranslator.FromHtml(Ink));
        using var muted = new SolidBrush(ColorTranslator.FromHtml("#647985"));
        using var title = new Font("Segoe UI", 27, FontStyle.Bold);
        using var caption = new Font("Segoe UI", 11);
        using var label = new Font("Segoe UI", 10);
        g.DrawString("ReBear", title, ink, 40, 25);
        g.DrawString($"Reinforced Concrete Sections · {names.Length} icone Grasshopper · SVG + PNG 24 / 96 px", caption, muted, 43, 79);
        using (var logo = Image.FromFile(Path.Combine(assets, "branding/symbol.png"))) g.DrawImage(logo, 1135, 18, 96, 96);
        for (int i = 0; i < names.Length; i++)
        {
            int x = 32 + (i % 4) * 312, y = 135 + (i / 4) * 96;
            using var tile = new SolidBrush(Color.White);
            g.FillRectangle(tile, x, y, 300, 83);
            using var icon = Image.FromFile(Path.Combine(assets, "icons", names[i] + ".png"));
            g.DrawImage(icon, x + 12, y + 12, 56, 56);
            string name = names[i].Replace("Component", "").Replace("Serviceability", "SLE ").Replace("RectangularSection", "Rect. ")
                .Replace("CircularSection", "Circ. ").Replace("HomogenizedMechanical", "Homog. ").Replace("ConcreteMechanical", "Concrete ");
            name = RegexWords(name);
            g.DrawString(name, label, ink, new RectangleF(x + 79, y + 13, 210, 52));
        }
        board.Save(Path.Combine(assets, "icon-gallery.png"), ImageFormat.Png);
    }
    private static string RegexWords(string value) => System.Text.RegularExpressions.Regex.Replace(value, @"(?<=[a-z])(?=[A-Z0-9])", " ");
}

internal sealed class VectorIcon : IDisposable
{
    internal Bitmap Bitmap { get; }
    private readonly Graphics graphics;
    private readonly StringBuilder svg = new StringBuilder("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"none\" stroke-linecap=\"round\" stroke-linejoin=\"round\">\n");
    internal VectorIcon(int size)
    {
        Bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        graphics = Graphics.FromImage(Bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.ScaleTransform(size / 24f, size / 24f);
        graphics.Clear(Color.Transparent);
    }
    private static string N(float x) => x.ToString("0.###", CultureInfo.InvariantCulture);
    private static Pen Pen(string color, float width) => new Pen(ColorTranslator.FromHtml(color), width)
    { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
    internal void Rect(float x, float y, float w, float h, string fill, string stroke = "none", float sw = 1)
    {
        svg.AppendLine($"<rect x=\"{N(x)}\" y=\"{N(y)}\" width=\"{N(w)}\" height=\"{N(h)}\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"{N(sw)}\"/>");
        if (fill != "none") { using var b = new SolidBrush(ColorTranslator.FromHtml(fill)); graphics.FillRectangle(b, x, y, w, h); }
        if (stroke != "none") { using var p = Pen(stroke, sw); graphics.DrawRectangle(p, x, y, w, h); }
    }
    internal void Circle(float x, float y, float r, string fill, string stroke = "none", float sw = 1)
    {
        svg.AppendLine($"<circle cx=\"{N(x)}\" cy=\"{N(y)}\" r=\"{N(r)}\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"{N(sw)}\"/>");
        if (fill != "none") { using var b = new SolidBrush(ColorTranslator.FromHtml(fill)); graphics.FillEllipse(b, x - r, y - r, r * 2, r * 2); }
        if (stroke != "none") { using var p = Pen(stroke, sw); graphics.DrawEllipse(p, x - r, y - r, r * 2, r * 2); }
    }
    internal void Line(float x1, float y1, float x2, float y2, string color, float sw)
    {
        svg.AppendLine($"<path d=\"M{N(x1)} {N(y1)}L{N(x2)} {N(y2)}\" stroke=\"{color}\" stroke-width=\"{N(sw)}\"/>");
        using var p = Pen(color, sw); graphics.DrawLine(p, x1, y1, x2, y2);
    }
    internal void Poly((float x, float y)[] points, string fill, string stroke, float sw, bool close = true)
    {
        string d = string.Join("L", points.Select(p => N(p.x) + " " + N(p.y)));
        svg.AppendLine($"<path d=\"M{d}{(close ? "Z" : "")}\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"{N(sw)}\"/>");
        var pts = points.Select(p => new PointF(p.x, p.y)).ToArray();
        if (fill != "none") { using var b = new SolidBrush(ColorTranslator.FromHtml(fill)); graphics.FillPolygon(b, pts); }
        if (stroke != "none") { using var p = Pen(stroke, sw); if (close) graphics.DrawPolygon(p, pts); else graphics.DrawLines(p, pts); }
    }
    internal void Text(string text, float x, float y, float size, string color)
    {
        svg.AppendLine($"<text x=\"{N(x)}\" y=\"{N(y + size * .82f)}\" font-family=\"Segoe UI, sans-serif\" font-size=\"{N(size)}\" font-weight=\"600\" fill=\"{color}\">{text}</text>");
        using var font = new Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(ColorTranslator.FromHtml(color));
        graphics.DrawString(text, font, brush, x, y, StringFormat.GenericTypographic);
    }
    internal void Save(string path)
    {
        Bitmap.Save(path + ".png", ImageFormat.Png);
        File.WriteAllText(path + ".svg", svg.ToString() + "</svg>\n");
    }
    public void Dispose() { graphics.Dispose(); Bitmap.Dispose(); }
}
