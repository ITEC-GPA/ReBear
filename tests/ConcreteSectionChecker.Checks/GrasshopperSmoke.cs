using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using Grasshopper.Kernel;

internal static class GrasshopperSmoke
{
    public static void WritePalette()
    {
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Program.RepositoryRoot, "bin/ReBear.gha"));
        var components = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(GH_Component).IsAssignableFrom(t))
            .Select(t => (GH_Component)Activator.CreateInstance(t)).OrderBy(c => c.SubCategory)
            .ThenBy(c => (int)c.Exposure).ThenBy(c => c.Name).ToArray();
        var text = new StringBuilder("# Palette ReBear — ordine di utilizzo\n\n");
        text.AppendLine("Tutti i 61 componenti, inclusi i 32 recuperati da Rhino2FEM, condividono lo stesso percorso nella scheda **ReBear**. I prefissi numerici ordinano i pannelli; le sezioni native di Grasshopper (`GH_Exposure`) ordinano le fasi all'interno dei pannelli. I componenti nella stessa fase sono strumenti alternativi o affini e mantengono l'ordinamento nativo di Grasshopper.\n");
        text.AppendLine("**Materials → Sections → Input → Setup → Analysis → Checks → Detailing → Results → Display**\n");
        text.AppendLine("Lo Standard è accanto ai materiali perché serve per i loro valori di progetto. Coordinate, azioni e inviluppi delle azioni sono in Input; opzioni e costruzione del checker sono in Setup. I risultati di verifica e i relativi inviluppi sono in Results. I componenti storici non sono nascosti e conservano nomi, GUID e porte.\n");
        foreach (var panel in components.GroupBy(c => c.SubCategory))
        {
            text.AppendLine($"## {panel.Key} — {panel.Count()} componenti\n");
            text.AppendLine("| Fase | Componente | Origine | Porte I/O |\n|---:|---|---|---:|");
            int stage = 0;
            foreach (var group in panel.GroupBy(c => c.Exposure))
            {
                stage++;
                foreach (var c in group)
                    text.AppendLine($"| {stage} | {c.Name} | {(c.GetType().Namespace == "ConcreteSectionChecker.Grasshopper.Features" ? "Nuovo" : "Storico")} | {c.Params.Input.Count}/{c.Params.Output.Count} |");
            }
            text.AppendLine();
        }
        text.AppendLine("La disposizione è definita una sola volta in `src/ConcreteSectionChecker.Grasshopper/ComponentPalette.cs`. Per rigenerare questa pagina e l'indice dei nuovi componenti dopo una modifica, compilare la Release ed eseguire:\n\n```powershell\ndotnet run --project tests/ConcreteSectionChecker.Checks -c Release --no-build -- --write-palette\n```");
        File.WriteAllText(Path.Combine(Program.RepositoryRoot, "docs/PALETTE.md"), text.ToString().TrimEnd() + Environment.NewLine);

        string path = Path.Combine(Program.RepositoryRoot, "docs/NEW_COMPONENTS.md");
        string previous = File.ReadAllText(path);
        var index = new StringBuilder("# ReBear 0.2 — nuovi componenti\n\n29 componenti aggiuntivi, integrati con quelli storici nella [palette comune](PALETTE.md). GUID e porte sono fissati in `tests/Compatibility/components-0.2.json`; il campo category di quella fotografia documenta il raggruppamento al rilascio iniziale, aggiornato successivamente nella palette.\n");
        foreach (var panel in components.Where(c => c.GetType().Namespace == "ConcreteSectionChecker.Grasshopper.Features").GroupBy(c => c.SubCategory))
        {
            index.AppendLine($"\n## {panel.Key}\n\n| Componente | Funzione | GUID |\n|---|---|---|");
            foreach (var c in panel) index.AppendLine($"| {c.Name} | {c.Description} | `{c.ComponentGuid}` |");
        }
        index.AppendLine();
        index.Append(previous[previous.IndexOf("## Porte e unità", StringComparison.Ordinal)..].TrimEnd());
        File.WriteAllText(path, index.ToString() + Environment.NewLine);
    }
    public static void WriteCatalog()
    {
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Program.RepositoryRoot, "bin/ReBear.gha"));
        var components = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(GH_Component).IsAssignableFrom(t)
            && t.Namespace == "ConcreteSectionChecker.Grasshopper.Features").OrderBy(t => t.Name)
            .Select(t => (GH_Component)Activator.CreateInstance(t)).Select(c => new
            {
                type = c.GetType().Name, guid = c.ComponentGuid, name = c.Name, category = c.SubCategory, description = c.Description,
                inputs = c.Params.Input.Select(p => new { name = p.Name, access = p.Access.ToString(), optional = p.Optional, description = p.Description }),
                outputs = c.Params.Output.Select(p => new { name = p.Name, access = p.Access.ToString(), description = p.Description })
            }).ToArray();
        File.WriteAllText(Path.Combine(Program.RepositoryRoot, "tests/Compatibility/components-0.2.json"),
            JsonSerializer.Serialize(new { version = "0.2.0", components }, new JsonSerializerOptions { WriteIndented = true }));
    }
    public static void Run()
    {
        string folder = Path.Combine(Program.RepositoryRoot, "bin");
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(folder, "ReBear.gha"));
        var types = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(GH_Component).IsAssignableFrom(t)).ToArray();
        Program.Check(types.Length == 61, "Expected 61 loadable components.");
        Program.Check(types.Select(t => ((GH_Component)Activator.CreateInstance(t)).ComponentGuid).Distinct().Count() == 61, "Unique GUIDs.");
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Program.RepositoryRoot, "tests/Compatibility/components-66555b.json")));
        using var newManifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Program.RepositoryRoot, "tests/Compatibility/components-0.2.json")));
        string[] panels = { "1 Materials", "2 Sections", "3 Input", "4 Setup", "5 Analysis", "6 Checks", "7 Detailing", "8 Results", "9 Display" };
        foreach (var type in types)
        {
            var component = (GH_Component)Activator.CreateInstance(type);
            Program.Check(component.Category == "ReBear" && panels.Contains(component.SubCategory), type.Name + " workflow panel");
            Program.Check(component.Exposure != GH_Exposure.hidden && !component.Exposure.HasFlag(GH_Exposure.obscure),
                type.Name + " must remain visible on the ribbon");
            if (type.Namespace == "ConcreteSectionChecker.Grasshopper.Features")
            {
                var entry = newManifest.RootElement.GetProperty("components").EnumerateArray().Single(c => c.GetProperty("type").GetString() == type.Name);
                Program.Check(component.ComponentGuid == entry.GetProperty("guid").GetGuid(), type.Name + " GUID");
                // The released port/GUID snapshot is immutable; palette placement is independent.
                foreach (var direction in new[] { ("inputs", component.Params.Input), ("outputs", component.Params.Output) })
                {
                    var ports = entry.GetProperty(direction.Item1).EnumerateArray().ToArray();
                    Program.Check(ports.Length == direction.Item2.Count, type.Name + " port count");
                    for (int i = 0; i < ports.Length; i++)
                        Program.Check(direction.Item2[i].Name == ports[i].GetProperty("name").GetString()
                            && direction.Item2[i].Access.ToString() == ports[i].GetProperty("access").GetString(), type.Name + " port contract");
                }
            }
            else
            {
                var baseline = manifest.RootElement.GetProperty("components").EnumerateArray()
                .Single(c => Path.GetFileNameWithoutExtension(c.GetProperty("file").GetString()) == type.Name);
            Program.Check(component.ComponentGuid == Guid.Parse(baseline.GetProperty("guid").GetString()), type.Name + " runtime GUID");
            Program.Check(component.Params.Input.Count == CountPorts(baseline.GetProperty("inputRegistration").GetString()), type.Name + " inputs");
            Program.Check(component.Params.Output.Count == CountPorts(baseline.GetProperty("outputRegistration").GetString()), type.Name + " outputs");
            }
            var icon = (System.Drawing.Bitmap)assembly.GetType("ConcreteSectionChecker.Grasshopper.GpcComponent")
                .GetProperty("Icon", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(component);
            Program.Check(icon.Width == 24 && icon.Height == 24, type.Name + " icon");
        }
        foreach (var type in assembly.GetTypes().Where(t => t.Namespace?.EndsWith(".Datatypes") == true))
        {
            var goo = (Grasshopper.Kernel.Types.IGH_Goo)Activator.CreateInstance(type);
            Program.Check(!goo.IsValid && goo.Duplicate() != null && goo.ToString() != null && goo.TypeDescription != null,
                type.Name + " empty/duplicate contract");
        }
    }

    private static int CountPorts(string registration) => Regex.Matches(registration, @"pManager\.Add\w+Parameter\(").Count;
}
