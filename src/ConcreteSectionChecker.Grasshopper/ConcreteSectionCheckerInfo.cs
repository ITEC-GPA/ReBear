using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper
{
    public sealed class ConcreteSectionCheckerInfo : GH_AssemblyInfo
    {
        public override string Name => "ReBear";
        public override Bitmap Icon => PluginIcons.Get("Plugin");
        public override string Description => "Verifica delle sezioni in calcestruzzo armato mediante le librerie GPC.";
        // This identifies the new assembly. The 32 historical ComponentGuid values are unchanged.
        public override Guid Id => new Guid("7ca34ed5-c328-465a-9940-b4c4f3d924b3");
        public override string AuthorName => "GPC";
        public override string AuthorContact => "";
    }
}
