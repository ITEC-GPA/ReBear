using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using Grasshopper.Kernel;

internal static class GrasshopperSmoke
{
    internal static Assembly Plugin => AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Program.RepositoryRoot, "bin/ReBear.gha"));
    internal const string Version = "0.3.0";
    internal const int ComponentCount = 58;
    /// <summary>0.2.2 components replaced in 0.3 without a successor of the same GUID (see docs/MIGRATION.md).</summary>
    internal static readonly string[] Removed =
    {
        "fdb8a104-8595-4b8f-9d1a-1ac36a664488", "73aec799-492f-4978-9e12-48faf9853d10", "08625aa4-a62b-4d06-91fb-9c9ffcf076fc", // shear v1
        "4cdbe859-b9a3-42d1-87d4-42c24647fc68", "22bb7ea8-8721-42bf-b016-67c4379f9ca3", // SLE v1
        "a8098614-15d9-4789-bffe-ecb9e694b888", "939e852a-e1b4-4b5b-9454-4eb54be70394", // design values: numbers instead of text
        "411284c2-923a-4475-94a5-8405ece71ce0", "e0846aae-ae28-41dc-8773-b5e4232ba6df", // rebar diameter in mm, tendon ports
        "aba32c4e-70fa-4e76-bef9-bb02214a1279", "c9e7fd77-486c-416e-987b-cf6521e649fd", // properties: principal angle in degrees
        "c84a4593-64c3-44b1-b663-6482d43b820b", // section axes: global directions instead of (−X, −Y)
        "acd21b40-347d-487b-ad3d-c355e19b5315", // checker options: θ discretization port
        "96102691-a47f-4cb5-a411-db638040b01e", "3951b5ca-01a2-4f6c-8acb-1b8a80ed1e65", // To Excel outputs replaced by Check Report
        "98dc4c10-e882-41ed-ae13-208c325c0013" // concrete cover: specification/actual diameters and explicit EN/NTC modifiers
    };

    private static GH_Component[] Components() => Plugin.GetTypes().Where(t => !t.IsAbstract && typeof(GH_Component).IsAssignableFrom(t))
        .Select(t => (GH_Component)Activator.CreateInstance(t)).OrderBy(c => c.SubCategory).ThenBy(c => (int)c.Exposure).ThenBy(c => c.Name).ToArray();

    /// <summary>Complete port contract of the built palette: identity, placement and every port (kind, access, optional).</summary>
    internal static object Contract(string version)
    {
        object Port(IGH_Param p) => new { name = p.Name, nickname = p.NickName, kind = p.GetType().Name, access = p.Access.ToString(), optional = p.Optional };
        var components = Components().Select(c => new
        {
            type = c.GetType().Name, guid = c.ComponentGuid, name = c.Name, nickname = c.NickName,
            panel = c.SubCategory, exposure = c.Exposure.ToString(),
            inputs = c.Params.Input.Select(Port).ToArray(), outputs = c.Params.Output.Select(Port).ToArray()
        }).ToArray();
        return new { product = "ReBear", version, count = components.Length, components };
    }
    private static string ContractJson(string version) => JsonSerializer.Serialize(Contract(version), new JsonSerializerOptions
        { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }).Replace("\r\n", "\n") + "\n";
    public static void WriteContract(string path, string version) => File.WriteAllText(path, ContractJson(version));

    /// <summary>The built palette equals tests/Compatibility/palette-0.3.json (regenerate deliberately with --write-contract).</summary>
    public static void Snapshot()
    {
        string expected = File.ReadAllText(Path.Combine(Program.RepositoryRoot, "tests/Compatibility/palette-0.3.json")).Replace("\r\n", "\n");
        string actual = ContractJson(Version);
        if (expected == actual) return;
        var e = expected.Split('\n'); var a = actual.Split('\n');
        int line = Enumerable.Range(0, Math.Min(e.Length, a.Length)).FirstOrDefault(i => e[i] != a[i], Math.Min(e.Length, a.Length));
        throw new Exception($"Palette differs from palette-0.3.json at line {line + 1}: expected '{e.ElementAtOrDefault(line)?.Trim()}', actual '{a.ElementAtOrDefault(line)?.Trim()}'.");
    }

    /// <summary>A 0.2.2 GUID is reused only with the same ports (name, kind, access); otherwise Grasshopper would rewire moved ports silently.</summary>
    public static void GuidPolicy()
    {
        using var old = JsonDocument.Parse(File.ReadAllText(Path.Combine(Program.RepositoryRoot, "tests/Compatibility/palette-0.2.2.json")));
        var current = Components().ToDictionary(c => c.ComponentGuid);
        Program.Check(current.Count == ComponentCount, $"Expected {ComponentCount} components with unique GUIDs, found {current.Count}.");
        IEnumerable<string> Ports(JsonElement ports) => ports.EnumerateArray().Select(p => $"{p.GetProperty("name").GetString()}|{p.GetProperty("kind").GetString()}|{p.GetProperty("access").GetString()}");
        IEnumerable<string> Live(IEnumerable<IGH_Param> ports) => ports.Select(p => $"{p.Name}|{p.GetType().Name}|{p.Access}");
        int kept = 0;
        foreach (var entry in old.RootElement.GetProperty("components").EnumerateArray())
        {
            var guid = entry.GetProperty("guid").GetGuid();
            if (!current.TryGetValue(guid, out var component))
            {
                Program.Check(Removed.Contains(guid.ToString()), $"0.2.2 component {entry.GetProperty("name").GetString()} removed without being listed.");
                continue;
            }
            Program.Check(!Removed.Contains(guid.ToString()), guid + " listed as removed");
            Program.Check(Ports(entry.GetProperty("inputs")).SequenceEqual(Live(component.Params.Input))
                && Ports(entry.GetProperty("outputs")).SequenceEqual(Live(component.Params.Output)),
                $"{component.Name}: ports changed under the 0.2.2 GUID {guid}; assign a new ComponentGuid.");
            kept++;
        }
        Program.Check(kept + Removed.Length == old.RootElement.GetProperty("count").GetInt32(), "Every 0.2.2 component kept or listed as removed");
    }

    public static void Run()
    {
        var assembly = Plugin;
        string[] panels = { "1 Materials", "2 Sections", "3 Input", "4 Setup", "5 Analysis", "6 Checks", "7 Detailing", "8 Results", "9 Display" };
        var components = Components();
        Program.Check(components.Length == ComponentCount, $"Expected {ComponentCount} loadable components.");
        foreach (var component in components)
        {
            string type = component.GetType().Name;
            Program.Check(component.Category == "ReBear" && panels.Contains(component.SubCategory), type + " workflow panel");
            Program.Check(component.Exposure != GH_Exposure.hidden && !component.Exposure.HasFlag(GH_Exposure.obscure), type + " must remain visible on the ribbon");
            Program.Check(component.GetType().Namespace is "ConcreteSectionChecker.Grasshopper.Components" or "ConcreteSectionChecker.Grasshopper.Features", type + " namespace");
            Program.Check(assembly.GetType("ConcreteSectionChecker.Grasshopper.ReBearComponent").IsAssignableFrom(component.GetType()), type + " must use ReBearComponent");
            Program.Check(component.Params.Input.Concat(component.Params.Output).All(p => !string.IsNullOrWhiteSpace(p.Description)), type + " port descriptions");
            var icon = (System.Drawing.Bitmap)assembly.GetType("ConcreteSectionChecker.Grasshopper.GpcComponent")
                .GetProperty("Icon", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(component);
            Program.Check(icon.Width == 24 && icon.Height == 24, type + " icon");
            Program.Check(File.Exists(Path.Combine(Program.RepositoryRoot, "assets/icons", type + ".svg")), type + " SVG icon");
        }
        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(Grasshopper.Kernel.Types.IGH_Goo).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null))
        {
            var goo = (Grasshopper.Kernel.Types.IGH_Goo)Activator.CreateInstance(type);
            Program.Check(!goo.IsValid && goo.Duplicate() != null && goo.ToString() != null && goo.TypeDescription != null,
                type.Name + " empty/duplicate contract");
        }
    }

    /// <summary>Internalized data: inputs round-trip through GH archives, results leave an explicit placeholder message.</summary>
    public static void Persistence()
    {
        var assembly = Plugin;
        object Goo(string name, object value) => Activator.CreateInstance(assembly.GetType("Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes.GH_" + name), value);
        object RoundTrip(object goo)
        {
            var archive = new GH_IO.Serialization.GH_Archive();
            Program.Check(archive.AppendObject((GH_IO.GH_ISerializable)goo, "item"), "Write " + goo.GetType().Name);
            var copy = (GH_IO.GH_ISerializable)Activator.CreateInstance(goo.GetType());
            var restored = new GH_IO.Serialization.GH_Archive();
            Program.Check(restored.Deserialize_Xml(archive.Serialize_Xml()) && restored.ExtractObject(copy, "item"), "Read " + goo.GetType().Name);
            return copy;
        }
        object Value(object goo) => goo.GetType().GetProperty("Value").GetValue(goo);
        var checker = Program.Checker();
        var restoredChecker = RoundTrip(Goo("SectionChecker", checker));
        Program.Check(((Grasshopper.Kernel.Types.IGH_Goo)restoredChecker).IsValid, "Checker restored");
        var c = (GPC.Checkers.Concrete.Checkers.SectionCheckerModelCode2010)Value(restoredChecker);
        Program.Close(checker.SectionCheckerAttribute.Section.Area, c.SectionCheckerAttribute.Section.Area, "Restored section area");
        string text = restoredChecker.ToString();
        Program.Check(text.Contains("NTC") && text.Contains("Ac=150000") && text.Contains("Iterative"), "Meaningful checker description: " + text);
        foreach (var (name, value) in new (string, object)[] { ("CheckerOptions", checker.SectionCheckerOptionsModelCode2010), ("Standard", checker.StandardModelCode2010),
            ("ResultBeamForces", Program.Force()), ("CoordinateSystem", GPC.Geometry.CoordinateSystem.Global),
            ("ConcreteMaterial", GPC.Model.Data.Concrete.ConcreteMaterialEN1992Data.C30_37), ("SteelMaterial", GPC.Model.Data.Steel.SteelMaterialEN1992Data.B450C),
            ("ReinforcedConcreteSection", checker.SectionCheckerAttribute.Section), ("Rebar", checker.SectionCheckerAttribute.Section.Rebars.First()) })
        {
            var back = RoundTrip(Goo(name, value));
            Program.Check(((Grasshopper.Kernel.Types.IGH_Goo)back).IsValid && Value(back).GetType() == value.GetType(), name + " round trip");
            Program.Check(!string.IsNullOrWhiteSpace(back.ToString()) && !back.ToString().StartsWith("Empty"), name + " description");
        }
        var data = Activator.CreateInstance(assembly.GetType("ConcreteSectionChecker.Grasshopper.GH_ReBearData"), (object)FeatureChecks.ShearOptions());
        Program.Check(Value(RoundTrip(data)) is ConcreteSectionChecker.Adapters.ShearOptions, "Options record round trip");
        // C3: options are mutable; Duplicate must not share them.
        var options = Goo("CheckerOptions", checker.SectionCheckerOptionsModelCode2010);
        var duplicate = ((Grasshopper.Kernel.Types.IGH_Goo)options).Duplicate();
        Program.Check(!ReferenceEquals(Value(duplicate), Value(options)), "Options duplicate is a copy");
        var stress = RoundTrip(Goo("StressAnalysisResult", checker.GetTensionAnalysisResult(Program.Force())));
        Program.Check(!((Grasshopper.Kernel.Types.IGH_Goo)stress).IsValid && stress.ToString().Contains("non viene salvato"), "Result placeholder: " + stress);
        var old = (GH_IO.GH_ISerializable)Activator.CreateInstance(assembly.GetType("Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes.GH_SectionChecker"));
        var empty = new GH_IO.Serialization.GH_Archive(); empty.CreateNewRoot(false); old.Read(empty.GetRootNode);
        Program.Check(old.ToString().Contains("0.2"), "Data internalized by 0.2 reported: " + old);
    }

    public static void WritePalette()
    {
        var components = Components();
        var old = JsonDocument.Parse(File.ReadAllText(Path.Combine(Program.RepositoryRoot, "tests/Compatibility/palette-0.2.2.json")))
            .RootElement.GetProperty("components").EnumerateArray().Select(e => e.GetProperty("guid").GetGuid()).ToHashSet();
        var text = new StringBuilder("# Palette ReBear 0.3 — ordine di utilizzo\n\n");
        text.AppendLine($"Tutti i {components.Length} componenti condividono il modello `ReBearComponent` e lo stesso percorso nella scheda **ReBear**. I prefissi numerici ordinano i pannelli; le sezioni native di Grasshopper (`GH_Exposure`) ordinano le fasi all'interno dei pannelli. I componenti nella stessa fase sono strumenti alternativi o affini.\n");
        text.AppendLine("**Materials → Sections → Input → Setup → Analysis → Checks → Detailing → Results → Display**\n");
        text.AppendLine("La colonna GUID indica se il componente conserva il `ComponentGuid` di 0.2.2 (porte identiche) o ne ha uno nuovo (porte o significato cambiati, vedere [MIGRATION.md](MIGRATION.md)). La fotografia completa di nomi, porte e GUID è `tests/Compatibility/palette-0.3.json`.\n");
        foreach (var panel in components.GroupBy(c => c.SubCategory))
        {
            text.AppendLine($"## {panel.Key} — {panel.Count()} componenti\n");
            text.AppendLine("| Fase | Componente | GUID | Porte I/O |\n|---:|---|---|---:|");
            int stage = 0;
            foreach (var group in panel.GroupBy(c => c.Exposure))
            {
                stage++;
                foreach (var c in group)
                    text.AppendLine($"| {stage} | {c.Name} | {(old.Contains(c.ComponentGuid) ? "0.2.2" : "nuovo 0.3")} | {c.Params.Input.Count}/{c.Params.Output.Count} |");
            }
            text.AppendLine();
        }
        text.AppendLine("La disposizione è definita una sola volta in `src/ConcreteSectionChecker.Grasshopper/ComponentPalette.cs`. Per rigenerare questa pagina e il catalogo delle porte dopo una modifica, compilare la Release ed eseguire:\n\n```powershell\ndotnet obj/bin/ConcreteSectionChecker.Checks/Release/net8.0-windows/ConcreteSectionChecker.Checks.dll --write-palette\n```");
        File.WriteAllText(Path.Combine(Program.RepositoryRoot, "docs/PALETTE.md"), text.ToString().TrimEnd() + Environment.NewLine);

        var catalog = new StringBuilder("# ReBear 0.3 — catalogo delle porte\n\n");
        catalog.AppendLine($"{components.Length} componenti, generato dal plugin compilato (`--write-palette`). Unità: geometria mm, aree mm², tensioni MPa, forze delle porte kN e momenti kNm salvo diversa indicazione, compressione negativa. Un valore mancante è `NaN`, mai zero.\n");
        foreach (var panel in components.GroupBy(c => c.SubCategory))
        {
            catalog.AppendLine($"## {panel.Key}\n");
            foreach (var c in panel)
            {
                catalog.AppendLine($"### {c.Name}\n\n{c.Description}\n\n`{c.ComponentGuid}`{(old.Contains(c.ComponentGuid) ? "" : " (nuovo in 0.3)")}\n");
                catalog.AppendLine("| Porta | Accesso | Descrizione |\n|---|---|---|");
                for (int i = 0; i < c.Params.Input.Count; i++)
                {
                    var p = c.Params.Input[i];
                    catalog.AppendLine($"| inputs[{i}] {p.Name} | {p.Access}{(p.Optional ? ", opzionale" : "")} | {p.Description.Replace("|", "\\|")} |");
                }
                for (int i = 0; i < c.Params.Output.Count; i++)
                {
                    var p = c.Params.Output[i];
                    catalog.AppendLine($"| outputs[{i}] {p.Name} | {p.Access} | {p.Description.Replace("|", "\\|")} |");
                }
                catalog.AppendLine();
            }
        }
        File.WriteAllText(Path.Combine(Program.RepositoryRoot, "docs/COMPONENTS.md"), catalog.ToString().TrimEnd() + Environment.NewLine);
    }
}
