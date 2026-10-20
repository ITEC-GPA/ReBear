using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper
{
    /// <summary>Translate expected GPC input/solver failures into visible Grasshopper errors.</summary>
    public abstract class GpcComponent : GH_Component
    {
        protected GpcComponent(string name, string nickname, string description)
            : base(name, nickname, description, ComponentPalette.Category, string.Empty)
        {
            SubCategory = ComponentPalette.For(GetType()).SubCategory;
        }

        public sealed override GH_Exposure Exposure => ComponentPalette.For(GetType()).Exposure;

        protected override Bitmap Icon => PluginIcons.Get(GetType().Name);

        protected sealed override void SolveInstance(IGH_DataAccess data)
        {
            try { SolveGpc(data); }
            catch (ArgumentException e) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message); }
            catch (InvalidOperationException e) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message); }
            catch (NotSupportedException e) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message); }
            catch (ArithmeticException e) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message); }
            catch (System.IO.IOException e) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message); }
            catch (UnauthorizedAccessException e) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message); }
        }

        protected abstract void SolveGpc(IGH_DataAccess data);
    }
}
