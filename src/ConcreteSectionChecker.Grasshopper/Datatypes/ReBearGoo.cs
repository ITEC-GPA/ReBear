using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using ConcreteSectionChecker.Adapters;
using ConcreteSectionChecker.Grasshopper;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;

namespace ConcreteSectionChecker.Grasshopper
{
    /// <summary>A Grasshopper value read back from a file without its data: components report <see cref="Note"/> as an input error.</summary>
    public interface IReBearPlaceholder { string Note { get; } }

    /// <summary>
    /// Versioned persistence of internalized ReBear data: ReBearSchema, then ReBearData (JSON of <see cref="ModelCodec"/>) or
    /// ReBearNotPersistent with the reason. Results are not stored: they are recomputed by the upstream components.
    /// </summary>
    internal static class Persistence
    {
        public static bool Write(GH_IWriter writer, object value, string type)
        {
            writer.SetInt32("ReBearSchema", ModelCodec.Version);
            if (value is null) return true;
            if (ModelCodec.CanWrite(value))
                try { writer.SetString("ReBearData", ModelCodec.Write(value)); return true; }
                catch (NotSupportedException e) { writer.SetString("ReBearNotPersistent", $"{type}: {e.Message} Ricollegare i componenti a monte."); return true; }
            writer.SetString("ReBearNotPersistent", $"{type} non viene salvato nel file: ricollegare i componenti a monte per ricalcolarlo.");
            return true;
        }

        public static T Read<T>(GH_IReader reader, string type, out string note) where T : class
        {
            note = null;
            if (reader.ItemExists("ReBearData"))
            {
                try { return ModelCodec.Read<T>(reader.GetString("ReBearData")); }
                catch (Exception e) when (e is InvalidDataException or JsonException or ArgumentException or InvalidCastException or NullReferenceException or FormatException)
                { note = $"{type}: dato salvato non ricostruibile ({e.Message}). Ricollegare i componenti a monte."; return null; }
            }
            note = reader.ItemExists("ReBearNotPersistent") ? reader.GetString("ReBearNotPersistent")
                : $"{type}: dato internalizzato con ReBear 0.2 o precedente, non salvato nel file. Ricollegare i componenti a monte.";
            return null;
        }

        public static string Number(double value) => value.ToString("G6", CultureInfo.InvariantCulture);
    }

    /// <summary>Common goo of the ReBear data types: validity, description, persistence, typed script value.</summary>
    public abstract class ReBearGoo<T> : GH_Goo<T>, IReBearPlaceholder where T : class
    {
        protected ReBearGoo() { }
        protected ReBearGoo(T value) { Value = value; }
        public string Note { get; private set; }
        public override bool IsValid => Value is not null;
        public override string IsValidWhyNot => Value is null ? Note ?? "Dato vuoto." : string.Empty;
        public override string ToString() => Value is null ? Note ?? "Empty " + TypeName : Describe(Value);
        protected abstract string Describe(T value);
        public override object ScriptVariable() => Value;
        public override bool Write(GH_IWriter writer) => Persistence.Write(writer, Value, TypeName);
        public override bool Read(GH_IReader reader)
        {
            Value = Persistence.Read<T>(reader, TypeName, out string note);
            Note = note;
            return true;
        }
    }

    /// <summary>ReBear options and native GPC results without a dedicated type (Shear/SLE/Torsion/Detailing options, crack data…).</summary>
    public sealed class GH_ReBearData : ReBearGoo<object>
    {
        public GH_ReBearData() { }
        public GH_ReBearData(object value) : base(value) { }
        public override string TypeName => Value is null ? "ReBear data" : "ReBear " + Value.GetType().Name;
        public override string TypeDescription => "Opzioni ReBear o risultato GPC.";
        public override IGH_Goo Duplicate() => new GH_ReBearData(Value); // records are immutable
        protected override string Describe(object value) => value.ToString();
    }

    public sealed class GH_CheckResult : GH_Goo<CheckResult>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals };
        public GH_CheckResult() { }
        public GH_CheckResult(CheckResult result) { Value = result; }
        public override bool IsValid => Value is not null;
        public override string TypeName => "ReBear Check Result";
        public override string TypeDescription => "Esito, combinazione, utilizzo e dettagli GPC.";
        public override IGH_Goo Duplicate() => new GH_CheckResult(Value);
        public override string ToString() => Value?.ToString() ?? "Empty ReBear result";
        public override object ScriptVariable() => Value;
        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32("Version", 1);
            if (Value is not null) writer.SetString("Result", JsonSerializer.Serialize(Value, JsonOptions));
            return true;
        }
        public override bool Read(GH_IReader reader)
        {
            if (!reader.ItemExists("Version") || reader.GetInt32("Version") != 1) return false;
            Value = reader.ItemExists("Result") ? JsonSerializer.Deserialize<CheckResult>(reader.GetString("Result"), JsonOptions) : null;
            return true;
        }
    }
}
