using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Detailing;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Torsion;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Standards;

namespace ConcreteSectionChecker.Adapters
{
    /// <summary>
    /// Versioned JSON persistence of the ReBear input objects (internalized Grasshopper data). Materials are stored by
    /// database name and verified on reading; standards by profile. Results are not persisted: recompute them.
    /// </summary>
    public static class ModelCodec
    {
        public const string Schema = "ReBear.Data";
        public const int Version = 1;
        private static readonly string[] Standards = { nameof(StandardNTC2018Concrete), nameof(StandardEN1992p11), nameof(StandardUNIEN1992p11) };

        public static bool CanWrite(object value) => value is StandardModelCode2010 or ConcreteMaterialEN1992 or SteelMaterialEN1992 or CoordinateSystem
            or SectionCheckerModelCode2010.SectionOptionsModelCode2010 or ReinforcedConcreteRebar or ReinforcedConcreteSection or SectionCheckerModelCode2010
            or ResultBeamForces or ShearOptions or TorsionOptions or SleOptions or DetailingOptions or PlateDetailingData;

        public static string Write(object value)
        {
            var node = new JsonObject { ["schema"] = Schema, ["version"] = Version, ["kind"] = Kind(value), ["data"] = Encode(value) };
            return node.ToJsonString();
        }

        public static object Read(string json)
        {
            var node = JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException("Dato ReBear non valido.");
            if ((string)node["schema"] != Schema) throw new InvalidDataException("Schema non riconosciuto.");
            int version = (int)node["version"];
            if (version > Version) throw new InvalidDataException($"Dato salvato da una versione più recente di ReBear (schema {version}).");
            return Decode((string)node["kind"], node["data"]);
        }

        public static T Read<T>(string json) => Read(json) is T value ? value : throw new InvalidDataException("Tipo salvato diverso da " + typeof(T).Name + ".");

        private static string Kind(object value) => value switch
        {
            StandardModelCode2010 => "Standard", ConcreteMaterialEN1992 => "Concrete", SteelMaterialEN1992 => "Steel",
            CoordinateSystem => "Axes", SectionCheckerModelCode2010.SectionOptionsModelCode2010 => "Options",
            ReinforcedConcreteRebar => "Rebar", ReinforcedConcreteSection => "Section", SectionCheckerModelCode2010 => "Checker",
            ResultBeamForces => "Forces", ShearOptions => "Shear", TorsionOptions => "Torsion", SleOptions => "Sle",
            DetailingOptions => "Detailing", PlateDetailingData => "Plate",
            _ => throw new NotSupportedException("Tipo non persistente: " + value?.GetType().Name)
        };

