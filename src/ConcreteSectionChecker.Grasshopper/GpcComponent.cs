using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper
{
    /// <summary>Translate GPC input/solver failures into visible Grasshopper errors, with context for unexpected ones.</summary>
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
            if (GpcReleaseGuard.Warning is string warning) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
            try { SolveGpc(data); }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or ArithmeticException
                or System.IO.IOException or UnauthorizedAccessException)
            { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message); }
            catch (OperationCanceledException) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Calcolo annullato."); }
            catch (Exception e)
            {
                // Unexpected failure: keep the solution alive and say where it happened, instead of a bare "Solution exception".
                var site = e.TargetSite == null ? "" : $" in {e.TargetSite.DeclaringType?.Name}.{e.TargetSite.Name}";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    $"{Name}: errore inatteso {e.GetType().Name}{site} (iterazione {data.Iteration}; ingressi: {Inputs(data)}): {e.Message}");
            }
        }

        /// <summary>Short description of the current inputs for error messages.</summary>
        private string Inputs(IGH_DataAccess data)
        {
            var parts = new List<string>();
            for (int i = 0; i < Params.Input.Count; i++)
            {
                string text;
                try
                {
                    if (Params.Input[i].Access == GH_ParamAccess.item)
                    {
                        object value = null;
                        text = data.GetData(i, ref value) && value != null ? Short(value.ToString()) : "vuoto";
                    }
                    else
                    {
                        var values = new List<object>();
                        text = data.GetDataList(i, values) ? values.Count + " elementi" : "vuoto";
                    }
                }
                catch (Exception) { text = "non leggibile"; }
                parts.Add(Params.Input[i].NickName + "=" + text);
            }
            return string.Join(", ", parts);
        }

        private static string Short(string text)
        {
            text = (text ?? "").Split('\n').FirstOrDefault()?.Trim() ?? "";
            return text.Length > 40 ? text[..40] + "…" : text;
        }

        protected abstract void SolveGpc(IGH_DataAccess data);
    }
}
