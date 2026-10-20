using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;

internal sealed class Graph : IDisposable
{
    private static readonly Assembly Plugin = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Program.Root, "bin/ReBear.gha"));
    // GH_Document initializes RhinoDoc and requires the native host. Write its observed archive
    // container explicitly; every component and panel still uses its own official GH serializer.
    private readonly List<IGH_DocumentObject> objects = new();
    private readonly List<GH_Component> nodes = new();
    private readonly List<(GH_Component From, int Output, GH_Component To, int Input)> wires = new();
    private readonly Dictionary<GH_Component, Dictionary<int, string>> values = new();
    public GH_Component Add(string type)
    {
        var t = Plugin.GetTypes().Single(x => x.Name == type && !x.IsAbstract && typeof(GH_Component).IsAssignableFrom(x));
        var c = (GH_Component)Activator.CreateInstance(t);
        c.CreateAttributes(); nodes.Add(c); values.Add(c, new()); objects.Add(c);
        return c;
    }
    public void Set(GH_Component c, int index, params object[] data)
    {
        var p = c.Params.Input[index];
        if (p.SourceCount != 0) throw new InvalidOperationException("Cannot set a connected input.");
        switch (p)
        {
            case Param_Number n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_Number(Convert.ToDouble(v, CultureInfo.InvariantCulture))); break;
            case Param_Integer n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_Integer(Convert.ToInt32(v, CultureInfo.InvariantCulture))); break;
            case Param_Boolean n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_Boolean((bool)v)); break;
            case Param_String n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_String((string)v)); break;
            case Param_Point n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_Point((Point3d)v)); break;
            default: throw new NotSupportedException(p.GetType().Name);
        }
        values[c][index] = string.Join(", ", data.Select(v => v is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : v.ToString()));
    }
    public void Wire(GH_Component from, GH_Component to, int input, int output = 0)
    {
        to.Params.Input[input].AddSource(from.Params.Output[output]);
        wires.Add((from, output, to, input)); values[to].Remove(input);
    }
    public void Numbers(GH_Component c, int offset, params double[] data)
    { for (int i = 0; i < data.Length; i++) Set(c, i + offset, data[i]); }
    public void Report(GH_Component result, int output = 0)
    {
        var report = Add("CheckReportComponent"); Wire(result, report, 0, output);
        var inspect = Add("CheckResultComponent"); Wire(result, inspect, 0, output);
        var panel = new GH_Panel { NickName = "CSV / risultati" };
        panel.CreateAttributes(); panel.AddSource(report.Params.Output[0]); objects.Add(panel);
    }
    public string Save(string folder, string title)
    {
        int Level(GH_Component c, HashSet<GH_Component> visited = null)
        {
            visited ??= new();
            if (!visited.Add(c)) throw new InvalidOperationException("Cycle in example graph.");
            var sources = wires.Where(w => w.To == c).Select(w => w.From).ToArray();
            return sources.Length == 0 ? 0 : 1 + sources.Max(s => Level(s, new(visited)));
        }
        var levels = nodes.ToDictionary(c => c, c => Level(c));
        var counts = new Dictionary<int,int>();
        foreach (var c in nodes)
        {
            int level = levels[c]; counts.TryGetValue(level, out int row); counts[level] = row + 1;
            c.Attributes.Pivot = new PointF(200 + level * 380, 280 + row * 480);
            c.Attributes.ExpireLayout(); c.Attributes.PerformLayout();
        }
        int panelRow = 0;
        foreach (var panel in objects.OfType<GH_Panel>())
        {
            panel.Attributes.Pivot = new PointF(200 + (levels.Values.Max() + 1) * 380, 280 + panelRow++ * 430);
            panel.Attributes.Bounds = new RectangleF(panel.Attributes.Pivot, new SizeF(650, 350));
        }
        var note = new GH_Panel { NickName = "ReBear — esempio svolto", UserText = title +
            "\nGeometria: mm; forze: kN; momenti: kNm; compressione negativa.\nDati persistenti nelle porte: tasto destro > Set...\nConsultare README.md e results.json nella stessa cartella.\nRun=True nei componenti di analisi predisposti." };
        note.CreateAttributes(); note.Attributes.Pivot = new PointF(20, 20); note.Attributes.Bounds = new RectangleF(20,20,700,160);
        objects.Add(note);
        foreach (var c in nodes)
            foreach (var p in c.Params.Input.Where(p => !p.Optional && p.SourceCount == 0))
                if (p.GetType().GetProperty("PersistentData")?.GetValue(p) is not IGH_Structure tree || tree.DataCount == 0)
                    throw new InvalidOperationException(c.Name + ": input required without data: " + p.Name);
        var archive = new GH_Archive();
        if (!archive.AppendObject(new Definition(objects, title), "Definition")) throw new InvalidOperationException("Document serialization failed.");
        string xml = archive.Serialize_Xml();
        File.WriteAllText(Path.Combine(folder, "definition.ghx"), xml);
        if (!archive.WriteToFile(Path.Combine(folder, "definition.gh"), true, false)) throw new IOException("GH binary write failed.");
        var readback = new GH_Archive();
        if (!readback.ReadFromFile(Path.Combine(folder, "definition.gh"))) throw new IOException("GH binary read failed.");
        var savedObjects = readback.GetRootNode.FindChunk("Definition").FindChunk("DefinitionObjects");
        for (int i = 0; i < nodes.Count; i++)
        {
            var original = nodes[i];
            var restored = (GH_Component)Activator.CreateInstance(original.GetType());
            restored.CreateAttributes();
            var reader = savedObjects.FindChunk("Object", objects.IndexOf(original)).FindChunk("Container");
            if (!restored.Read(reader) || restored.InstanceGuid != original.InstanceGuid ||
                restored.Params.Input.Count != original.Params.Input.Count || restored.Params.Output.Count != original.Params.Output.Count)
                throw new InvalidDataException("Component deserialization failed: " + original.Name);
            foreach (int input in values[original].Keys)
            {
                var before = original.Params.Input[input].GetType().GetProperty("PersistentData")?.GetValue(original.Params.Input[input]) as IGH_Structure;
                var after = restored.Params.Input[input].GetType().GetProperty("PersistentData")?.GetValue(restored.Params.Input[input]) as IGH_Structure;
                if (before == null || after == null || before.DataCount != after.DataCount ||
                    !before.AllData(true).Select(v=>v.ToString()).SequenceEqual(after.AllData(true).Select(v=>v.ToString())))
                    throw new InvalidDataException("Persistent input round trip failed: " + original.Name);
            }
        }
        // Check serialized object identities and every source connection without loading the host/plugin server.
        var parsed = System.Xml.Linq.XDocument.Parse(readback.Serialize_Xml());
        var ids = parsed.Descendants("item").Where(e => (string)e.Attribute("name") == "InstanceGuid").Select(e => e.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var c in nodes) if (!ids.Contains(c.InstanceGuid.ToString())) throw new InvalidDataException("Missing component " + c.Name);
        var sourceIds = parsed.Descendants("item").Where(e => (string)e.Attribute("name") == "Source").Select(e => e.Value).ToArray();
        foreach (var w in wires)
            if (!sourceIds.Contains(w.From.Params.Output[w.Output].InstanceGuid.ToString(), StringComparer.OrdinalIgnoreCase))
                throw new InvalidDataException("Missing saved connection.");
        File.WriteAllText(Path.Combine(folder,"graph.json"), JsonSerializer.Serialize(new {
            components = nodes.Select((c,i) => new { index=i, type=c.GetType().Name, guid=c.ComponentGuid,
                inputs=values[c].ToDictionary(v=>c.Params.Input[v.Key].Name,v=>v.Value) }),
            connections = wires.Select(w => new { from=nodes.IndexOf(w.From), output=w.Output, to=nodes.IndexOf(w.To), input=w.Input })
        }, new JsonSerializerOptions { WriteIndented=true }));
        var text = new StringBuilder("\n## Componenti e collegamenti\n\nI valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.\n\n");
        foreach (var c in nodes)
        {
            text.AppendLine($"**{nodes.IndexOf(c):D2} — {c.Name}**\n");
            foreach (var v in values[c]) text.AppendLine($"- {c.Params.Input[v.Key].Name} = `{v.Value}`");
            foreach (var w in wires.Where(w=>w.To==c)) text.AppendLine($"- {c.Params.Input[w.Input].Name} ← {nodes.IndexOf(w.From):D2} {w.From.Name} / {w.From.Params.Output[w.Output].Name}");
            text.AppendLine();
        }
        return text.ToString();
    }
    public void Dispose() { }

    private sealed class Definition(List<IGH_DocumentObject> objects, string title) : GH_IO.GH_ISerializable
    {
        public bool Read(GH_IReader reader) => throw new NotSupportedException();
        public bool Write(GH_IWriter writer)
        {
            Guid library = new("7ca34ed5-c328-465a-9940-b4c4f3d924b3");
            writer.SetVersion("plugin_version", 1, 0, 8);
            var header=writer.CreateChunk("DocumentHeader");
            header.SetGuid("DocumentID",Guid.NewGuid()); header.SetString("Preview","Shaded"); header.SetInt32("PreviewMeshType",1);
            header.SetDrawingColor("PreviewNormal",Color.FromArgb(100,150,0,0)); header.SetDrawingColor("PreviewSelected",Color.FromArgb(100,0,150,0));
            var props=writer.CreateChunk("DefinitionProperties");
            props.SetDate("Date",new DateTime(2026,10,20)); props.SetString("Name",title); props.SetString("Description","Esempio ReBear; leggere README.md."); props.SetBoolean("KeepOpen",false);
            props.CreateChunk("Revisions").SetInt32("RevisionCount",0); props.CreateChunk("Views").SetInt32("ViewCount",0);
            var projection=props.CreateChunk("Projection"); projection.SetDrawingPoint("Target",new System.Drawing.Point(20,20)); projection.SetSingle("Zoom",.8f);
            writer.CreateChunk("RcpLayout").SetInt32("GroupCount",0);
            var libraries=writer.CreateChunk("GHALibraries"); libraries.SetInt32("Count",1);
            var lib=libraries.CreateChunk("Library",0); lib.SetGuid("Id",library); lib.SetString("Name","ReBear"); lib.SetString("Author","GPC"); lib.SetString("Version","0.2.0");
            var definition=writer.CreateChunk("DefinitionObjects"); definition.SetInt32("ObjectCount",objects.Count);
            for(int i=0;i<objects.Count;i++)
            {
                var c=objects[i]; var item=definition.CreateChunk("Object",i); item.SetGuid("GUID",c.ComponentGuid); item.SetString("Name",c.Name);
                if(c is GH_Component) item.SetGuid("Lib",library);
                if(!c.Write(item.CreateChunk("Container"))) return false;
            }
            return true;
        }
    }
}
