# -*- coding: utf-8 -*-
"""Rhino 8 acceptance of the ReBear examples: run with _-RunPythonScript (IronPython or Python 3).

For every examples/<slug>/definition.gh the script opens the definition with the ReBear build in bin/,
solves it, waits for the background components (Batch, Search, Domain Slice, Moment Curvature) and records
for every object its runtime messages (errors, warnings, remarks), its message line and a summary of the
outputs. Results: artifacts/example-check/<slug>.json and artifacts/example-check/summary.json.
Nothing is exported and no plugin setting is changed persistently.

Options (edit below): SYNCHRONOUS = True runs the background components synchronously (REBEAR_SYNCHRONOUS=1);
EXAMPLES limits the run to some folders.
"""
import os
import json
import time
import traceback
import clr
import System
import Rhino

SYNCHRONOUS = False
EXAMPLES = []          # e.g. ["10-resistenza-inviluppo"]; empty = all
TIMEOUT_SECONDS = 180

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPORT = os.path.join(ROOT, "artifacts", "example-check")
GRASSHOPPER = os.path.join(str(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles)),
                           "Rhino 8", "Plug-ins", "Grasshopper")


def write_report(name, data):
    if not os.path.isdir(REPORT):
        os.makedirs(REPORT)
    with open(os.path.join(REPORT, name), "w") as stream:
        json.dump(data, stream, indent=2, ensure_ascii=True)


def text(value, limit=400):
    try:
        value = str(value)
    except Exception:
        value = repr(value)
    return value[:limit]


def load_grasshopper():
    clr.AddReferenceToFileAndPath(os.path.join(GRASSHOPPER, "Grasshopper.dll"))
    clr.AddReferenceToFileAndPath(os.path.join(GRASSHOPPER, "GH_IO.dll"))
    gh = Rhino.RhinoApp.GetPlugInObject("Grasshopper")
    gh.ShowEditor()
    import Grasshopper
    return Grasshopper


def load_rebear(Grasshopper):
    from Grasshopper.Kernel import GH_ExternalFile
    server = Grasshopper.Instances.ComponentServer
    plugin_path = os.path.join(ROOT, "bin", "ReBear.gha")
    already = [e.FilePath for e in server.ExternalFiles(False, False) if os.path.basename(e.FilePath).lower() == "rebear.gha"]
    if already:
        return {"loaded": True, "path": already[0], "note": "ReBear already loaded by Grasshopper"}
    # Rhino 8 exposes single-file loading internally; used only for this session.
    flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
    loader = server.GetType().GetMethod("LoadExternalFile", flags)
    loaded = loader.Invoke(server, System.Array[System.Object]([GH_ExternalFile(plugin_path), False]))
    return {"loaded": bool(loaded), "path": plugin_path}


def describe_param(param):
    values = list(param.VolatileData.AllData(True))
    return {"name": param.Name, "count": len(values),
            "nullCount": sum(1 for v in values if v is None),
            "invalidCount": sum(1 for v in values if v is not None and not v.IsValid),
            "samples": [text(v) for v in values[:3]]}


def computing(document):
    for obj in document.Objects:
        if getattr(obj, "IsComputing", False):
            return True
    return False