        private static JsonNode Encode(object value)
        {
            switch (value)
            {
                case StandardModelCode2010 s:
                    if (!Standards.Contains(s.GetType().Name)) throw new NotSupportedException("Norma non persistente: " + s.GetType().Name);
                    return new JsonObject { ["profile"] = s.GetType().Name };
                case ConcreteMaterialEN1992 c:
                    return new JsonObject { ["name"] = c.Name, ["fck"] = c.Fck };
                case SteelMaterialEN1992 s:
                    return new JsonObject { ["name"] = s.Name, ["curve"] = s.StressStrainCurve.ToString(), ["E"] = s.E, ["fyk"] = s.Fyk, ["fu"] = s.Fu };
                case CoordinateSystem a:
                    return new JsonObject { ["name"] = a.Name, ["origin"] = Vector(a.Origin.X, a.Origin.Y, a.Origin.Z),
                        ["v1"] = Vector(a.V1.X, a.V1.Y, a.V1.Z), ["v2"] = Vector(a.V2.X, a.V2.Y, a.V2.Z) };
                case SectionCheckerModelCode2010.SectionOptionsModelCode2010 o:
                    return new JsonObject
                    {
                        ["axes"] = Encode(o.ForceReferenceCoordinateSystem), ["criterion"] = o.FailureAnalysisType.ToString(),
                        ["domain"] = o.FailureDomainType.ToString(), ["stress"] = o.StressAnalysisType.ToString(),
                        ["strategy"] = o.DomainPointStrategy.ToString(), ["tension"] = o.StressAnalysisConcreteTension.ToString(),
                        ["psiRebar"] = o.PsiCoefficientRebar, ["psiTendon"] = o.PsiCoefficientTendon,
                        ["tensileConcrete"] = o.ConsiderTensileConcrete, ["theta"] = o.TetaDiscretization
                    };
                case ReinforcedConcreteRebar r:
                    if (r.RebarSection is not RebarSectionCircular) throw new NotSupportedException("Solo barre circolari persistenti.");
                    return new JsonObject { ["x"] = r.Position.X, ["y"] = r.Position.Y, ["diameter"] = r.RebarSection.Diameter,
                        ["material"] = Encode(r.RebarMaterial), ["sigmaP"] = r.EpsilonP * r.RebarMaterial.E, ["id"] = r.Id, ["name"] = r.Name };
                case ReinforcedConcreteSection s:
                    if (s.IsCompositeSteelConcrete) throw new NotSupportedException("Sezioni composte non persistenti.");
                    return new JsonObject
                    {
                        ["name"] = s.Name, ["concrete"] = Encode(s.ConcreteMaterial),
                        ["outline"] = Polygon(s.ConcreteShape.Fill2d),
                        ["holes"] = new JsonArray((s.ConcreteShape.Holes2d ?? Array.Empty<Polygon2d>()).Select(h => (JsonNode)Polygon(h)).ToArray()),
                        ["rebars"] = new JsonArray(s.Rebars.Select(r => Encode(r)).ToArray())
                    };
                case SectionCheckerModelCode2010 c:
                    return new JsonObject { ["section"] = Encode(c.SectionCheckerAttribute.Section), ["options"] = Encode(c.SectionCheckerOptionsModelCode2010),
                        ["standard"] = Encode(c.StandardModelCode2010) };
                case ResultBeamForces f:
                    return new JsonObject { ["N"] = f.N, ["V1"] = f.V1, ["V2"] = f.V2, ["T"] = f.T, ["M1"] = f.M1, ["M2"] = f.M2,
                        ["axes"] = Encode(f.CoordinateSystem), ["id"] = f.Id, ["name"] = f.Name };
                case ShearOptions p:
                    return new JsonObject { ["linkSteel"] = Encode(p.LinkSteel), ["longitudinalSteel"] = Encode(p.LongitudinalSteel),
                        ["width"] = p.Width, ["depth"] = p.Depth, ["longitudinalArea"] = p.LongitudinalArea, ["linkArea"] = p.LinkArea,
                        ["spacing"] = p.Spacing, ["alpha"] = p.Alpha, ["cotTheta"] = p.CotTheta, ["lever"] = p.LeverFactor,
                        ["aggregate"] = p.Aggregate, ["eccentricity"] = p.Eccentricity };
                case TorsionOptions t:
                    return new JsonObject { ["Ak"] = t.Geometry.EnclosedArea, ["uk"] = t.Geometry.Perimeter, ["tef"] = t.Geometry.Thickness,
                        ["linkSteel"] = Encode(t.LinkSteel), ["longitudinalSteel"] = Encode(t.LongitudinalSteel), ["legArea"] = t.LinkLegArea,
                        ["spacing"] = t.Spacing, ["longitudinalArea"] = t.LongitudinalArea, ["cotTheta"] = t.CotTheta, ["hollow"] = t.HollowBothFaces };
                case SleOptions s:
                    return JsonSerializer.SerializeToNode(s);
                case PlateDetailingData p:
                    return new JsonObject { ["secondary"] = p.SecondarySteelPerMetre, ["spacing"] = p.SecondarySpacing, ["critical"] = p.CriticalRegion };
                case DetailingOptions d:
                    return new JsonObject { ["kind"] = d.Kind.ToString(), ["top"] = d.TopWidth, ["bottom"] = d.BottomWidth, ["web"] = d.WebWidth,
                        ["linkDiameter"] = d.LinkDiameter, ["linkSpacing"] = d.LinkSpacing, ["legs"] = d.LinkLegs, ["cover"] = d.Cover,
                        ["aggregate"] = d.Aggregate, ["deviation"] = d.Deviation, ["durability"] = d.DurabilityCover, ["lap"] = d.LapZone,
                        ["restrained"] = d.Restrained, ["ends"] = d.EndZones, ["plate"] = d.Plate == null ? null : Encode(d.Plate) };
                default:
                    throw new NotSupportedException("Tipo non persistente: " + value?.GetType().Name);
            }
        }

