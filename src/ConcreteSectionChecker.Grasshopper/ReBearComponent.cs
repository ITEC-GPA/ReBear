using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ConcreteSectionChecker.Adapters;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace ConcreteSectionChecker.Grasshopper
{
    public sealed class GH_CheckResult : GH_Goo<CheckResult>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals };
        public GH_CheckResult() { }
        public GH_CheckResult(CheckResult result) { Value = result; }
        public override bool IsValid => Value != null;
        public override string TypeName => "ReBear Check Result";
        public override string TypeDescription => "Esito, combinazione, utilizzo e dettagli GPC.";
        public override IGH_Goo Duplicate() => new GH_CheckResult(Value);
        public override string ToString() => Value?.ToString() ?? "Empty ReBear result";
        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("Version", 1);
            if (Value != null) writer.SetString("Result", JsonSerializer.Serialize(Value, JsonOptions));
            return true;
        }
        public override bool Read(GH_IReader reader)
        {
            if (!reader.ItemExists("Version") || reader.GetInt32("Version") != 1) return false;
            Value = reader.ItemExists("Result") ? JsonSerializer.Deserialize<CheckResult>(reader.GetString("Result"), JsonOptions) : null;
            return true;
        }
    }

    /// <summary>Shared port declarations for new components; historical ports stay untouched.</summary>
    public abstract class ReBearComponent : GpcComponent
    {
        protected sealed record Port(string Kind, string Name, string Nick, string Description,
            GH_ParamAccess Access = GH_ParamAccess.item, object Default = null, bool Optional = false);
        protected static Port N(string name, string nick, string description, double? value = null, bool optional = false)
            => new("N", name, nick, description, Default: value, Optional: optional);
        protected static Port I(string name, string nick, string description, int? value = null)
            => new("I", name, nick, description, Default: value);
        protected static Port B(string name, string nick, string description, bool value = false)
            => new("B", name, nick, description, Default: value);
        protected static Port T(string name, string nick, string description, string value = null)
            => new("T", name, nick, description, Default: value);
        protected static Port G(string name, string nick, string description, bool optional = false)
            => new("G", name, nick, description, Optional: optional);
        protected static Port P(string name, string nick, string description) => new("P", name, nick, description);
        protected static Port C(string name, string nick, string description) => new("C", name, nick, description);
        protected static Port Surface(string name, string nick, string description) => new("S", name, nick, description);
        protected static Port List(Port port) => port with { Access = GH_ParamAccess.list };
        protected abstract Port[] Inputs { get; }
        protected abstract Port[] Outputs { get; }
        protected ReBearComponent(string name, string nick, string description, string subcategory)
            : base(name, nick, description, "ReBear", subcategory) { }

        protected sealed override void RegisterInputParams(GH_InputParamManager p)
        {
            foreach (var s in Inputs)
            {
                int i = s.Kind switch
                {
                    "N" => s.Default == null ? p.AddNumberParameter(s.Name, s.Nick, s.Description, s.Access)
                        : p.AddNumberParameter(s.Name, s.Nick, s.Description, s.Access, (double)s.Default),
                    "I" => s.Default == null ? p.AddIntegerParameter(s.Name, s.Nick, s.Description, s.Access)
                        : p.AddIntegerParameter(s.Name, s.Nick, s.Description, s.Access, (int)s.Default),
                    "B" => p.AddBooleanParameter(s.Name, s.Nick, s.Description, s.Access, (bool)s.Default),
                    "T" => s.Default == null ? p.AddTextParameter(s.Name, s.Nick, s.Description, s.Access)
                        : p.AddTextParameter(s.Name, s.Nick, s.Description, s.Access, (string)s.Default),
                    "P" => p.AddPointParameter(s.Name, s.Nick, s.Description, s.Access),
                    "C" => p.AddCurveParameter(s.Name, s.Nick, s.Description, s.Access),
                    "S" => p.AddBrepParameter(s.Name, s.Nick, s.Description, s.Access),
                    _ => p.AddGenericParameter(s.Name, s.Nick, s.Description, s.Access)
                };
                p[i].Optional = s.Optional;
            }
        }
        protected sealed override void RegisterOutputParams(GH_OutputParamManager p)
        {
            foreach (var s in Outputs)
                switch (s.Kind)
                {
                    case "N": p.AddNumberParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case "I": p.AddIntegerParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case "B": p.AddBooleanParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case "T": p.AddTextParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case "P": p.AddPointParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case "C": p.AddCurveParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    case "S": p.AddBrepParameter(s.Name, s.Nick, s.Description, s.Access); break;
                    default: p.AddGenericParameter(s.Name, s.Nick, s.Description, s.Access); break;
                }
        }
        protected static object Unwrap(object value)
        {
            while (value is IGH_Goo goo)
            {
                var next = goo.ScriptVariable();
                if (ReferenceEquals(next, value)) break;
                value = next;
            }
            return value;
        }
        protected static V Read<V>(IGH_DataAccess d, int i, V fallback = default)
        {
            object value = null;
            if (!d.GetData(i, ref value)) return fallback;
            value = Unwrap(value);
            if (value is V typed) return typed;
            if (typeof(V) == typeof(double) && value is int n) return (V)(object)(double)n;
            throw new ArgumentException($"Input {i + 1}: atteso {typeof(V).Name}.");
        }
        protected static V Required<V>(IGH_DataAccess d, int i) where V : class =>
            Read<V>(d, i) ?? throw new ArgumentException($"Input {i + 1} mancante o vuoto.");
        protected static List<V> ReadList<V>(IGH_DataAccess d, int i)
        {
            var values = new List<object>(); d.GetDataList(i, values);
            return values.Select(v => Unwrap(v) is V typed ? typed :
                throw new ArgumentException($"Input {i + 1}: elemento vuoto o tipo diverso da {typeof(V).Name}.")).ToList();
        }
        protected void Result(IGH_DataAccess d, CheckResult result, int index = 0)
        {
            d.SetData(index, new GH_CheckResult(result));
            if (result.Status == CheckStatus.NotEvaluated)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, result.Message);
        }
    }
}