def run_example(Grasshopper, slug):
    from Grasshopper.Kernel import GH_Document, GH_RuntimeMessageLevel
    from GH_IO.Serialization import GH_Archive
    path = os.path.join(ROOT, "examples", slug, "definition.gh")
    archive = GH_Archive()
    if not archive.ReadFromFile(path):
        raise Exception("Cannot read " + path)
    document = GH_Document()
    if not archive.ExtractObject(document, "Definition"):
        raise Exception("Cannot extract the definition of " + slug)
    document.FilePath = path
    Grasshopper.Instances.DocumentServer.AddDocument(document)
    if Grasshopper.Instances.ActiveCanvas is not None:
        Grasshopper.Instances.ActiveCanvas.Document = document
    missing = [str(o.ComponentGuid) for o in document.Objects if o.GetType().Name == "GH_Placeholder"]
    start = time.time()
    document.Enabled = True
    document.NewSolution(False)
    # Background components post their outputs to the UI thread: keep pumping messages until they finish.
    while computing(document) and time.time() - start < TIMEOUT_SECONDS:
        Rhino.RhinoApp.Wait()
        time.sleep(0.05)
    elapsed = time.time() - start
    objects = []
    for obj in document.Objects:
        item = {"name": obj.Name, "nickname": obj.NickName, "guid": str(obj.ComponentGuid),
                "instance": str(obj.InstanceGuid), "assembly": obj.GetType().Assembly.GetName().Name}
        if hasattr(obj, "RuntimeMessages"):
            item["errors"] = [text(m) for m in obj.RuntimeMessages(GH_RuntimeMessageLevel.Error)]
            item["warnings"] = [text(m) for m in obj.RuntimeMessages(GH_RuntimeMessageLevel.Warning)]
            item["remarks"] = [text(m) for m in obj.RuntimeMessages(GH_RuntimeMessageLevel.Remark)]
            item["message"] = text(getattr(obj, "Message", "") or "")
            item["computing"] = bool(getattr(obj, "IsComputing", False))
        if hasattr(obj, "Params"):
            item["inputs"] = [describe_param(p) for p in obj.Params.Input]
            item["outputs"] = [describe_param(p) for p in obj.Params.Output]
        objects.append(item)
    result = {"example": slug, "seconds": round(elapsed, 3), "timedOut": computing(document),
              "missingComponents": missing, "solutionState": str(document.SolutionState), "objects": objects}
    expected = os.path.join(ROOT, "examples", slug, "results.json")
    if os.path.isfile(expected):
        with open(expected) as stream:
            result["expectedChecks"] = len(json.load(stream).get("checks", []))
    write_report(slug + ".json", result)
    Grasshopper.Instances.DocumentServer.RemoveDocument(document)
    document.Enabled = False
    return {"example": slug, "seconds": result["seconds"], "timedOut": result["timedOut"], "missing": len(missing),
            "errors": sum(len(o.get("errors", [])) for o in objects),
            "warnings": sum(len(o.get("warnings", [])) for o in objects),
            "components": len(objects)}


def main():
    write_report("host-progress.json", {"stage": "starting", "rhino": str(Rhino.RhinoApp.Version),
                                        "runtime": str(System.Environment.Version)})
    if SYNCHRONOUS:
        System.Environment.SetEnvironmentVariable("REBEAR_SYNCHRONOUS", "1")
    Grasshopper = load_grasshopper()
    loading = load_rebear(Grasshopper)
    assemblies = []
    for asm in System.AppDomain.CurrentDomain.GetAssemblies():
        name = asm.GetName().Name
        if name.startswith("GPC") or name in ["ReBear", "ConcreteSectionChecker.Adapters", "Grasshopper", "DelaunayMesh"]:
            location = ""
            try:
                location = asm.Location
            except Exception:
                pass
            assemblies.append({"name": name, "version": str(asm.GetName().Version), "path": location})
    write_report("host-load.json", {"rebear": loading, "assemblies": assemblies, "synchronous": SYNCHRONOUS})
    folders = sorted(d for d in os.listdir(os.path.join(ROOT, "examples"))
                     if os.path.isfile(os.path.join(ROOT, "examples", d, "definition.gh")))
    if EXAMPLES:
        folders = [d for d in folders if d in EXAMPLES]
    summary = []
    for slug in folders:
        write_report("host-progress.json", {"stage": "solving", "example": slug})
        try:
            summary.append(run_example(Grasshopper, slug))
        except Exception:
            summary.append({"example": slug, "exception": traceback.format_exc()})
    write_report("summary.json", {"examples": summary, "synchronous": SYNCHRONOUS})
    write_report("host-progress.json", {"stage": "complete", "examples": len(summary)})
    print("ReBear examples evaluated: see artifacts/example-check/summary.json")


try:
    main()
except Exception:
    write_report("host-error.json", {"traceback": traceback.format_exc()})
    print(traceback.format_exc())
