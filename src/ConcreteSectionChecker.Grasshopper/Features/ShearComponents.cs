using System;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Torsion;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Standards;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Features
{
    public sealed class ShearOptionsComponent : ReBearComponent
    {
        public ShearOptionsComponent() : base("Shear Options", "VOpt",
            "Parametri di una direzione nel riferimento del checker. Acciai indipendenti; d esplicito in mm.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0005");
        protected override Port[] Inputs => new[] { G("Link steel", "Sw", "Acciaio delle staffe."),
            G("Longitudinal steel", "Sl", "Acciaio delle barre longitudinali: Es per i modelli GPC."),
            N("Web width", "bw", "Larghezza resistente ortogonale al taglio, mm."),
            N("Effective depth", "d", "Altezza utile lungo la direzione del taglio, mm."),
            N("Longitudinal area", "Asl", "Armatura longitudinale tesa e ancorata, mm²."),
            N("Link area", "Asw", "Somma delle aree dei bracci nella direzione, mm²; 0 senza staffe."),
            N("Spacing", "s", "Passo mm; positivo anche senza staffe.", 200),
            N("Link angle", "α", "Gradi [45,90].", 90), N("Cot theta", "cotθ", "0 = scelta GPC; altrimenti valore assegnato.", 0),
            N("Lever factor", "z/d", "Rapporto esplicito (0,0.9].", .9), N("Aggregate", "dg", "Diametro massimo aggregato, mm.", 20),
            N("Axial eccentricity", "Δe", "Eccentricità MC2010 rispetto a metà z, mm; positiva verso il corrente teso.", 0) };
        protected override Port[] Outputs => new[] { G("Options", "O", "Opzioni taglio di una direzione.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            double cot = Finite(d, 8, nonNegative: true);
            d.SetData(0, Data(new ShearOptions(Required<SteelMaterial>(d, 0), Required<SteelMaterial>(d, 1),
                Finite(d, 2), Finite(d, 3), Finite(d, 4), Finite(d, 5), Finite(d, 6), Finite(d, 7), cot == 0 ? null : cot,
                Finite(d, 9), Finite(d, 10), Finite(d, 11))));
        }
    }

    /// <summary>Replaces the 0.2 "Rectangular Section Shear Check" and "… No stirrup" geometry inputs.</summary>
    public sealed class RectangularShearOptionsComponent : ReBearComponent
    {
        public RectangularShearOptionsComponent() : base("Rectangular Shear Options", "VRect",
            "Opzioni di taglio X e Y di una sezione rettangolare b × h (geometria dei componenti di taglio 0.2). Diametro staffe 0 = senza staffe.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000001");
        protected override Port[] Inputs => new[] { G("Standard", "S", "Norma: braccio z/d del profilo GPC."),
            G("Link steel", "Sw", "Acciaio delle staffe."), G("Longitudinal steel", "Sl", "Acciaio delle barre longitudinali."),
            N("Base", "B", "Dimensione X, mm."), N("Height", "H", "Dimensione Y, mm."),
            N("Bar cover", "c", "Copriferro alla superficie delle barre longitudinali, mm."),
            N("Link diameter", "Øw", "Diametro staffe, mm; 0 = nessuna staffa.", 8),
            I("Legs X", "nX", "Bracci efficaci per il taglio X.", 2), I("Legs Y", "nY", "Bracci efficaci per il taglio Y.", 2),
            N("Spacing", "s", "Passo delle staffe, mm.", 150), N("Link angle X", "αX", "Inclinazione staffe per X, gradi [45,90].", 90),
            N("Link angle Y", "αY", "Inclinazione staffe per Y, gradi [45,90].", 90),
            N("Bar diameter X", "ØX", "Diametro barre tese per il taglio X, mm.", 20), N("Bar diameter Y", "ØY", "Diametro barre tese per il taglio Y, mm.", 20),
            I("Bar count X", "nlX", "Barre tese per il taglio X.", 2), I("Bar count Y", "nlY", "Barre tese per il taglio Y.", 2),
            N("Cot theta", "cotθ", "0 = scelta GPC; altrimenti valore assegnato.", 0), N("Aggregate", "dg", "Diametro massimo aggregato, mm.", 20) };
        protected override Port[] Outputs => new[] { G("Options X", "X", "Taglio X: bw = H, d = B − c − ØX/2."),
            G("Options Y", "Y", "Taglio Y: bw = B, d = H − c − ØY/2.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            double cot = Finite(d, 16, nonNegative: true);
            var r = ShearGeometry.Rectangular(Required<StandardModelCode2010>(d, 0), Required<SteelMaterial>(d, 1), Required<SteelMaterial>(d, 2),
                Finite(d, 3), Finite(d, 4), Finite(d, 5), Finite(d, 6), Required<int>(d, 7), Required<int>(d, 8), Finite(d, 9), Finite(d, 10), Finite(d, 11),
                Finite(d, 12), Finite(d, 13), Required<int>(d, 14), Required<int>(d, 15), cot == 0 ? null : cot, Finite(d, 17));
            d.SetData(0, Data(r.X)); d.SetData(1, Data(r.Y));
        }
    }

    /// <summary>Replaces the 0.2 "Circular Section Shear Check" geometry inputs; use Shear Check with Direction = 2.</summary>
    public sealed class CircularShearOptionsComponent : ReBearComponent
    {
        public CircularShearOptionsComponent() : base("Circular Shear Options", "VCirc",
            "Opzioni di taglio di una pila circolare (modello GPC NTC 2018 per pile circolari); usare Shear Check con Direction = 2 (risultante).") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000002");
        protected override Port[] Inputs => new[] { G("Standard", "S", "Norma NTC 2018."), G("Link steel", "Sw", "Acciaio delle staffe."),
            G("Longitudinal steel", "Sl", "Acciaio delle barre longitudinali."), N("Diameter", "D", "Diametro, mm."),
            N("Link cover", "c", "Copriferro alla superficie esterna della staffa, mm."), N("Link diameter", "Øw", "Diametro staffe, mm.", 10),
            I("Legs", "n", "Bracci efficaci.", 2), N("Spacing", "s", "Passo delle staffe, mm.", 150), N("Link angle", "α", "Inclinazione, gradi [45,90].", 90),
            N("Bar diameter", "Ø", "Diametro barre longitudinali, mm.", 20), N("Cot theta", "cotθ", "0 = scelta GPC; altrimenti valore assegnato.", 0),
            N("Aggregate", "dg", "Diametro massimo aggregato, mm.", 20) };
        protected override Port[] Outputs => new[] { G("Options", "O", "bw = D, d = D − c − Øw − Ø/2, z/d GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            double cot = Finite(d, 10, nonNegative: true);
            d.SetData(0, Data(ShearGeometry.Circular(Required<StandardModelCode2010>(d, 0), Required<SteelMaterial>(d, 1), Required<SteelMaterial>(d, 2),
                Finite(d, 3), Finite(d, 4), Finite(d, 5), Required<int>(d, 6), Finite(d, 7), Finite(d, 8), Finite(d, 9), cot == 0 ? null : cot, Finite(d, 11))));
        }
    }

    public sealed class ShearCheckComponent : ReBearComponent
    {
        public ShearCheckComponent() : base("Shear Check", "Shear",
            "Verifica a taglio GPC di una direzione con risultato strutturato: 0 = X (V1/M2), 1 = Y (V2/M1), 2 = risultante (sezioni circolari).") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0006");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Checker della sezione."), G("Force", "F", "Combinazione, N/Nmm."),
            G("Options", "O", "Da Shear Options, Rectangular Shear Options o Circular Shear Options."),
            I("Direction", "A", "0 = X (V1/M2), 1 = Y (V2/M1), 2 = risultante √(V1²+V2²).", 1, Named("X", "Y", "Resultant")),
            T("Section ID", "ID", "Identificativo per gli inviluppi.", "Section") };
        protected override Port[] Outputs => new[] { G("Result", "R", "Risultato strutturato ReBear.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var f = Required<ResultBeamForces>(d, 1); string id = Read(d, 4, "Section"); int axis = Choice(d, 3, 3);
            Result(d, SectionChecks.Try(id, LoadEnvelopes.Name(f, d.Iteration), ShearDirections.Name(axis), () =>
                AdvancedShearChecks.Shear(Required<SectionCheckerModelCode2010>(d, 0), f, Required<ShearOptions>(d, 2), axis, id, d.Iteration)));
        }
    }

    public sealed class TorsionOptionsComponent : ReBearComponent
    {
        public TorsionOptionsComponent() : base("Torsion Options", "TOpt",
            "Profilo sottile esplicito e staffe chiuse a 90°. Area longitudinale aggiuntiva a quella per flessione.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0007");
        protected override Port[] Inputs => new[] { N("Enclosed area", "Ak", "Area racchiusa dalla linea media, mm²."),
            N("Midline perimeter", "uk", "Perimetro linea media, mm."), N("Effective thickness", "tef", "Spessore efficace, mm."),
            G("Link steel", "Sw", "Acciaio delle staffe chiuse."), G("Longitudinal steel", "Sl", "Acciaio longitudinale per torsione."),
            N("One leg area", "At", "Area di un solo braccio di staffa chiusa, mm²."),
            N("Spacing", "s", "Passo staffe mm."), N("Torsion longitudinal area", "ΣAsl", "Area aggiuntiva disponibile per torsione, distribuita sul profilo, mm²."),
            N("Cot theta", "cotθ", "Inclinazione comune a taglio/torsione.", 1),
            B("Hollow, both faces reinforced", "H", "Sezione cava con armature su entrambe le facce delle pareti."),
            B("Closed links and distributed bars confirmed", "OK", "Confermare staffe chiuse e barre longitudinali distribuite sul profilo, con barre agli angoli.") };
        protected override Port[] Outputs => new[] { G("Options", "O", "Opzioni torsione.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            if (!Read(d, 10, false)) throw new ArgumentException("Confermare la disposizione delle armature richiesta dal modello di torsione.");
            d.SetData(0, Data(new TorsionOptions(new TorsionGeometry(Finite(d, 0), Finite(d, 1), Finite(d, 2)),
                Required<SteelMaterial>(d, 3), Required<SteelMaterial>(d, 4), Finite(d, 5), Finite(d, 6),
                Finite(d, 7), Finite(d, 8), Read(d, 9, false))));
        }
    }

    public sealed class TorsionShearCheckComponent : ReBearComponent
    {
        public TorsionShearCheckComponent() : base("Torsion and Shear", "TV", "Torsione e interazioni GPC con entrambi i tagli concomitanti.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0008");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Checker della sezione."), G("Force", "F", "Combinazione completa, N/Nmm."),
            G("Torsion options", "T", "Da Torsion Options."), G("Shear X options", "X", "Richieste se Vx non nullo.", true),
            G("Shear Y options", "Y", "Richieste se Vy non nullo.", true), T("Section ID", "ID", "Identificativo per gli inviluppi.", "Section") };
        protected override Port[] Outputs => new[] { G("Result", "R", "Utilizzo massimo fra torsione e interazioni; dettagli in Check Result.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var f = Required<ResultBeamForces>(d, 1); string id = Read(d, 5, "Section");
            Result(d, SectionChecks.Try(id, LoadEnvelopes.Name(f, d.Iteration), "Torsion + shear", () =>
                AdvancedShearChecks.Torsion(Required<SectionCheckerModelCode2010>(d, 0), f, Required<TorsionOptions>(d, 2),
                    Read<ShearOptions>(d, 3), Read<ShearOptions>(d, 4), id, d.Iteration)));
        }
    }
}