        private static object Decode(string kind, JsonNode n)
        {
            switch (kind)
            {
                case "Standard":
                {
                    string profile = (string)n["profile"];
                    if (!Standards.Contains(profile)) throw new InvalidDataException("Norma non riconosciuta: " + profile);
                    return Activator.CreateInstance(typeof(StandardModelCode2010).Assembly.GetType("GPC.Model.Standards." + profile, true));
                }
                case "Concrete":
                {
                    string name = (string)n["name"];
                    var c = Database<ConcreteMaterialEN1992>(typeof(ConcreteMaterialEN1992Data), name);
                    if (!Same(c.Fck, (double)n["fck"])) throw new InvalidDataException("Calcestruzzo " + name + " diverso dal database GPC.");
                    return c;
                }
                case "Steel":
                {
                    string name = (string)n["name"];
                    var s = Database<SteelMaterialEN1992>(typeof(SteelMaterialEN1992Data), name);
                    s.StressStrainCurve = Enum.Parse<SteelMaterial.StressStrainCurveType>((string)n["curve"]);
                    if (!Same(s.E, (double)n["E"]) || !Same(s.Fyk, (double)n["fyk"]) || !Same(s.Fu, (double)n["fu"]))
                        throw new InvalidDataException("Acciaio " + name + " diverso dal database GPC.");
                    return s;
                }
                case "Axes":
                {
                    var o = Point(n["origin"]); var v1 = Point(n["v1"]); var v2 = Point(n["v2"]);
                    return new CoordinateSystem(new Point3d(o[0], o[1], o[2]), new Vector3d(v1[0], v1[1], v1[2]), new Vector3d(v2[0], v2[1], v2[2]), (string)n["name"] ?? "");
                }
                case "Options":
                {
                    var o = new SectionCheckerModelCode2010.SectionOptionsModelCode2010(
                        (CoordinateSystem)Decode("Axes", n["axes"]), Enum.Parse<SectionSolver.FailureAnalysisTypes>((string)n["criterion"]),
                        Enum.Parse<SectionSolver.FailureDomainTypes>((string)n["domain"]), Enum.Parse<SectionSolver.StressAnalysisTypes>((string)n["stress"]),
                        (double)n["psiRebar"], (double)n["psiTendon"], (bool)n["tensileConcrete"], (int)n["theta"])
                    {
                        DomainPointStrategy = Enum.Parse<SectionSolver.DomainPointStrategyTypes>((string)n["strategy"]),
                        StressAnalysisConcreteTension = Enum.Parse<ConcreteMaterial.TensionResponseDiagrams>((string)n["tension"])
                    };
                    return o;
                }
                case "Rebar":
                    return new ReinforcedConcreteRebar(new RebarSectionCircular((double)n["diameter"], (SteelMaterial)Decode("Steel", n["material"])),
                        new Point2d((double)n["x"], (double)n["y"]), (double)n["sigmaP"], (int)n["id"], (string)n["name"] ?? "");
                case "Section":
                {
                    var holes = n["holes"].AsArray().Select(h => PolygonFrom(h)).ToArray();
                    var section = new ReinforcedConcreteSection(new Shape2d(PolygonFrom(n["outline"]), holes.Length == 0 ? null : holes),
                        (ConcreteMaterial)Decode("Concrete", n["concrete"]), (string)n["name"] ?? "");
                    section.AddRebars(n["rebars"].AsArray().Select(r => (ReinforcedConcreteRebar)Decode("Rebar", r)).ToArray());
                    return section;
                }
                case "Checker":
                    return new SectionCheckerModelCode2010(new SectionCheckerAttribute((ReinforcedConcreteSection)Decode("Section", n["section"])),
                        (SectionCheckerModelCode2010.SectionOptionsModelCode2010)Decode("Options", n["options"]), (StandardModelCode2010)Decode("Standard", n["standard"]));
                case "Forces":
                    return new ResultBeamForces((double)n["N"], (double)n["V1"], (double)n["V2"], (double)n["T"], (double)n["M1"], (double)n["M2"],
                        (CoordinateSystem)Decode("Axes", n["axes"]), (int)n["id"], (string)n["name"] ?? "");
                case "Shear":
                    return new ShearOptions((SteelMaterial)Decode("Steel", n["linkSteel"]), (SteelMaterial)Decode("Steel", n["longitudinalSteel"]),
                        (double)n["width"], (double)n["depth"], (double)n["longitudinalArea"], (double)n["linkArea"], (double)n["spacing"],
                        (double)n["alpha"], (double?)n["cotTheta"], (double)n["lever"], (double)n["aggregate"], (double)n["eccentricity"]);
                case "Torsion":
                    return new TorsionOptions(new TorsionGeometry((double)n["Ak"], (double)n["uk"], (double)n["tef"]),
                        (SteelMaterial)Decode("Steel", n["linkSteel"]), (SteelMaterial)Decode("Steel", n["longitudinalSteel"]), (double)n["legArea"],
                        (double)n["spacing"], (double)n["longitudinalArea"], (double)n["cotTheta"], (bool)n["hollow"]);
                case "Sle":
                    return n.Deserialize<SleOptions>();
                case "Plate":
                    return new PlateDetailingData((double)n["secondary"], (double)n["spacing"], (bool)n["critical"]);
                case "Detailing":
                    return new DetailingOptions(Enum.Parse<MemberDetailingKind>((string)n["kind"]), (double)n["top"], (double)n["bottom"], (double)n["web"],
                        (double)n["linkDiameter"], (double)n["linkSpacing"], (int)n["legs"], (double)n["cover"], (double)n["aggregate"], (double)n["deviation"],
                        (double?)n["durability"], (bool)n["lap"], (bool)n["restrained"], (bool)n["ends"],
                        n["plate"] == null ? null : (PlateDetailingData)Decode("Plate", n["plate"]));
                default:
                    throw new InvalidDataException("Tipo salvato non riconosciuto: " + kind);
            }
        }

        private static T Database<T>(Type database, string name) where T : class
        {
            foreach (var property in database.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.PropertyType == typeof(T)))
                if (property.GetValue(null) is T material && string.Equals((material as Material)?.Name, name, StringComparison.Ordinal)) return material;
            throw new InvalidDataException($"Materiale '{name}' assente dal database GPC: non ricostruibile.");
        }
        private static bool Same(double a, double b) => Math.Abs(a - b) <= 1e-9 * Math.Max(1, Math.Abs(a));
        private static JsonArray Vector(params double[] values) => new(values.Select(v => (JsonNode)v).ToArray());
        private static double[] Point(JsonNode n) => n.AsArray().Select(v => (double)v).ToArray();
        private static JsonArray Polygon(Polygon2d polygon) => new(polygon.Points.Select(p => (JsonNode)Vector(p.X, p.Y)).ToArray());
        private static Polygon2d PolygonFrom(JsonNode n) => new(n.AsArray().Select(p => { var v = Point(p); return new Point2d(v[0], v[1]); }));
        internal static string Invariant(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    }
}
