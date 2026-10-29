using System;
using System.Collections.Generic;
using System.Linq;
using ConcreteSectionChecker.Adapters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;

namespace ConcreteSectionChecker.Grasshopper
{
    /// <summary>
    /// Base of every ReBear component: ports declared once (kind, access, default, named values), typed reading of
    /// Grasshopper data and structured results. Missing numerical values are NaN, never zero.
    /// </summary>
    public abstract class ReBearComponent : GpcComponent
    {
        protected enum Kind { Number, Integer, Boolean, Text, Generic, Point, Curve, Brep, Mesh, Colour }
        protected sealed record Port(Kind Kind, string Name, string Nick, string Description,
            GH_ParamAccess Access = GH_ParamAccess.item, object Default = null, bool Optional = false, (string Name, int Value)[] Values = null);
        protected static Port N(string name, string nick, string description, double? value = null, bool optional = false)
            => new(Kind.Number, name, nick, description, Default: value, Optional: optional);
        protected static Port I(string name, string nick, string description, int? value = null, params (string Name, int Value)[] values)
            => new(Kind.Integer, name, nick, description, Default: value, Values: values.Length == 0 ? null : values);
        protected static Port B(string name, string nick, string description, bool value = false)
            => new(Kind.Boolean, name, nick, description, Default: value);
        protected static Port T(string name, string nick, string description, string value = null)
            => new(Kind.Text, name, nick, description, Default: value);
        protected static Port G(string name, string nick, string description, bool optional = false)
            => new(Kind.Generic, name, nick, description, Optional: optional);
        protected static Port P(string name, string nick, string description) => new(Kind.Point, name, nick, description);
        protected static Port C(string name, string nick, string description) => new(Kind.Curve, name, nick, description);
        protected static Port Surface(string name, string nick, string description) => new(Kind.Brep, name, nick, description);
        protected static Port M(string name, string nick, string description) => new(Kind.Mesh, name, nick, description);
        protected static Port Colour(string name, string nick, string description) => new(Kind.Colour, name, nick, description);
        protected static Port List(Port port) => port with { Access = GH_ParamAccess.list };
        protected static Port Optional(Port port) => port with { Optional = true };
        protected static (string, int)[] Named(params string[] names) => names.Select((n, i) => (n, i)).ToArray();
        protected abstract Port[] Inputs { get; }
        protected abstract Port[] Outputs { get; }
        protected ReBearComponent(string name, string nick, string description)
            : base(name, nick, description) { }

        protected sealed override void RegisterInputParams(GH_InputParamManager p)
        {
            foreach (var s in Inputs)
            {
                int i = s.Kind switch
                {
                    Kind.Number => s.Default == null ? p.AddNumberParameter(s.Name, s.Nick, s.Description, s.Access)
                        : p.AddNumberParameter(s.Name, s.Nick, s.Description, s.Access, (double)s.Default),
                    Kind.Integer => s.Default == null ? p.AddIntegerParameter(s.Name, s.Nick, s.Description, s.Access)
                        : p.AddIntegerParameter(s.Name, s.Nick, s.Description, s.Access, (int)s.Default),
                    Kind.Boolean => p.AddBooleanParameter(s.Name, s.Nick, s.Description, s.Access, (bool)s.Default),
                    Kind.Text => s.Default == null ? p.AddTextParameter(s.Name, s.Nick, s.Description, s.Access)
                        : p.AddTextParameter(s.Name, s.Nick, s.Description, s.Access, (string)s.Default),
                    Kind.Point => p.AddPointParameter(s.Name, s.Nick, s.Description, s.Access),
                    Kind.Curve => p.AddCurveParameter(s.Name, s.Nick, s.Description, s.Access),
                    Kind.Brep => p.AddBrepParameter(s.Name, s.Nick, s.Description, s.Access),
                    Kind.Mesh => p.AddMeshParameter(s.Name, s.Nick, s.Description, s.Access),
                    Kind.Colour => p.AddColourParameter(s.Name, s.Nick, s.Description, s.Access),
                    _ => p.AddGenericParameter(s.Name, s.Nick, s.Description, s.Access)
                };
                p[i].Optional = s.Optional;
                if (s.Values != null && p[i] is Param_Integer integer)
                    foreach (var (name, value) in s.Values) integer.AddNamedValue(name, value);
            }
        }
        protected sealed override void RegisterOutputParams(GH_OutputParamManager p)
        {
            foreach (var s in Outputs)
                switch (s.Kind)
                {
                    case Kind.Number: p.AddNumberParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case Kind.Integer: p.AddIntegerParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case Kind.Boolean: p.AddBooleanParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case Kind.Text: p.AddTextParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case Kind.Point: p.AddPointParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case Kind.Curve: p.AddCurveParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case Kind.Brep: p.AddBrepParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case Kind.Mesh: p.AddMeshParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case Kind.Colour: p.AddColourParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    default: p.AddGenericParameter(s.Name, s.Nick, s.Description, s.Access); break;
                }
        }

