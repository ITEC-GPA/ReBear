using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ServiceabilityCrackWidthControlCheckComponent : GpcComponent
    {
        protected Dictionary<int, string> _serviceabilityLimitState = new Dictionary<int, string>() {
            { 0, "Quasi-Permanent" },
            { 1, "Frequent" },
            { 2, "Characteristic" },
        };

        protected Dictionary<int, string> _classeEsposizione = new Dictionary<int, string>() {
            { 0, "X0" },
            { 1, "XC1" },
            { 2, "XC2" },
            { 3, "XC3" },
            { 4, "XF1" },
            { 5, "XC4" },
            { 6, "XD1" },
            { 7, "XS1" },
            { 8, "XA1" },
            { 9, "XA2" },
            { 10, "XF2" },
            { 11, "XF3" },
            { 12, "XD2" },
            { 13, "XD3" },
            { 14, "XS2" },
            { 15, "XS3" },
            { 16, "XA3" },
            { 17, "XF4" },
        };

        protected Dictionary<int, string> _tipoArmatura = new Dictionary<int, string>() {
            { 0, "Sensibile" },
            { 1, "Poco Sensibile" },
        };

        // Keep the historical registration shape. Numerical coefficients are resolved exclusively by GPC.
        protected Dictionary<int, (string name, double unused)> _tipoDiCarico = new Dictionary<int, (string, double)>() {
            { 0, ("Breve durata", double.NaN) },
            { 1, ("Lunga durata", double.NaN) },
        };

        protected Dictionary<int, (string name, double unused)> _tipoBarra = new Dictionary<int, (string, double)>() {
            { 0, ("Aderenza migliorata", double.NaN) },
            { 1, ("Lisce", double.NaN) },
        };

        public ServiceabilityCrackWidthControlCheckComponent()
            : base("Serviceability Crack Width Check", "SCWC", "Serviceability Crack Width Check")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis Results", "SA", "Stress Analysis Results", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ricoprimento dell'armatura", "C", "Ricoprimento dell'armatura", GH_ParamAccess.item);
            int i = pManager.AddIntegerParameter("Serviceability Limit State", "SLS", "Serviceability Limit State", GH_ParamAccess.item);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _serviceabilityLimitState)
                mtParam.AddNamedValue(v.Value, v.Key);
            i = pManager.AddIntegerParameter("Classe di Esposizione", "CE", "Classe di Esposizione", GH_ParamAccess.item);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _classeEsposizione)
                mtParam.AddNamedValue(v.Value, v.Key);
            i = pManager.AddIntegerParameter("Tipo di Armatura", "TA", "Tipo di Armatura", GH_ParamAccess.item, 1);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _tipoArmatura)
                mtParam.AddNamedValue(v.Value, v.Key);
            i = pManager.AddIntegerParameter("Tipo di barra", "TB", "Tipo di Barra", GH_ParamAccess.item, 0);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, (string, double)> v in _tipoBarra)
                mtParam.AddNamedValue(v.Value.Item1, v.Key);
            i = pManager.AddIntegerParameter("Tipo di Carico", "TA", "Tipo di Carico", GH_ParamAccess.item, 1);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, (string, double)> v in _tipoDiCarico)
                mtParam.AddNamedValue(v.Value.Item1, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Crack width", "CW", "Crack width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Crack width limit", "CWL", "Crack width limit", GH_ParamAccess.item);
            pManager.AddTextParameter("Crack width limit", "CWL", "Crack width limit", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio", "WR", "Working Ratio", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_StressAnalysisResult stress = null;
            double cover = 0;
            int combination = 0, exposure = 0, sensitivity = 0, barType = 0, duration = 0;
            if (!DA.GetData(0, ref stress) || !DA.GetData(1, ref cover) || !DA.GetData(2, ref combination)
                || !DA.GetData(3, ref exposure) || !DA.GetData(4, ref sensitivity) || !DA.GetData(5, ref barType)
                || !DA.GetData(6, ref duration)) return;
            var r = ConcreteSectionChecker.Adapters.ServiceabilityChecks.Cracking(stress.Value, cover,
                combination, exposure, sensitivity, barType, duration);
            DA.SetData(0, "Norma;Riferimento;Criterio;Stato;wk[mm];wlim[mm];WR;Ac,eff[mm2];As,eff[mm2];Regione;Dettagli GPC");
            DA.SetData(1, GpcReports.Row(r.Profile, r.Reference, r.Requirement?.Criterion, r.Status, r.Width,
                r.Limit, r.Ratio, r.EffectiveArea, r.EffectiveSteel, r.GoverningRegion,
                string.Join(" | ", r.Details.Select(d => GpcReports.Row(d.Symbol, d.Value, d.Unit, d.Expression)))));
            DA.SetData(2, GpcReports.Number(r.Width));
            DA.SetData(3, GpcReports.Number(r.Limit));
            DA.SetData(4, r.Requirement?.Criterion + ": " + r.Status);
            DA.SetData(5, GpcReports.Number(r.Ratio));
            if (r.Verdict != GPC.Checkers.Concrete.Cracking.CrackVerdict.Satisfied)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, r.Status);
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("22bb7ea8-8721-42bf-b016-67c4379f9ca3");

    }
}
