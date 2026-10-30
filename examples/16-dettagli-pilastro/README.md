# 16 — Dettagli di un pilastro con conferme mancanti

Pilastro 300 × 500, 8Ø20, staffe Ø8/150 e N minimo=−600 kN. Il copriferro nominale è 30 mm; si lasciano intenzionalmente senza conferma il trattenimento delle barre compresse e i dettagli delle zone di estremità. Si omette anche cmin,dur.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Reinforcement Detailing produce una riga per ogni regola GPC, conservando Actual, Limit, unità, riferimento e verso del confronto. Gli esiti possono includere regole non soddisfatte o non valutabili: l'esempio serve a leggerle, non assume che il layout sia già approvato. Check Report espone la lista completa. Il controllo automatico richiede un insieme di regole non vuoto e valori disponibili finiti; nel pilastro richiede esplicitamente almeno una regola NotEvaluated, così la mancanza delle conferme non diventa un esito positivo.

| Quantità calcolata | Valore |
|---|---:|
| As totale [mm²] | 2513.27412 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Detailing | Detailing / ClearSpacing | Satisfied | — | 48 | 25 | mm |
| Detailing | Detailing / NominalCover | NotEvaluated | — | — | — |  |
| Detailing | Detailing / LongitudinalDiameter | Satisfied | — | 20 | 12 | mm |
| Detailing | Detailing / LongitudinalSpacing | NotSatisfied | — | 404 | 300 | mm |
| Detailing | Detailing / MinimumLongitudinal | Satisfied | — | 2513.27412 | 450 | mm2 |
| Detailing | Detailing / MaximumLongitudinal | Satisfied | — | 2513.27412 | 6000 | mm2 |
| Detailing | Detailing / LinkDiameter | Satisfied | — | 8 | 6 | mm |
| Detailing | Detailing / LinkSpacing | Satisfied | — | 150 | 240 | mm |
| Detailing | Detailing / BarsHeldByLinks | NotEvaluated | — | — | — |  |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): **Concrete Checker Options**. Gli altri componenti del grafo conservano il GUID di 0.2.2.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/16-dettagli-pilastro.json`); soglia 1E-09 relativo, 1E-10 assoluto.

Nessuna variazione numerica oltre la soglia e nessun esito diverso.

## Componenti e collegamenti

Componenti numerati come sulla tela, da sinistra a destra e dall'alto in basso. I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- tutti i valori di default

**02 — Standard**

- tutti i valori di default

**03 — Detailing Options**

- Member kind = `1`
- Top tension width = `300`
- Bottom tension width = `300`
- Web width = `300`
- Link diameter = `8`
- Link spacing = `150`
- Nominal cover = `30`

**04 — Coordinate System**

- Name = `Global XY`
- Origin = `0,0,0`
- P1 = `1,0,0`
- P2 = `0,1,0`

**05 — Rectangular RC Section**

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

**06 — Concrete Checker Options**

- Failure Analysis Types = `0`
- Coordinate System ← 04 Coordinate System / Coordinate System

**07 — Concrete Checker**

- Reinforced Concrete Section ← 05 Rectangular RC Section / RC section
- Standard ← 02 Standard / Standard
- Options ← 06 Concrete Checker Options / Checker Options

**08 — Reinforcement Detailing**

- Minimum axial force = `-600`
- Section ID = `S`
- Checker ← 07 Concrete Checker / Concrete Checker
- Options ← 03 Detailing Options / Options

**09 — Check Report**

- Results ← 08 Reinforcement Detailing / Results

**10 — Check Result**

- Result ← 08 Reinforcement Detailing / Results
