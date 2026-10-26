# 07 — Taglio senza staffe: caso valutabile e caso escluso

Rettangolo 300 × 500 mm, 8Ø20. Direzione Y: bw=300, d=452 mm, Asl=4π20²/4. Si confrontano un caso compresso e uno teso; Asw=0 esclude il contributo delle staffe.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.1 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Nel caso compresso N=−300 kN e Vy=60 kN. Con σcp=300000/150000=2 MPa, k=1+√(200/452), ρl=Asl/(bw d), il controllo indipendente usa VRd=max{[0.18/1.5 k(100ρl fck)^(1/3)+0.15σcp]bw d; [0.035 k^(3/2)√fck+0.15σcp]bw d}. Nel secondo caso N=+100 kN: il profilo GPC restituisce NotEvaluated, che deve impedire un inviluppo complessivo positivo.

| Quantità calcolata | Valore |
|---|---:|
| VRd analitico [kN] | 122.76464 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Compresso | Shear Y | Satisfied | 0.488740078 | 60 | 122.76464 | kN |
| Teso | Shear Y | NotEvaluated | — | 60 | — | kN |

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

- Web width = `300`
- Effective depth = `452`
- Longitudinal area = `1256.6370614359173`
- Link area = `0`
- Spacing = `150`
- Link angle = `90`
- Cot theta = `1.5`
- Lever factor = `0.9`
- Aggregate = `20`
- Axial eccentricity = `0`
- Link steel ← 01 Database Rebar Steel / Rebar Steel Material
- Longitudinal steel ← 01 Database Rebar Steel / Rebar Steel Material

**08 — Force component**

- Name = `Compresso`
- Axial Force = `-300`
- Shear X = `0`
- Shear Y = `60`
- Torsion = `0`
- Bending X = `45`
- Bending Y = `0`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**09 — Force component**

- Name = `Teso`
- Axial Force = `100`
- Shear X = `0`
- Shear Y = `60`
- Torsion = `0`
- Bending X = `45`
- Bending Y = `0`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**10 — Batch Section Checks**

- Section ID = `S`
- N-Mx-My = `False`
- Run = `True`
- Checker ← 06 Concrete Checker / Concrete Checker
- Forces ← 08 Force component / Force
- Forces ← 09 Force component / Force
- Shear Y ← 07 Shear Options / Options

**11 — Check Report**

- Results ← 10 Batch Section Checks / Results

**12 — Check Result**

- Result ← 10 Batch Section Checks / Results
