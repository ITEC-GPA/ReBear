using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.RegularExpressions;
using Grasshopper.Kernel;

internal static class GrasshopperSmoke
{
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
        foreach (var type in types)
        {
            var component = (GH_Component)Activator.CreateInstance(type);
            if (type.Namespace == "ConcreteSectionChecker.Grasshopper.Features")
            {
                var entry = newManifest.RootElement.GetProperty("components").EnumerateArray().Single(c => c.GetProperty("type").GetString() == type.Name);
                Program.Check(component.ComponentGuid == entry.GetProperty("guid").GetGuid(), type.Name + " GUID");
                Program.Check(component.Category == "ReBear" && component.SubCategory == entry.GetProperty("category").GetString(), type.Name + " category");
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
            Program.Check(component.Category == "ReBear" && component.SubCategory == "Concrete Checker", type.Name + " palette");
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
