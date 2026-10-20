# 09 — Torsione con due tagli concomitanti

Sezione 300 × 500 mm con 8Ø20. Si assegna un profilo efficace Ak=80000 mm², uk=1200 mm, tef=100 mm. Staffe chiuse Ø8/150, cotθ=1.5; area longitudinale aggiuntiva per torsione 800 mm². È un dato disponibile assegnato e richiede conferma della sua effettiva disposizione.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Azioni: N=−300 kN, Vx=20 kN, Vy=30 kN, T=5 kNm, Mx=45 kNm e My=15 kNm. La torsione usa l'area di un braccio At=π8²/4; le opzioni dei tagli usano due bracci. Il controllo richiama GPC con entrambe le direzioni e confronta il rapporto con quello della sola torsione, a parità degli altri dati. L'interazione non deve migliorare l'utilizzo. Geometria efficace e quota aggiuntiva di armatura non vengono inferite dal builder.

| Quantità calcolata | Valore |
|---|---:|
| η sola torsione [-] | 0.1796875 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| SLU-TV | Torsion + shear | Satisfied | 0.383046015 | 5 | 27.826087 | kNm |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Componenti e collegamenti

I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- Type = `1`
- Behaviour = `0`

**02 — Rectangular RC Section**

- Width = `300`
- Height = `500`
- Link cover = `30`
- Link diameter = `8`
- Bar diameter = `20`
- Top bars = `4`
- Bottom bars = `4`
- Side bars per face = `0`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**03 — Standard**

- Type = `0`

**04 — Concrete Checker Options**

- Failure Domain Types = `1`
- Failure Analysis Types = `0`
- Domain Point Strategy Types = `1`
- Stress Analysis Types = `1`
- ψ rebar = `1.5`
- ψ tendon = `0`
- Consider tensile concrete = `False`
- Coordinate System ← 05 Coordinate System / Coordinate System

**05 — Coordinate System**

- Name = `Global XY`
- Origin = `0,0,0`
- P1 = `1,0,0`
- P2 = `0,1,0`

**06 — Concrete Checker**

- Reinforced Concrete Section ← 02 Rectangular RC Section / RC section
- Standard ← 03 Standard / Standard
- Options ← 04 Concrete Checker Options / Checker Options

**07 — Shear Options**

- Web width = `500`
- Effective depth = `252`
- Longitudinal area = `628.3185307179587`
- Link area = `100.53096491487338`
- Spacing = `150`
- Link angle = `90`
- Cot theta = `1.5`
- Lever factor = `0.9`
- Aggregate = `20`
- Axial eccentricity = `0`
- Link steel ← 01 Database Rebar Steel / Rebar Steel Material
- Longitudinal steel ← 01 Database Rebar Steel / Rebar Steel Material

**08 — Shear Options**

- Web width = `300`
- Effective depth = `452`
- Longitudinal area = `1256.6370614359173`
- Link area = `100.53096491487338`
- Spacing = `150`
- Link angle = `90`
- Cot theta = `1.5`
- Lever factor = `0.9`
- Aggregate = `20`
- Axial eccentricity = `0`
- Link steel ← 01 Database Rebar Steel / Rebar Steel Material
- Longitudinal steel ← 01 Database Rebar Steel / Rebar Steel Material

**09 — Force component**

- Name = `SLU-TV`
- Axial Force = `-300`
- Shear X = `20`
- Shear Y = `30`
- Torsion = `5`
- Bending X = `45`
- Bending Y = `15`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**10 — Torsion Options**

- Enclosed area = `80000`
- Midline perimeter = `1200`
- Effective thickness = `100`
- One leg area = `50.26548245743669`
- Spacing = `150`
- Torsion longitudinal area = `800`
- Cot theta = `1.5`
- Hollow, both faces reinforced = `False`
- Closed links and distributed bars confirmed = `True`
- Link steel ← 01 Database Rebar Steel / Rebar Steel Material
- Longitudinal steel ← 01 Database Rebar Steel / Rebar Steel Material

**11 — Torsion and Shear**

- Section ID = `S`
- Checker ← 06 Concrete Checker / Concrete Checker
- Force ← 09 Force component / Force
- Torsion options ← 10 Torsion Options / Options
- Shear X options ← 07 Shear Options / Options
- Shear Y options ← 08 Shear Options / Options

**12 — Check Report**

- Results ← 11 Torsion and Shear / Result

**13 — Check Result**

- Result ← 11 Torsion and Shear / Result
