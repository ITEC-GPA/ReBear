# 17 — Soletta: armatura principale e secondaria

Striscia di soletta larga 1000 mm e alta 250 mm, sei Ø12 per faccia, cbar=30 mm, senza staffe. Armatura secondaria totale delle due facce 500 mm²/m, passo 200 mm, regione non critica. Profilo NTC2018; cmin,dur=20 e Δcdev=10 mm.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Reinforcement Detailing produce una riga per ogni regola GPC, conservando Actual, Limit, unità, riferimento e verso del confronto. Gli esiti possono includere regole non soddisfatte o non valutabili: l'esempio serve a leggerle, non assume che il layout sia già approvato. Check Report espone la lista completa. Il controllo automatico richiede un insieme di regole non vuoto e valori disponibili finiti; nel pilastro richiede esplicitamente almeno una regola NotEvaluated, così la mancanza delle conferme non diventa un esito positivo.

| Quantità calcolata | Valore |
|---|---:|
| As totale [mm²] | 1357.16803 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Detailing | Detailing / ClearSpacing | Satisfied | — | 166 | 25 | mm |
| Detailing | Detailing / NominalCover | Satisfied | — | 30 | 30 | mm |
| Detailing | Detailing / BarCoverMargin | Satisfied | — | 0 | 0 | mm |
| Detailing | Detailing / MinimumTension:Bottom | Satisfied | — | 678.584013 | 358.132196 | mm2 |
| Detailing | Detailing / MaximumTension:Bottom | Satisfied | — | 678.584013 | 10000 | mm2 |
| Detailing | Detailing / MinimumTension:Top | Satisfied | — | 678.584013 | 358.132196 | mm2 |
| Detailing | Detailing / MaximumTension:Top | Satisfied | — | 678.584013 | 10000 | mm2 |
| Detailing | Detailing / MainSpacing | Satisfied | — | 185.6 | 400 | mm |
| Detailing | Detailing / SecondaryReinforcement | Satisfied | — | 500 | 271.433605 | mm2/m |
| Detailing | Detailing / SecondaryFaceDistribution | NotEvaluated | — | — | — |  |
| Detailing | Detailing / SecondarySpacing | Satisfied | — | 200 | 450 | mm |
| Detailing | Detailing / EdgesSupportsPunching | NotEvaluated | — | — | — |  |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): **Concrete Checker Options**. Gli altri componenti del grafo conservano il GUID di 0.2.2.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/17-dettagli-soletta.json`); soglia 1E-09 relativo, 1E-10 assoluto.

Nessuna variazione numerica oltre la soglia e nessun esito diverso.

## Componenti e collegamenti

I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- Type = `1`
- Behaviour = `0`

**02 — Rectangular RC Section**

- Width = `1000`
- Height = `250`
- Link cover = `30`
- Link diameter = `0`
- Bar diameter = `12`
- Top bars = `6`
- Bottom bars = `6`
- Side bars per face = `0`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**03 — Standard**

- Type = `0`

**04 — Concrete Checker Options**

- Failure Domain Types = `1`
- Failure Analysis Types = `0`
- Domain Point Strategy Types = `0`
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

- Member kind = `2`
- Top tension width = `1000`
- Bottom tension width = `1000`
- Web width = `1000`
- Link diameter = `0`
- Link spacing = `0`
- Link legs = `0`
- Nominal cover = `30`
- Aggregate = `20`
- Cover deviation = `10`
- Durability minimum = `20`
- Lap zone = `False`
- Compression bars restrained = `True`
- End zones confirmed = `True`
- Plate data ← 08 Plate Detailing Options / Plate data

**08 — Plate Detailing Options**

- Secondary steel = `500`
- Secondary spacing = `200`
- Critical region = `False`

**09 — Reinforcement Detailing**

- Minimum axial force = `0`
- Section ID = `S`
- Checker ← 06 Concrete Checker / Concrete Checker
- Options ← 07 Detailing Options / Options

**10 — Check Report**

- Results ← 09 Reinforcement Detailing / Results

**11 — Check Result**

- Result ← 09 Reinforcement Detailing / Results
