using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The GPC guard of ReBear.gha inside a real Rhino, started by RegressionChecks.StaleBin with a copy of the published bin whose
/// release-set.json does not match GPCChecker.Concrete. The library is loaded first by a worker while Rhino's thread waits for it
/// without pumping messages, as the runtime makes it wait when the background calculations of ReBear load a library on a worker:
/// the guard must report it from the worker without waiting for Rhino's thread. Prints "REBEAR GUARD LATE LOAD worker", or
/// "REBEAR GUARD LATE LOAD hang" after 120 s and ends the process, whose threads would otherwise never return (4/11/2026:
/// RhinoApp.WriteLine in AssemblyLoad, called from a worker, waited for Rhino's thread).
/// </summary>
internal static class Program
{
    private const string RhinoSystem = @"C:\Program Files\Rhino 8\System";

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            RhinoInside.Resolver.Initialize(RhinoSystem);
            return Run(args[0]);
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }

    // RhinoCommon types only after the resolver is initialized.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int Run(string folder)
    {
        using var rhino = new Rhino.Runtime.InProcess.RhinoCore(new[] { "/nosplash" }, Rhino.Runtime.InProcess.WindowStyle.Hidden);
        Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(RhinoSystem), "Plug-ins", "Grasshopper", "Grasshopper.dll"));
        var guard = Assembly.LoadFrom(Path.Combine(folder, "ReBear.gha")).GetType("ConcreteSectionChecker.Grasshopper.GpcReleaseGuard", true);
        guard.GetMethod("Start").Invoke(null, null);
        Console.WriteLine("REBEAR GUARD LATE LOAD " + LoadOnWorker(Path.Combine(folder, "GPCChecker.Concrete.dll")));
        Console.WriteLine("REBEAR GUARD WARNING: " + (guard.GetProperty("Warning").GetValue(null) ?? "none"));
        return 0;
    }

    private static string LoadOnWorker(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == name)) return "already loaded";
        var load = Task.Run(() => Assembly.LoadFrom(path));
        if (SpinWait.SpinUntil(() => load.IsCompleted, TimeSpan.FromSeconds(120))) { _ = load.Result; return "worker"; }
        Console.WriteLine("REBEAR GUARD LATE LOAD hang");
        Console.Out.Flush();
        Process.GetCurrentProcess().Kill();
        return "hang";
    }
}
