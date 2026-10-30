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
    internal IReadOnlyList<GH_Component> Nodes => nodes;
    private readonly List<(GH_Component From, int Output, GH_Component To, int Input)> wires = new();
    private readonly Dictionary<GH_Component, Dictionary<int, string>> values = new();
    private readonly List<(GH_Panel Panel, GH_Component Source)> panels = new();
    private static readonly SizeF PanelSize = new(650, 350);
    private const float Left = 40, Top = 240, RowGap = 40, ColumnGap = 120;
    public GH_Component Add(string type)
    {
        var t = Plugin.GetTypes().Single(x => x.Name == type && !x.IsAbstract && typeof(GH_Component).IsAssignableFrom(x));
        var c = (GH_Component)Activator.CreateInstance(t);
        c.CreateAttributes(); nodes.Add(c); values.Add(c, new()); objects.Add(c);
        return c;
    }
    /// <summary>Stores a value in an input port. A value equal to the component default is refused: the port already holds it.</summary>
    public void Set(GH_Component c, int index, params object[] data) => Assign(c, index, data, keepDefault: false);
    private void Assign(GH_Component c, int index, object[] data, bool keepDefault)
    {
        var p = c.Params.Input[index];
        if (p.SourceCount != 0) throw new InvalidOperationException("Cannot set a connected input.");
        string Stored() => p.GetType().GetProperty("PersistentData")?.GetValue(p) is IGH_Structure s ? string.Join("|", s.AllData(true).Select(v => v?.ToString())) : null;
        string before = Stored();
        Store(p, data);
        if (!keepDefault && Stored() == before)
            throw new InvalidOperationException($"{c.Name}: {p.Name} = {before} is the component default; omit it.");
        values[c][index] = string.Join(", ", data.Select(v => v is IFormattable f ? f.ToString(null, CultureInfo.InvariantCulture) : v.ToString()));
    }
    private static void Store(IGH_Param p, object[] data)
    {
        switch (p)
        {
            case Param_Number n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_Number(Convert.ToDouble(v, CultureInfo.InvariantCulture))); break;
            case Param_Integer n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_Integer(Convert.ToInt32(v, CultureInfo.InvariantCulture))); break;
            case Param_Boolean n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_Boolean((bool)v)); break;
            case Param_String n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_String((string)v)); break;
            case Param_Point n: n.PersistentData.Clear(); foreach (var v in data) n.PersistentData.Append(new GH_Point((Point3d)v)); break;
            default: throw new NotSupportedException(p.GetType().Name);
        }
    }
    public void Wire(GH_Component from, GH_Component to, int input, int output = 0)
    {
        to.Params.Input[input].AddSource(from.Params.Output[output]);
        wires.Add((from, output, to, input)); values[to].Remove(input);
    }
    public void Numbers(GH_Component c, int offset, params double[] data)
    { for (int i = 0; i < data.Length; i++) Set(c, i + offset, data[i]); }
    /// <summary>Section dimensions and bars: all stored, also when equal to a default, because together they describe the section.</summary>
    public void Dimensions(GH_Component c, int offset, params double[] data)
    { for (int i = 0; i < data.Length; i++) Assign(c, i + offset, new object[] { data[i] }, keepDefault: true); }
    public void Report(GH_Component result, int output = 0)
    {
        var report = Add("CheckReportComponent"); Wire(result, report, 0, output);
        var inspect = Add("CheckResultComponent"); Wire(result, inspect, 0, output);
        var panel = new GH_Panel { NickName = "CSV / risultati" };
        panel.CreateAttributes(); panel.AddSource(report.Params.Output[0]); objects.Add(panel); panels.Add((panel, report));
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
        // Left to right: one column per depth of the longest path from the inputs, each report panel in the column after
        // its Check Report. Rows follow the connected objects, so wires stay short and seldom cross.
        var levels = nodes.ToDictionary(c => c, c => Level(c));
        var items = nodes.Cast<IGH_DocumentObject>().Concat(panels.Select(p => p.Panel)).ToList();
        var sources = items.ToDictionary(o => o, _ => new List<IGH_DocumentObject>());
        var targets = items.ToDictionary(o => o, _ => new List<IGH_DocumentObject>());
        foreach (var (from, to) in wires.Select(w => ((IGH_DocumentObject)w.From, (IGH_DocumentObject)w.To))
            .Concat(panels.Select(p => ((IGH_DocumentObject)p.Source, (IGH_DocumentObject)p.Panel))))
        { sources[to].Add(from); targets[from].Add(to); }
        int Column(IGH_DocumentObject o) => o is GH_Component c ? levels[c] : levels[panels.Single(p => p.Panel == o).Source] + 1;
        var columns = items.GroupBy(Column).OrderBy(g => g.Key).Select(g => g.ToList()).ToList();
        if (columns.Count != items.Max(Column) + 1) throw new InvalidOperationException("Empty column in the layout.");
        int Rank(IGH_DocumentObject o) => columns[Column(o)].IndexOf(o);
        void Order(List<IGH_DocumentObject> column, Dictionary<IGH_DocumentObject, List<IGH_DocumentObject>> neighbours)
        {
            var key = column.ToDictionary(o => o, o => neighbours[o].Count == 0 ? column.IndexOf(o) : neighbours[o].Average(Rank));
            var sorted = column.OrderBy(o => key[o]).ThenBy(column.IndexOf).ToList();
            column.Clear(); column.AddRange(sorted);
        }
        for (int sweep = 0; sweep < 4; sweep++)
        {
            for (int k = 1; k < columns.Count; k++) Order(columns[k], sources);
            for (int k = columns.Count - 2; k >= 0; k--) Order(columns[k], targets);
        }
        SizeF Size(IGH_DocumentObject o)
        {
            if (o is GH_Panel) return PanelSize;
            o.Attributes.ExpireLayout(); o.Attributes.PerformLayout(); return o.Attributes.Bounds.Size;
        }
        void Place(IGH_DocumentObject o, float left, float top)
        {
            if (o is GH_Panel panel) { panel.Attributes.Pivot = new PointF(left, top); panel.Attributes.Bounds = new RectangleF(left, top, PanelSize.Width, PanelSize.Height); return; }
            var bounds = o.Attributes.Bounds; var pivot = o.Attributes.Pivot;
            o.Attributes.Pivot = new PointF(left + pivot.X - bounds.X, top + pivot.Y - bounds.Y);
            o.Attributes.ExpireLayout(); o.Attributes.PerformLayout();
        }
        // Each object at the mean height of the objects it is wired to, alternately towards its targets and its sources.
        // The order inside a column is kept: objects that would overlap form a block placed at the mean of their wishes.
        var sizes = items.ToDictionary(o => o, Size);
        var tops = new Dictionary<IGH_DocumentObject, float>();
        float Centre(IGH_DocumentObject o) => tops[o] + sizes[o].Height / 2;
        void Align(List<IGH_DocumentObject> column, Dictionary<IGH_DocumentObject, List<IGH_DocumentObject>> neighbours)
        {
            // Sum = Σ (wanted top − offset inside the block), so the block top is Sum / count.
            var blocks = new List<(List<IGH_DocumentObject> Items, float Sum, float Height)>();
            foreach (var o in column)
            {
                float wanted = neighbours[o].Count == 0 ? tops[o] : neighbours[o].Average(Centre) - sizes[o].Height / 2;
                blocks.Add((new List<IGH_DocumentObject> { o }, wanted, sizes[o].Height));
                while (blocks.Count > 1)
                {
                    var (upper, lower) = (blocks[^2], blocks[^1]);
                    if (upper.Sum / upper.Items.Count + upper.Height + RowGap <= lower.Sum / lower.Items.Count) break;
                    float offset = upper.Height + RowGap;
                    upper.Items.AddRange(lower.Items);
                    blocks[^2] = (upper.Items, upper.Sum + lower.Sum - offset * lower.Items.Count, offset + lower.Height);
                    blocks.RemoveAt(blocks.Count - 1);
                }
            }
            foreach (var block in blocks)
            {
                float y = block.Sum / block.Items.Count;
                foreach (var o in block.Items) { tops[o] = y; y += sizes[o].Height + RowGap; }
            }
        }
        foreach (var column in columns)
        {
            float y = Top;
            foreach (var o in column) { tops[o] = y; y += sizes[o].Height + RowGap; }
        }
        for (int sweep = 0; sweep < 3; sweep++)
        {
            for (int k = columns.Count - 2; k >= 0; k--) Align(columns[k], targets);
            for (int k = 1; k < columns.Count; k++) Align(columns[k], sources);
        }
        float shift = Top - tops.Values.Min(), x = Left;
        foreach (var column in columns)
        {
            foreach (var o in column) Place(o, x, tops[o] + shift);
            x += column.Max(o => sizes[o].Width) + ColumnGap;
        }
        var note = new GH_Panel { NickName = "ReBear — esempio svolto", UserText = title +
            "\nGeometria: mm; forze: kN; momenti: kNm; compressione negativa.\nDati a sinistra, verifiche e risultati verso destra.\nDati persistenti nelle porte: tasto destro > Set...\nConsultare README.md e results.json nella stessa cartella.\nRun=True nei componenti di analisi predisposti." };
        note.CreateAttributes(); note.Attributes.Pivot = new PointF(Left, 20); note.Attributes.Bounds = new RectangleF(Left, 20, 700, 180);
        objects.Add(note);
        var boxes = items.Append(note).Select(o => (Object: o, Box: o.Attributes.Bounds)).ToArray();
        for (int i = 0; i < boxes.Length; i++)
            for (int j = i + 1; j < boxes.Length; j++)
                if (RectangleF.Inflate(boxes[i].Box, RowGap / 4, RowGap / 4).IntersectsWith(boxes[j].Box))
                    throw new InvalidOperationException($"Layout: {boxes[i].Object.Name} overlaps {boxes[j].Object.Name}.");
        foreach (var o in items)
            foreach (var s in sources[o])
                if (s.Attributes.Bounds.Right + ColumnGap / 2 > o.Attributes.Bounds.Left)
                    throw new InvalidOperationException($"Layout: the wire {s.Name} → {o.Name} does not run left to right.");
        // graph.json and the README number the components as they are read on the canvas.
        var reading = columns.SelectMany(column => column).OfType<GH_Component>().ToList();
        nodes.Clear(); nodes.AddRange(reading);
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
        var text = new StringBuilder("\n## Componenti e collegamenti\n\nComponenti numerati come sulla tela, da sinistra a destra e dall'alto in basso. I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.\n\n");
        foreach (var c in nodes)
        {
            text.AppendLine($"**{nodes.IndexOf(c):D2} — {c.Name}**\n");
            foreach (var v in values[c]) text.AppendLine($"- {c.Params.Input[v.Key].Name} = `{v.Value}`");
            foreach (var w in wires.Where(w=>w.To==c)) text.AppendLine($"- {c.Params.Input[w.Input].Name} ← {nodes.IndexOf(w.From):D2} {w.From.Name} / {w.From.Params.Output[w.Output].Name}");
            if (values[c].Count == 0 && !wires.Any(w => w.To == c)) text.AppendLine("- tutti i valori di default");
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
            var lib=libraries.CreateChunk("Library",0); lib.SetGuid("Id",library); lib.SetString("Name","ReBear"); lib.SetString("Author","GPC"); lib.SetString("Version",Program.ReBearVersion);
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
