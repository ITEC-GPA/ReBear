using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ConcreteSectionChecker.Adapters;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Results;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

// Exercise the actual component methods and their port indexes, without starting Rhino.
internal static class ComponentBoundary
{
    private static Assembly Assembly => GrasshopperSmoke.Plugin;
    private static object Goo(string name, object model) => Activator.CreateInstance(
        Assembly.GetType("Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes.GH_" + name), model);
    private static object Data(object model) => Activator.CreateInstance(Assembly.GetType("ConcreteSectionChecker.Grasshopper.GH_ReBearData"), model);
    private static (GH_Component component, DataAccessProxy data) Solve(string name, params object[] inputs) => SolveAt(name, 0, inputs);
    private static (GH_Component component, DataAccessProxy data) SolveAt(string name, int iteration, params object[] inputs)
    {
        var type = Assembly.GetTypes().Single(t => t.Name == name + "Component" || t.Name == name);
        var component = (GH_Component)Activator.CreateInstance(type);
        var access = DispatchProxy.Create<IGH_DataAccess, DataAccessProxy>();
        var data = (DataAccessProxy)access;
        data.Inputs = inputs; data.Iteration = iteration;
        Assembly.GetType("ConcreteSectionChecker.Grasshopper.GpcComponent")
            .GetMethod("SolveInstance", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(component, new object[] { access });
        return (component, data);
    }
    private static object Unwrap(object goo) => goo?.GetType().GetProperty("Value")?.GetValue(goo) ?? goo;
    private static void Expected(double expected, object actual, string name)
    {
        if (double.IsNaN(expected)) Program.Check(actual is double d && double.IsNaN(d), name + " must be NaN");
        else Program.Close(expected, Convert.ToDouble(actual, CultureInfo.InvariantCulture), name);
    }
    private static string Errors(GH_Component c) => string.Join(" | ", c.RuntimeMessages(GH_RuntimeMessageLevel.Error));

    public static void Run()
    {
        var checker = Program.Checker();
        var checkGoo = Goo("SectionChecker", checker);
        var axes = Goo("CoordinateSystem", CoordinateSystem.Global);

        // Units and unnamed combinations (C8).
        var (forceComponent, forceData) = SolveAt("SectionForce", 2, "", -300d, 60d, 90d, 12d, 45d, 15d, axes, 1);
        var f = (ResultBeamForces)Unwrap(forceData.Outputs[0]);
        Expected(-300000, f.N, "N conversion"); Expected(60000, f.V1, "Vx conversion"); Expected(90000, f.V2, "Vy conversion");
        Expected(12e6, f.T, "T conversion"); Expected(45e6, f.M1, "Mx conversion"); Expected(15e6, f.M2, "My conversion");
        Program.Check(f.Name == "Caso 3" && forceComponent.RuntimeMessages(GH_RuntimeMessageLevel.Remark).Count == 1, "Unnamed force gets the iteration name");
        var (_, nmm) = Solve("SectionForce", "A", 1d, 2d, 3d, 4d, 5d, 6d, axes, 0);
        Program.Check(((ResultBeamForces)Unwrap(nmm.Outputs[0])).M2 == 6, "N/Nmm units");
        var (badUnits, _) = Solve("SectionForce", "A", 1d, 2d, 3d, 4d, 5d, 6d, axes, 2);
        Program.Check(Errors(badUnits).Contains("Units"), "Invalid units named in the error: " + Errors(badUnits));

        // C6: validated enumerations and θ discretization.
        var (options, optionData) = Solve("CheckerOptions", axes, 1, 0, 0, 1, 1.5, 0d, false, 48);
        var o = (GPC.Checkers.Concrete.Checkers.SectionCheckerModelCode2010.SectionOptionsModelCode2010)Unwrap(optionData.Outputs[0]);
        Program.Check(o.TetaDiscretization == 48 && o.DomainPointStrategy == GPC.Checkers.Concrete.SectionSolvers.SectionSolver.DomainPointStrategyTypes.Iterative
            && o.FailureAnalysisType == GPC.Checkers.Concrete.SectionSolvers.SectionSolver.FailureAnalysisTypes.ConstantN, "Options mapped");
        foreach (var invalid in new[] { new object[] { axes, 2, 0, 0, 1, 1.5, 0d, false, 64 }, new object[] { axes, 1, 7, 0, 1, 1.5, 0d, false, 64 },
            new object[] { axes, 1, 0, 2, 1, 1.5, 0d, false, 64 }, new object[] { axes, 1, 0, 0, 1, 1.5, 0d, false, 4 } })
        {
            var (bad, badData) = Solve("CheckerOptions", invalid);
            Program.Check(badData.Outputs.Count == 0 && Errors(bad).Length > 0, "Invalid option rejected");
        }
        // C3: the checker owns a private copy of the options.
        var (_, built) = Solve("Checker", Goo("ReinforcedConcreteSection", checker.SectionCheckerAttribute.Section), Goo("Standard", checker.StandardModelCode2010), optionData.Outputs[0]);
        var builtChecker = (GPC.Checkers.Concrete.Checkers.SectionCheckerModelCode2010)Unwrap(built.Outputs[0]);
        o.PsiCoefficientRebar = 9;
        Program.Check(builtChecker.SectionCheckerOptions.PsiCoefficientRebar == 1.5, "Checker isolated from later option changes");

        // Shear through the components (replacing the 0.2 rectangular/circular components).
        var steel = Goo("SteelMaterial", SteelMaterialEN1992Data.B450C); var ntc = Goo("Standard", new GPC.Model.Standards.StandardNTC2018Concrete());
        var (_, rect) = Solve("RectangularShearOptions", ntc, steel, steel, 300d, 500d, 30d, 8d, 2, 4, 150d, 60d, 90d, 16d, 20d, 3, 4, 0d, 20d);
        var (_, shearY) = Solve("ShearCheck", checkGoo, Goo("ResultBeamForces", Program.Force()), rect.Outputs[1], 1, "S");
        var y = (CheckResult)Unwrap(shearY.Outputs[0]);
        Expected(90, y.Demand, "Shear Y demand [kN]");
        var direct = AdvancedShearChecks.Shear(checker, Program.Force(), (ShearOptions)Unwrap(rect.Outputs[1]), 1, "S");
        Expected(direct.Capacity.Value, y.Capacity, "Shear Y capacity [kN]");
        var (_, circ) = Solve("CircularShearOptions", ntc, steel, steel, 600d, 40d, 10d, 2, 150d, 90d, 20d, 0d, 20d);
        var (_, resultant) = Solve("ShearCheck", checkGoo, Goo("ResultBeamForces", Program.Force()), circ.Outputs[0], 2, "S");
        Expected(Math.Sqrt(60 * 60 + 90 * 90), ((CheckResult)Unwrap(resultant.Outputs[0])).Demand, "Resultant demand [kN]");
        var (tensionComponent, tension) = Solve("ShearCheck", checkGoo, Goo("ResultBeamForces", Program.Force(100000)),
            Data(ShearGeometry.Rectangular(new GPC.Model.Standards.StandardNTC2018Concrete(), SteelMaterialEN1992Data.B450C, SteelMaterialEN1992Data.B450C,
                300, 500, 30, 0, 0, 0, 0, 90, 90, 16, 20, 3, 4).Y), 1, "S");
        var pending = (CheckResult)Unwrap(tension.Outputs[0]);
        Program.Check(pending.Status == CheckStatus.NotEvaluated && pending.Ratio == null
            && tensionComponent.RuntimeMessages(GH_RuntimeMessageLevel.Warning).Count > 0, "Visible unevaluated warning");

        // C8 in single-check components: the iteration names the case.
        var unnamed = Goo("ResultBeamForces", Program.Force());
        var (_, first) = SolveAt("ResistanceCheck", 0, checkGoo, unnamed, "S");
        var (_, second) = SolveAt("ResistanceCheck", 1, checkGoo, unnamed, "S");
        Program.Check(((CheckResult)Unwrap(first.Outputs[0])).Case == "Caso 1" && ((CheckResult)Unwrap(second.Outputs[0])).Case == "Caso 2", "Unique case names");

        // SLE through Serviceability Check (replacing the 0.2 SLE components).
        var stress = checker.GetTensionAnalysisResult(Program.Force(-300000, 0, 0, 0, 0));
        var (_, frequentOptions) = Solve("SleOptions", 1, "XC4", false, false, true, 38d, null, null, null, false);
        var (_, frequent) = Solve("ServiceabilityCheck", Goo("StressAnalysisResult", stress), frequentOptions.Outputs[0], "S");
        var frequentResults = ((object[])frequent.Outputs[0]).Select(r => (CheckResult)Unwrap(r)).ToArray();
        Program.Check(frequentResults[0].Status == CheckStatus.NotRequired && frequentResults[0].Ratio == null, "Frequent: no invented stress limit");
        var (_, qpOptions) = Solve("SleOptions", 0, GPC.Checkers.Concrete.Cracking.CrackRequirements.ExposureClasses[12], true, false, true, 38d, null, null, null, false);
        var (_, decompression) = Solve("ServiceabilityCheck", Goo("StressAnalysisResult", stress), qpOptions.Outputs[0], "S");
        var crack = ((object[])decompression.Outputs[0]).Select(r => (CheckResult)Unwrap(r)).Last();
        Program.Check(crack.Ratio == null && crack.Unit == "MPa" && crack.Message.Contains("ecompression"), "Decompression is not wk/0: " + crack.Message);

        // B1/B2: summary without To Excel strings; absent quantities are NaN.
        var analysis = checker.GetTensionAnalysisResult(Program.Force(-300000, 0, 0, 60e6, 0));
        var plane = analysis.CalculateStrainPlaneResult(true, 1.5, 0);
        var (_, summary) = Solve("PostProcessStrainResult", Goo("StrainPlaneResult", plane));
        Program.Check(summary.Outputs.Count == 3 + StressSummary.Header.Length - 2, "Summary ports");
        Program.Check(summary.Outputs.Values.OfType<string>().All(s => !s.Contains(';')), "No CSV string outputs");
        Expected(-300, summary.Outputs[3], "N [kN]");
        foreach (int port in new[] { 10, 11, 12, 13, 18, 19, 20, 21 }) Expected(double.NaN, summary.Outputs[port], "Absent tendon/steel quantity " + port);
        Program.Check(summary.Outputs.Values.OfType<double>().All(v => !double.IsInfinity(v)), "No infinite output");
        var (_, report) = Solve("CheckReport", (object)new[] { summary.Outputs[0], summary.Outputs[0] });
        Program.Check(((string)report.Outputs[0]).Split(Environment.NewLine).Length == 3 && ((string)report.Outputs[1]).Contains("ReBear.StressReport"), "One report path for stresses");
        var (mixed, _) = Solve("CheckReport", (object)new[] { summary.Outputs[0], shearY.Outputs[0] });
        Program.Check(Errors(mixed).Contains("separati"), "Mixed report rejected");

        // 3D domain post-processing: structured results instead of To Excel rows.
        var domain = Activator.CreateInstance(Assembly.GetType("Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes.GH_FailureDomain3DResult"),
            checker.GetFailureDomainResult(), checker);
        var (postComponent, post) = Solve("PostProcessFailureDomain3D", domain, new object[] { Goo("ResultBeamForces", Program.Force(-300e3, 0, 0, 80e6, 10e6)),
            Goo("ResultBeamForces", Program.Force(-1e10, 0, 0, 80e6, 0)) }, "S");
        Program.Check(post.Outputs.ContainsKey(0), "Domain post-processing: " + Errors(postComponent));
        var domainResults = ((object[])post.Outputs[0]).Select(r => (CheckResult)Unwrap(r)).ToArray();
        Program.Check(domainResults[0].Status == CheckStatus.Satisfied && domainResults[1].Status == CheckStatus.NotSatisfied, "Domain check results");
        Expected(domainResults[0].Ratio.Value, ((object[])post.Outputs[3])[0], "Utilization port");

        // B4 through the component.
        var tee = RegressionChecks.Tee();
        var (_, resistance) = Solve("FailureDomainSummary", Goo("ReinforcedConcreteSection", tee), ntc, 0d);
        Expected(ResistanceChecks.Summary(tee, new GPC.Model.Standards.StandardNTC2018Concrete(), 0).MxPlusPlastic, resistance.Outputs[0], "MxRd+ pl");
        Program.Check((double)resistance.Outputs[0] > 500 && (double)resistance.Outputs[2] < -400, "Global signs");

        // C7: cover component with actual bars unknown.
        var (_, cover) = Solve("ConcreteCover", new GPC.Model.Standards.StandardEN1992p11(), ConcreteMaterialEN1992Data.C30_37, new[] { "XC4" },
            20d, null, 60d, 50, 10d, 20d, false, false, false, false, false, false, 1, 1, null, "S");
        Program.Check(((CheckResult)Unwrap(cover.Outputs[0])).Status == CheckStatus.NotEvaluated, "Cover pending without actual bars");
        Expected(45, cover.Outputs[1], "Abrasion index 1 maps to +5 mm and prepared ground to 40 mm");

        // Batch (asynchronous component) runs synchronously without a document.
        var (_, batch) = Solve("BatchSectionChecks", checkGoo, new object[] { unnamed, Goo("ResultBeamForces", Program.Force(-200e3)) }, "S", true,
            null, rect.Outputs[1], null, null, true);
        var batchResults = ((object[])batch.Outputs[0]).Select(r => (CheckResult)Unwrap(r)).ToArray();
        Program.Check(batchResults.Length == 4 && batchResults.Select(r => r.Case).Distinct().Count() == 2 && ((string)batch.Outputs[1]).StartsWith("\"Section\""), "Batch outputs");
        var (_, idle) = Solve("BatchSectionChecks", checkGoo, new object[] { unnamed }, "S", true, null, null, null, null, false);
        Program.Check(idle.Outputs.Count == 0, "Run = false computes nothing");

        // C5: unexpected exceptions report the component, the iteration and the inputs.
        var broken = new GPC.Checkers.Concrete.Results.StressAnalysisResult(stress.ConcreteSection, stress.Force, null, stress.SectionSolver, stress.Standard, true, 0, 0);
        var (inspect, _) = Solve("ViewStressAnalysisResultMesh", Goo("StressAnalysisResult", broken), new object[] { new GH_Point(new Rhino.Geometry.Point3d(0, 0, 0)) });
        Program.Check(Errors(inspect).Contains("Inspect Strain Analysis Results: errore inatteso") && Errors(inspect).Contains("iterazione 0")
            && Errors(inspect).Contains("CP=1 elementi"), "Unexpected exception with context: " + Errors(inspect));

        // C4: a result internalized in a file is reported, not silently empty.
        var placeholder = Activator.CreateInstance(Assembly.GetType("Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes.GH_StressAnalysisResult"));
        var archive = new GH_IO.Serialization.GH_Archive(); archive.CreateNewRoot(false);
        ((GH_IO.GH_ISerializable)placeholder).Read(archive.GetRootNode);
        var (lost, _) = Solve("ServiceabilityCheck", placeholder, frequentOptions.Outputs[0], "S");
        Program.Check(Errors(lost).Contains("Ricollegare"), "Placeholder input explained: " + Errors(lost));
    }
}

public class DataAccessProxy : DispatchProxy
{
    public object[] Inputs { get; set; } = Array.Empty<object>();
    public int Iteration { get; set; }
    public Dictionary<int, object> Outputs { get; } = new Dictionary<int, object>();
    protected override object Invoke(MethodInfo method, object[] args)
    {
        switch (method.Name)
        {
            case "GetData":
                int index = (int)args[0];
                if (index >= Inputs.Length || Inputs[index] is null) return false;
                args[1] = Inputs[index]; return true;
            case "GetDataList":
                if ((int)args[0] >= Inputs.Length || Inputs[(int)args[0]] == null) return false;
                foreach (object value in (IEnumerable)Inputs[(int)args[0]]) ((IList)args[1]).Add(value);
                return true;
            case "SetData": Outputs[(int)args[0]] = args[1]; return true;
            case "SetDataList": Outputs[(int)args[0]] = ((IEnumerable)args[1]).Cast<object>().ToArray(); return true;
            case "get_Iteration": return Iteration;
            default: throw new NotSupportedException("Unexpected IGH_DataAccess call: " + method);
        }
    }
}
