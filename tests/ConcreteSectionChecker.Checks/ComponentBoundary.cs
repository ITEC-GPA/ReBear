using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using ConcreteSectionChecker.Adapters;
using GPC.Geometry;
using GPC.Model.Results;
using Grasshopper.Kernel;

// Exercise the actual component methods and their port indexes, without starting Rhino.
internal static class ComponentBoundary
{
    private static Assembly Assembly => AssemblyLoadContext.Default.LoadFromAssemblyPath(
        Path.Combine(Program.RepositoryRoot, "bin/ReBear.gha"));
    private static object Goo(string name, object model) => Activator.CreateInstance(
        Assembly.GetType("Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes.GH_" + name), model);
    private static (GH_Component component, DataAccessProxy data) Solve(string name, params object[] inputs)
        => SolveType("Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components." + name + "Component", inputs);
    private static (GH_Component component, DataAccessProxy data) SolveType(string name, params object[] inputs)
    {
        var component = (GH_Component)Activator.CreateInstance(Assembly.GetType(name));
        var access = DispatchProxy.Create<IGH_DataAccess, DataAccessProxy>();
        var data = (DataAccessProxy)access;
        data.Inputs = inputs;
        Assembly.GetType("ConcreteSectionChecker.Grasshopper.GpcComponent")
            .GetMethod("SolveInstance", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(component, new object[] { access });
        return (component, data);
    }
    private static void Expected(double expected, object actual, string name)
    {
        if (double.IsNaN(expected)) Program.Check(actual is double d && double.IsNaN(d), name + " must be NaN");
        else Program.Close(expected, Convert.ToDouble(actual, CultureInfo.InvariantCulture), name);
    }
    public static void Run()
    {
        var checker = Program.Checker();
        var force = Program.Force();
        var checkGoo = Goo("SectionChecker", checker);
        var forceGoo = Goo("ResultBeamForces", force);
        var (_, rectangular) = Solve("RectangularSectionShearCheck", checkGoo, forceGoo,
            500d, 300d, 8d, 2, 4, 150d, 60d, 90d, 16d, 20d, 3, 4, 30d, 150000d);
        var r = ShearChecks.Rectangular(checker, force, 500, 300, 8, 2, 4, 150, 60, 90, 16, 20, 3, 4, 30, 150000);
        var values = new[] { r[0].VRsd, r[1].VRsd, r[0].VRcd, r[1].VRcd, r[0].VRd, r[1].VRd,
            r[2].VRsd, r[3].VRsd, r[2].VRcd, r[3].VRcd, r[2].VRd, r[3].VRd }.Select(x => x / 1000)
            .Concat(r.Select(x => x.Ratio.Value)).ToArray();
        Program.Check(rectangular.Outputs.Count == 18, "All rectangular outputs");
        for (int i = 0; i < values.Length; i++) Expected(values[i], rectangular.Outputs[i + 2], "Rectangular port " + (i + 2));

        var (_, circular) = Solve("CircularSectionShearCheck", checkGoo, forceGoo, 600d, 10d, 2, 150d, 90d, 20d, 40d);
        var c = ShearChecks.Circular(checker, force, 600, 10, 2, 150, 90, 20, 40);
        Expected(c.VRsd / 1000, circular.Outputs[2], "Circular VRsd [kN]");
        Expected(c.VRcd / 1000, circular.Outputs[3], "Circular VRcd [kN]");
        Expected(c.VRd / 1000, circular.Outputs[4], "Circular VRd [kN]");
        Expected(c.Ratio.Value, circular.Outputs[5], "Circular ratio");
        var (tensionComponent, tension) = Solve("RectangularSectionShearCheckNoStirrup", checkGoo,
            Goo("ResultBeamForces", Program.Force(100000)), 500d, 300d, 20d, 4d, 30d, 150000d);
        foreach (int port in new[] { 2, 3, 4, 5 }) Expected(double.NaN, tension.Outputs[port], "Axial tension " + port);
        Program.Check(tensionComponent.RuntimeMessages(GH_RuntimeMessageLevel.Warning).Count > 0, "Visible unevaluated warning");
        var (invalidComponent, invalid) = Solve("CircularSectionShearCheck", checkGoo, forceGoo, 600d, 10d, 2, 0d, 90d, 20d, 40d);
        Program.Check(invalid.Outputs.Count == 0 && invalidComponent.RuntimeMessages(GH_RuntimeMessageLevel.Error).Count > 0,
            "Invalid geometry must not yield a numeric result");

        var (_, forceData) = Solve("SectionForce", "Load", -300d, 60d, 90d, 12d, 45d, 15d, Goo("CoordinateSystem", CoordinateSystem.Global), 1);
        var forceResult = (ResultBeamForces)forceData.Outputs[0].GetType().GetProperty("Value").GetValue(forceData.Outputs[0]);
        Expected(-300000, forceResult.N, "N conversion");
        Expected(60000, forceResult.V1, "Vx conversion"); Expected(90000, forceResult.V2, "Vy conversion");
        Expected(12e6, forceResult.T, "T conversion"); Expected(45e6, forceResult.M1, "Mx conversion"); Expected(15e6, forceResult.M2, "My conversion");

        var stress = checker.GetTensionAnalysisResult(Program.Force(-300000, 0, 0, 0, 0));
        var (_, frequent) = Solve("ServiceabilityStressCheck", Goo("StressAnalysisResult", stress), 1);
        Program.Check(((object[])frequent.Outputs[3]).All(v => (double)v < 0), "Frequent: retain actual stresses");
        Program.Check(((object[])frequent.Outputs[4]).All(v => double.IsNaN((double)v)), "Frequent: no invented concrete limit");
        Program.Check(((object[])frequent.Outputs[7]).All(v => double.IsNaN((double)v)), "Frequent: no invented steel limit");
        var (_, decompression) = Solve("ServiceabilityCrackWidthControlCheck", Goo("StressAnalysisResult", stress), 30d, 0, 12, 0, 0, 1);
        Expected(double.NaN, decompression.Outputs[2], "Decompression width");
        Expected(double.NaN, decompression.Outputs[5], "Decompression ratio");
        Program.Check(decompression.Outputs[4].ToString().Contains("Decompression"), "Decompression criterion");
        NewFeatures();
    }
    private static void NewFeatures()
    {
        string ns = "ConcreteSectionChecker.Grasshopper.Features.";
        var checker = Program.Checker(); var force = Program.Force(); var options = FeatureChecks.ShearOptions();
        var (_, shear) = SolveType(ns + "ShearCheckComponent", Goo("SectionChecker", checker), Goo("ResultBeamForces", force),
            new Grasshopper.Kernel.Types.GH_ObjectWrapper(options), 1, "S");
        Program.Check(shear.Outputs.TryGetValue(0, out var wrapped), "New shear result output");
        var value = (CheckResult)wrapped.GetType().GetProperty("Value").GetValue(wrapped);
        Expected(90, value.Demand, "New shear demand [kN]");
        var archive = new GH_IO.Serialization.GH_Archive();
        Program.Check(archive.AppendObject((GH_IO.GH_ISerializable)wrapped, "result"), "Write typed result");
        var copy = (GH_IO.GH_ISerializable)Activator.CreateInstance(wrapped.GetType());
        var restored = new GH_IO.Serialization.GH_Archive();
        Program.Check(restored.Deserialize_Xml(archive.Serialize_Xml()) && restored.ExtractObject(copy, "result"), "Read typed result");
        Program.Check(value == (CheckResult)copy.GetType().GetProperty("Value").GetValue(copy), "Result round trip");
        var (_, envelope) = SolveType(ns + "CheckEnvelopeComponent", (object)new[] { wrapped,
            Activator.CreateInstance(wrapped.GetType(), value with { Status = CheckStatus.NotEvaluated, Ratio = null }) });
        Program.Check(((object[])envelope.Outputs[1]).Single().ToString() == "NotEvaluated", "New envelope pending");
        var (_, cover) = SolveType(ns + "ConcreteCoverComponent", new GPC.Model.Standards.StandardEN1992p11(),
            GPC.Model.Data.Concrete.ConcreteMaterialEN1992Data.C30_37, new[] { "XC4" }, 20d, 60d,
            50, 10d, 20d, false, false, false, false, 1, 1, null, "S");
        Expected(45, cover.Outputs[1], "Abrasion index 1 maps to +5 mm and prepared ground to 40 mm");
    }
}

public class DataAccessProxy : DispatchProxy
{
    public object[] Inputs { get; set; } = Array.Empty<object>();
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
            case "get_Iteration": return 0;
            default: throw new NotSupportedException("Unexpected IGH_DataAccess call: " + method);
        }
    }
}
