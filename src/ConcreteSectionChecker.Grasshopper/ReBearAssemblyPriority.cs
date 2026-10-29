using System;
using System.IO;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper
{
    public sealed class ReBearAssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            global::Grasshopper.Instances.ComponentServer.AddCategoryIcon(ComponentPalette.Category, PluginIcons.Get("Plugin"));
            global::Grasshopper.Instances.ComponentServer.AddCategoryShortName(ComponentPalette.Category, "ReBear");
            global::Grasshopper.Instances.ComponentServer.AddCategorySymbolName(ComponentPalette.Category, 'R');
            GpcReleaseGuard.Start();
            return GH_LoadingInstruction.Proceed;
        }
    }

    /// <summary>
    /// Load-time check of the GPC assemblies in the process against release-set.json: another plugin loading a different GPC
    /// set first silently changes the engine. Mismatches are written to the Rhino command line and shown as a warning on
    /// every ReBear component. Assemblies loaded later are checked when they load.
    /// </summary>
    public static class GpcReleaseGuard
    {
        private static readonly object Sync = new();
        private static ReleaseSetCheck.Entry[] manifest;
        private static string warning;
        public static string Warning { get { lock (Sync) return warning; } }

        public static void Start()
        {
            try
            {
                string folder = Path.GetDirectoryName(typeof(GpcReleaseGuard).Assembly.Location);
                string path = string.IsNullOrEmpty(folder) ? null : Path.Combine(folder, "release-set.json");
                if (path == null || !File.Exists(path))
                {
                    // Not a mismatch: say it once on the command line, without a warning on every component.
                    try { Rhino.RhinoApp.WriteLine("ReBear: release-set.json non trovato accanto a ReBear.gha; set GPC caricato non verificato."); } catch (Exception) { }
                    return;
                }
                manifest = ReleaseSetCheck.Manifest(File.ReadAllText(path)).ToArray();
                AppDomain.CurrentDomain.AssemblyLoad += (_, e) => { if (e.LoadedAssembly.GetName().Name?.StartsWith("GPC", StringComparison.Ordinal) == true || e.LoadedAssembly.GetName().Name == "DelaunayMesh") Check(); };
                Check();
            }
            catch (Exception e) { Report("Verifica del set GPC non riuscita: " + e.Message); }
        }

        private static void Check()
        {
            if (manifest == null) return;
            var mismatches = ReleaseSetCheck.Compare(manifest, AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(ReleaseSetCheck.Describe));
            if (mismatches.Count > 0)
                Report("Librerie GPC diverse dal set di rilascio di ReBear (probabilmente caricate da un altro plugin): " + string.Join("; ", mismatches));
        }

        private static void Report(string message)
        {
            lock (Sync) { if (warning == message) return; warning = message; }
            try { Rhino.RhinoApp.WriteLine("ReBear: " + message); } catch (Exception) { }
        }
    }
}
