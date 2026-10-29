using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace ConcreteSectionChecker.Adapters
{
    /// <summary>A GPC assembly loaded in the process that differs from the release set ReBear was built and tested with.</summary>
    public sealed record ReleaseSetMismatch(string Assembly, string ExpectedVersion, string LoadedVersion, string LoadedPath, string Reason)
    {
        public override string ToString() => $"{Assembly}: {Reason} (atteso {ExpectedVersion}, caricato {LoadedVersion ?? "—"} da {LoadedPath ?? "posizione sconosciuta"})";
    }

    /// <summary>
    /// Compares the GPC assemblies loaded in the process with lib/GPC/release-set.json (shipped next to ReBear.gha).
    /// Plugins sharing a Rhino process share the first loaded copy of each assembly: a different set loaded by another
    /// plugin changes the engine of ReBear without any visible sign.
    /// </summary>
    public static class ReleaseSetCheck
    {
        public sealed record Entry(string File, string Version, string Sha256);

        public static IReadOnlyList<Entry> Manifest(string json)
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("files").EnumerateArray().Select(e => new Entry(e.GetProperty("file").GetString(),
                e.GetProperty("assemblyVersion").GetString(), e.GetProperty("sha256").GetString())).ToArray();
        }

        /// <summary>Mismatches of the loaded assemblies listed in the manifest (assemblies not loaded yet are ignored).</summary>
        public static IReadOnlyList<ReleaseSetMismatch> Compare(IEnumerable<Entry> manifest, IEnumerable<(string Name, string Version, string Path)> loaded,
            Func<string, string> hash = null)
        {
            hash ??= Sha256;
            var result = new List<ReleaseSetMismatch>();
            var assemblies = loaded.ToArray();
            foreach (var entry in manifest.Where(e => e.File.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
            {
                string name = Path.GetFileNameWithoutExtension(entry.File);
                foreach (var copy in assemblies.Where(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (copy.Version != entry.Version)
                    {
                        result.Add(new(name, entry.Version, copy.Version, copy.Path, "versione diversa dal set di rilascio"));
                        continue;
                    }
                    string actual = null;
                    try { if (!string.IsNullOrEmpty(copy.Path)) actual = hash(copy.Path); }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
                    if (actual == null)
                        result.Add(new(name, entry.Version, copy.Version, copy.Path, "file non verificabile (hash non disponibile)"));
                    else if (!string.Equals(actual, entry.Sha256, StringComparison.OrdinalIgnoreCase))
                        result.Add(new(name, entry.Version, copy.Version, copy.Path, "contenuto diverso dal set di rilascio (SHA-256)"));
                }
            }
            return result;
        }

        public static IReadOnlyList<ReleaseSetMismatch> CompareLoaded(string manifestPath) => Compare(Manifest(File.ReadAllText(manifestPath)),
            AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(Describe));

        public static (string Name, string Version, string Path) Describe(Assembly assembly)
        {
            var name = assembly.GetName();
            string location = null;
            try { location = assembly.Location; } catch (NotSupportedException) { }
            return (name.Name, name.Version?.ToString(), location);
        }

        public static string Sha256(string path)
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(SHA256.HashData(stream));
        }
    }
}
