# -*- coding: utf-8 -*-
"""Run from Rhino's _-RunPythonScript. Evaluate the local acceptance copy without exporting."""
import os
import json
import traceback
import clr
import System
import Rhino
import scriptcontext as sc

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPORT = os.path.join(ROOT, "artifacts", "example-check")

def write_report(name, data):
    with open(os.path.join(REPORT, name), "w") as stream:
        json.dump(data, stream, indent=2, ensure_ascii=True)

try:
    write_report("host-progress.json", {"stage": "starting", "rhino": str(Rhino.RhinoApp.Version),
                                     "runtime": str(System.Environment.Version)})
    clr.AddReferenceToFileAndPath(r"C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Grasshopper.dll")
    clr.AddReferenceToFileAndPath(r"C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll")
    import Grasshopper
    from Grasshopper.Kernel import GH_Document, GH_ExternalFile, GH_RuntimeMessageLevel
    from Grasshopper.Kernel.Types import GH_Boolean, GH_String
    from GH_IO.Serialization import GH_Archive
    GH_Document.EnableSolutions = False
    write_report("host-progress.json", {"stage": "selecting-required-libraries"})
    server = Grasshopper.Instances.ComponentServer
    candidates = list(server.ExternalFiles(False, False))
    selected_paths = []
    for entry in candidates:
        path = entry.FilePath
        name = os.path.basename(path).lower()
        if (path.lower().startswith(r"c:\program files\rhino 8\plug-ins\grasshopper".lower())
            or name in ["heteroptera.gha", "treesloth.gha", "pufferfish.gha", "parakeet.gha",
                        "metahopper.gha", "elefront5.gha", "wombatgh.gha", "core.grasshopper.public.gha"]):
            selected_paths.append(path)
    write_report("host-libraries.json", {"selected": selected_paths,
                                      "candidates": [e.FilePath for e in candidates]})
    write_report("host-progress.json", {"stage": "grasshopper-initialized"})
    plugin_path = os.path.join(ROOT, "bin", "ReBear.gha")
    # Rhino 8 exposes single-file loading internally. Use the verified installed signature,
    # only for this temporary acceptance session; no persistent plugin-folder setting is changed.
    flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
    loader = server.GetType().GetMethod("LoadExternalFile", flags)
    loaded = loader.Invoke(server, System.Array[System.Object]([GH_ExternalFile(plugin_path), False]))
    write_report("host-progress.json", {"stage": "rebear-loaded", "loaded": bool(loaded)})
    archive = GH_Archive()
    example = os.path.join(REPORT, "template", "02_GH", "ReBear-Tipologico1-Test.ghx")
    if not archive.ReadFromFile(example):
        raise Exception("Cannot read example archive")
    document = GH_Document()
    extracted = archive.ExtractObject(document, "Definition")
    document.FilePath = example
    sc.sticky["ReBear.ExampleDocument"] = document
    Grasshopper.Instances.DocumentServer.AddDocument(document)
    # Keep exporters disabled while checking the engineering graph.
    exporters = []
    readers = []
    def set_persistent(param, value):
        param.RemoveAllSources()
        param.PersistentData.Clear()
        param.PersistentData.Append(value)
    for obj in document.Objects:
        if str(obj.ComponentGuid) == "fd47efda-72cb-4496-b9ec-bff81953c953":
            set_persistent(obj.Params.Input[0], GH_Boolean(False))
            obj.Locked = True
            exporters.append(str(obj.InstanceGuid))
        if str(obj.ComponentGuid) == "25f5b853-1e28-4de9-a4aa-d178b8496d93":
            set_persistent(obj.Params.Input[1], GH_String(os.path.join(REPORT, "template", "01_Sollecitazioni GH",
                "PXXX-XX-X-FMXX-Tipologico1-Pila INPUT GH.xlsx")))
            set_persistent(obj.Params.Input[3], GH_Boolean(False))
            readers.append(str(obj.InstanceGuid))
    objects = []
    for obj in document.Objects:
        objects.append({"name": obj.Name, "nickname": obj.NickName, "guid": str(obj.ComponentGuid),
                        "instance": str(obj.InstanceGuid), "type": obj.GetType().FullName,
                        "assembly": obj.GetType().Assembly.GetName().Name,
                        "locked": bool(obj.Locked) if hasattr(obj, "Locked") else None})
    assemblies = []
    for asm in System.AppDomain.CurrentDomain.GetAssemblies():
        name = asm.GetName().Name
        if name.startswith("GPC") or name in ["ReBear", "ConcreteSectionChecker.Adapters", "Grasshopper"]:
            assemblies.append({"name": name, "version": str(asm.GetName().Version), "path": asm.Location})
    write_report("host-load.json", {"extracted": bool(extracted), "count": len(objects), "objects": objects,
                                  "assemblies": assemblies, "exportersTemporarilyLocked": exporters,
                                  "readersRedirectedToLocalCopy": readers})
    gh = Rhino.RhinoApp.GetPlugInObject("Grasshopper")
    gh.ShowEditor()
    if Grasshopper.Instances.ActiveCanvas is not None:
        Grasshopper.Instances.ActiveCanvas.Document = document
    write_report("host-progress.json", {"stage": "solving", "count": len(objects)})
    GH_Document.EnableSolutions = True
    document.Enabled = True
    document.NewSolution(False)
    def describe_param(param):
        values = list(param.VolatileData.AllData(True))
        return {"name": param.Name, "count": len(values),
                "nullCount": sum(1 for v in values if v is None),
                "invalidCount": sum(1 for v in values if v is not None and not v.IsValid),
                "samples": [str(v)[:1400] for v in values[:3]]}
    results = []
    for obj in document.Objects:
        item = {"name": obj.Name, "nickname": obj.NickName, "guid": str(obj.ComponentGuid),
                "instance": str(obj.InstanceGuid), "assembly": obj.GetType().Assembly.GetName().Name}
        if hasattr(obj, "RuntimeMessages"):
            item["errors"] = list(obj.RuntimeMessages(GH_RuntimeMessageLevel.Error))
            item["warnings"] = list(obj.RuntimeMessages(GH_RuntimeMessageLevel.Warning))
            item["locked"] = bool(obj.Locked)
        if hasattr(obj, "Params"):
            item["inputs"] = [describe_param(p) for p in obj.Params.Input]
            item["outputs"] = [describe_param(p) for p in obj.Params.Output]
        results.append(item)
    write_report("host-results.json", {"objects": results, "solutionState": str(document.SolutionState)})
    write_report("host-progress.json", {"stage": "complete", "count": len(objects)})
    print("ReBear example evaluated. See artifacts/example-check/host-results.json. Excel exporters disabled.")
except Exception:
    write_report("host-error.json", {"traceback": traceback.format_exc()})
    print(traceback.format_exc())
