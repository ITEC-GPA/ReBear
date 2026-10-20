using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper
{
    public sealed class ReBearAssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            global::Grasshopper.Instances.ComponentServer.AddCategoryIcon(Constants.CATEGORY_CHECKS, PluginIcons.Get("Plugin"));
            global::Grasshopper.Instances.ComponentServer.AddCategoryShortName(Constants.CATEGORY_CHECKS, "ReBear");
            global::Grasshopper.Instances.ComponentServer.AddCategorySymbolName(Constants.CATEGORY_CHECKS, 'R');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
