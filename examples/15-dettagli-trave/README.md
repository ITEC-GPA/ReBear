# 15 — Dettagli di armatura di una trave

Trave rettangolare 300 × 500, 8Ø20, staffe Ø8 a due bracci /150, cnom=30 mm e aggregato 20 mm. Le larghezze tese superiore/inferiore e dell'anima sono 300 mm. Si assegna cmin,dur=20 mm e Δcdev=10 mm, senza zona di sovrapposizione.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Reinforcement Detailing produce una riga per ogni regola GPC, conservando Actual, Limit, unità, riferimento e verso del confronto. Gli esiti possono includere regole non soddisfatte o non valutabili: l'esempio serve a leggerle, non assume che il layout sia già approvato. Check Report espone la lista completa. Il controllo automatico richiede un insieme di regole non vuoto e valori disponibili finiti; nel pilastro richiede esplicitamente almeno una regola NotEvaluated, così la mancanza delle conferme non diventa un esito positivo.

| Quantità calcolata | Valore |
|---|---:|
| As totale [mm²] | 2513.27412 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Detailing | Detailing / ClearSpacing | Satisfied | — | 48 | 25 | mm |
| Detailing | Detailing / NominalCover | Satisfied | — | 30 | 30 | mm |
| Detailing | Detailing / BarCoverMargin | Satisfied | — | 8 | 0 | mm |
| Detailing | Detailing / MinimumTension:Bottom | Satisfied | — | 1256.63706 | 226.928625 | mm2 |
| Detailing | Detailing / MaximumTension:Bottom | Satisfied | — | 1256.63706 | 6000 | mm2 |
| Detailing | Detailing / MinimumLinks:Bottom | Satisfied | — | 670.206433 | 450 | mm2/m |
| Detailing | Detailing / LinkSpacing:Bottom | Satisfied | — | 150 | 333.333333 | mm |
| Detailing | Detailing / MinimumTension:Top | Satisfied | — | 1256.63706 | 226.928625 | mm2 |
| Detailing | Detailing / MaximumTension:Top | Satisfied | — | 1256.63706 | 6000 | mm2 |
| Detailing | Detailing / MinimumLinks:Top | Satisfied | — | 670.206433 | 450 | mm2/m |
| Detailing | Detailing / LinkSpacing:Top | Satisfied | — | 150 | 333.333333 | mm |
| Detailing | Detailing / CompressionBarRestraint | Satisfied | — | 150 | 300 | mm |

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

**07 — Detailing Options**

- Member kind = `0`
- Top tension width = `300`
- Bottom tension width = `300`
- Web width = `300`
- Link diameter = `8`
- Link spacing = `150`
- Link legs = `2`
- Nominal cover = `30`
- Aggregate = `20`
- Cover deviation = `10`
- Durability minimum = `20`
- Lap zone = `False`
- Compression bars restrained = `True`
- End zones confirmed = `True`

**08 — Reinforcement Detailing**

- Minimum axial force = `0`
- Section ID = `S`
- Checker ← 06 Concrete Checker / Concrete Checker
- Options ← 07 Detailing Options / Options

**09 — Check Report**

- Results ← 08 Reinforcement Detailing / Results

**10 — Check Result**

- Result ← 08 Reinforcement Detailing / Results
