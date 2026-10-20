# 08 — Taglio X e Y in un unico batch

Stessa sezione rettangolare; staffe Ø8 a due bracci, passo 150 mm, B450C, cotθ=1.5. Un unico Batch Section Checks riceve le opzioni distinte X e Y e due combinazioni complete.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Asw = 2π8²/4. Per X si assegnano bw=500, d=252 mm e Asl=2π20²/4; per Y bw=300, d=452 mm e Asl=4π20²/4. Le domande sono V1 per X e V2 per Y, mentre i momenti associati sono M2 e M1. Il controllo automatico verifica quattro risultati e la corretta domanda su entrambe le direzioni. Modificare un'opzione X non deve cambiare il significato dell'opzione Y.

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| SLU1 | Shear X | Satisfied | 0.672502148 | 60 | 89.2190459 | kN |
| SLU1 | Shear Y | Satisfied | 0.562402239 | 90 | 160.027812 | kN |
| SLU2 | Shear X | NotSatisfied | 1.12083691 | 100 | 89.2190459 | kN |
| SLU2 | Shear Y | Satisfied | 0.437423963 | 70 | 160.027812 | kN |

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

- Name = `SLU1`
- Axial Force = `-300`
- Shear X = `60`
- Shear Y = `90`
- Torsion = `0`
- Bending X = `45`
- Bending Y = `15`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**10 — Force component**

- Name = `SLU2`
- Axial Force = `-200`
- Shear X = `-100`
- Shear Y = `70`
- Torsion = `0`
- Bending X = `70`
- Bending Y = `-25`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**11 — Batch Section Checks**

- Section ID = `S`
- N-Mx-My = `False`
- Run = `True`
- Checker ← 06 Concrete Checker / Concrete Checker
- Forces ← 09 Force component / Force
- Forces ← 10 Force component / Force
- Shear X ← 07 Shear Options / Options
- Shear Y ← 08 Shear Options / Options

**12 — Check Envelope**

- Results ← 11 Batch Section Checks / Results

**13 — Check Report**

- Results ← 11 Batch Section Checks / Results

**14 — Check Result**

- Result ← 11 Batch Section Checks / Results
