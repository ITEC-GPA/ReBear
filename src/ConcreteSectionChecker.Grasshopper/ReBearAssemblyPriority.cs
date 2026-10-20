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
            return GH_LoadingInstruction.Proceed;
        }
    }
}