        /// <summary>Port label for messages: "Nome (n)" with 1-based index.</summary>
        protected string Label(int i) => i < Params.Input.Count ? $"{Params.Input[i].Name} ({i + 1})" : $"Input {i + 1}";

        protected static object Unwrap(object value)
        {
            while (value is IGH_Goo goo)
            {
                if (goo is IReBearPlaceholder placeholder && placeholder.Note != null)
                    throw new ArgumentException(placeholder.Note);
                var next = goo.ScriptVariable();
                if (ReferenceEquals(next, value)) break;
                value = next;
            }
            return value;
        }
        protected bool TryRead<V>(IGH_DataAccess d, int i, out V result)
        {
            object value = null; result = default;
            if (!d.GetData(i, ref value)) return false;
            value = Unwrap(value);
            if (value == null) return false;
            if (value is V typed) { result = typed; return true; }
            if (typeof(V) == typeof(double) && value is int n) { result = (V)(object)(double)n; return true; }
            if (typeof(V) == typeof(int) && value is double x && x == Math.Truncate(x) && Math.Abs(x) <= int.MaxValue) { result = (V)(object)(int)x; return true; }
            throw new ArgumentException($"{Label(i)}: atteso {Describe(typeof(V))}, ricevuto {value.GetType().Name}.");
        }
        protected V Read<V>(IGH_DataAccess d, int i, V fallback = default) => TryRead(d, i, out V value) ? value : fallback;
        protected V Required<V>(IGH_DataAccess d, int i) =>
            TryRead(d, i, out V value) ? value : throw new ArgumentException($"{Label(i)}: dato mancante o vuoto.");
        protected double? ReadOptional(IGH_DataAccess d, int i) => TryRead(d, i, out double value) ? value : null;
        /// <summary>The Grasshopper wrapper itself, for goo types that carry more than their value.</summary>
        protected G RequiredGoo<G>(IGH_DataAccess d, int i) where G : class, IGH_Goo
        {
            object value = null;
            if (!d.GetData(i, ref value) || value == null) throw new ArgumentException($"{Label(i)}: dato mancante o vuoto.");
            if (value is GH_ObjectWrapper wrapper) value = wrapper.Value;
            if (value is IReBearPlaceholder { Note: not null } placeholder) throw new ArgumentException(placeholder.Note);
            return value as G ?? throw new ArgumentException($"{Label(i)}: atteso {typeof(G).Name}, ricevuto {value.GetType().Name}.");
        }
        protected List<V> ReadList<V>(IGH_DataAccess d, int i)
        {
            var values = new List<object>(); d.GetDataList(i, values);
            return values.Select((v, k) => Unwrap(v) is V typed ? typed :
                throw new ArgumentException($"{Label(i)}: elemento {k} vuoto o diverso da {Describe(typeof(V))}.")).ToList();
        }
        /// <summary>Finite number check with the port name in the message.</summary>
        protected double Finite(IGH_DataAccess d, int i, bool positive = false, bool nonNegative = false)
        {
            double v = Required<double>(d, i);
            if (!double.IsFinite(v) || positive && v <= 0 || nonNegative && v < 0)
                throw new ArgumentException($"{Label(i)}: valore finito{(positive ? " e positivo" : nonNegative ? " non negativo" : "")} richiesto.");
            return v;
        }
        protected int Choice(IGH_DataAccess d, int i, int count)
        {
            int v = Required<int>(d, i);
            if (v < 0 || v >= count) throw new ArgumentException($"{Label(i)}: valore {v} non valido (0–{count - 1}).");
            return v;
        }
        private static string Describe(Type type) => type.Name switch
        {
            "Double" => "un numero", "Int32" => "un intero", "Boolean" => "un booleano", "String" => "un testo",
            _ => type.Name
        };

        protected void Result(IGH_DataAccess d, CheckResult result, int index = 0)
        {
            d.SetData(index, new GH_CheckResult(result));
            if (result.Status == CheckStatus.NotEvaluated)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, result.Message);
        }
        protected static GH_ReBearData Data(object value) => value == null ? null : new GH_ReBearData(value);
        protected static double Number(double? value) => value ?? double.NaN;
    }
}
