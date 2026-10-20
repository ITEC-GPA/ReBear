using System;
using ConcreteSectionChecker.Adapters;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Torsion;
using GPC.Model.Materials;
using GPC.Model.Results;
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
            double cot = Read<double>(d, 8);
            if (!double.IsFinite(cot) || cot < 0) throw new ArgumentException("cotθ deve essere finito e non negativo.");
            d.SetData(0, new ShearOptions(Required<SteelMaterial>(d, 0), Required<SteelMaterial>(d, 1),
                Read<double>(d, 2), Read<double>(d, 3), Read<double>(d, 4), Read<double>(d, 5),
                Read<double>(d, 6), Read<double>(d, 7), cot == 0 ? null : cot, Read<double>(d, 9), Read<double>(d, 10), Read<double>(d, 11)));
        }
    }
    public sealed class ShearCheckComponent : ReBearComponent
    {
        public ShearCheckComponent() : base("Shear Check v2", "Shear", "Verifica GPC con risultato strutturato; mantiene il nome della combinazione.") { }
        public override Guid ComponentGuid => new("98dc4c10-e882-41ed-ae13-208c325c0006");
        protected override Port[] Inputs => new[] { G("Checker", "C", "Checker della sezione."), G("Force", "F", "Combinazione, N/Nmm."),
            G("Options", "O", "Da Shear Options."), I("Direction", "A", "0 = X (V1/M2), 1 = Y (V2/M1).", 1),
            T("Section ID", "ID", "Identificativo per gli inviluppi.", "Section") };
        protected override Port[] Outputs => new[] { G("Result", "R", "Risultato strutturato ReBear.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var f = Required<ResultBeamForces>(d, 1); string id = Read<string>(d, 4); int axis = Read<int>(d, 3);
            try { Result(d, AdvancedShearChecks.Shear(Required<SectionCheckerModelCode2010>(d, 0), f, Required<ShearOptions>(d, 2), axis, id)); }
            catch (Exception e) when (e is ArgumentException or NotSupportedException or InvalidOperationException or ArithmeticException)
            { Result(d, CheckResult.Pending(id, LoadEnvelopes.Name(f), axis == 0 ? "Shear X" : "Shear Y", e)); }
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
            if (!Read<bool>(d, 10)) throw new ArgumentException("Confermare la disposizione delle armature richiesta dal modello di torsione.");
            d.SetData(0, new TorsionOptions(new TorsionGeometry(Read<double>(d, 0), Read<double>(d, 1), Read<double>(d, 2)),
                Required<SteelMaterial>(d, 3), Required<SteelMaterial>(d, 4), Read<double>(d, 5), Read<double>(d, 6),
                Read<double>(d, 7), Read<double>(d, 8), Read<bool>(d, 9)));
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
            var f = Required<ResultBeamForces>(d, 1); string id = Read<string>(d, 5);
            try { Result(d, AdvancedShearChecks.Torsion(Required<SectionCheckerModelCode2010>(d, 0), f,
                Required<TorsionOptions>(d, 2), Read<ShearOptions>(d, 3), Read<ShearOptions>(d, 4), id)); }
            catch (Exception e) when (e is ArgumentException or NotSupportedException or InvalidOperationException or ArithmeticException)
            { Result(d, CheckResult.Pending(id, LoadEnvelopes.Name(f), "Torsion + shear", e)); }
        }
    }
}
