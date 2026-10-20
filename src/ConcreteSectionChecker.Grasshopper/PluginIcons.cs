using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ConcreteSectionChecker.Grasshopper
{
    internal static class PluginIcons
    {
        private static readonly ConcurrentDictionary<string, Bitmap> Cache = new ConcurrentDictionary<string, Bitmap>();
        // Windows preserves whichever filename casing was first written; manifest lookup is case sensitive.
        private static readonly Dictionary<string, string> ResourceNames = typeof(PluginIcons).Assembly.GetManifestResourceNames()
            .ToDictionary(name => name, name => name, StringComparer.OrdinalIgnoreCase);
        internal static Bitmap Get(string name) => Cache.GetOrAdd(name, key =>
        {
            string resource = "ConcreteSectionChecker.Grasshopper.Resources.Icons." + key + ".png";
            if (!ResourceNames.TryGetValue(resource, out string actualName))
                throw new InvalidOperationException("Missing plugin icon: " + resource);
            using var stream = typeof(PluginIcons).Assembly.GetManifestResourceStream(actualName)
                ?? throw new InvalidOperationException("Missing plugin icon: " + resource);
            using var bitmap = new Bitmap(stream);
            return new Bitmap(bitmap);
        });
    }
}
