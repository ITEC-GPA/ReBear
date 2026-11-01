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
            // The GPC libraries are checked against the release set by GpcReleaseGuard.
            Gpc.PluginChecks.PluginBuildCheck.Start(typeof(ReBearAssemblyPriority).Assembly, "ReBear", sharedLibraries: false);
            return GH_LoadingInstruction.Proceed;
        }
    }

    /// <summary>
    /// Load-time check of the GPC libraries of ReBear. In a checkout, the GPC DLLs next to ReBear.gha are compared with
    /// lib/GPC (a bin not rebuilt after lib/GPC changed keeps the old engine); the GPC assemblies in the process are compared
    /// with release-set.json (another plugin loading a different GPC set first silently changes the engine). Mismatches are
    /// written to the Rhino command line and shown as a warning on every ReBear component. Assemblies loaded later are
    /// checked when they load.
    /// </summary>
    public static class GpcReleaseGuard
    {
        private static readonly object Sync = new();
        private static ReleaseSetCheck.Entry[] manifest;
        private static string stale, foreign;
        private static bool started;

        public static string Warning
        {
            get { lock (Sync) return stale == null && foreign == null ? null : string.Join(" ", new[] { stale, foreign }.Where(m => m != null)); }
        }

        public static void Start()
        {
            lock (Sync) { if (started) return; started = true; }
            try
            {
                string folder = Path.GetDirectoryName(typeof(GpcReleaseGuard).Assembly.Location);
                if (!string.IsNullOrEmpty(folder)) CheckBin(folder);
                string path = string.IsNullOrEmpty(folder) ? null : Path.Combine(folder, "release-set.json");
                if (path == null || !File.Exists(path))
                {
                    // Not a mismatch: say it once on the command line, without a warning on every component.
                    Write("release-set.json non trovato accanto a ReBear.gha; set GPC caricato non verificato.");
                    return;
                }
                manifest = ReleaseSetCheck.Manifest(File.ReadAllText(path)).ToArray();
                AppDomain.CurrentDomain.AssemblyLoad += (_, e) => { if (ReleaseSetCheck.IsGpc(e.LoadedAssembly.GetName().Name)) Check(); };
                Check();
            }
            catch (Exception e) { Report("Verifica del set GPC non riuscita: " + e.Message); }
        }

        /// <summary>The GPC DLLs next to ReBear.gha against lib/GPC of the checkout, if the plugin runs from one.</summary>
        private static void CheckBin(string folder)
        {
            try
            {
                string repository = ReleaseSetCheck.RepositoryManifest(folder);
                string message = repository == null ? null : ReleaseSetCheck.StaleBin(folder, ReleaseSetCheck.Manifest(File.ReadAllText(repository)));
                if (message == null) return;
                lock (Sync) stale = message;
                Write(message);
            }
            // Not a mismatch either: the loaded set is still checked below.
            catch (Exception e) { Write("Confronto del bin con lib/GPC non riuscito: " + e.Message); }
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
            lock (Sync) { if (foreign == message) return; foreign = message; }
            Write(message);
        }

        private static void Write(string message)
        {
            try { Rhino.RhinoApp.WriteLine("ReBear: " + message); } catch (Exception) { }
        }
    }
}
