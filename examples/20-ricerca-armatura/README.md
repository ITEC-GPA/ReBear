# 20 — Scelta della minima armatura fra tre candidati

Tre sezioni 300 × 500 mm, tutte con quattro barre superiori e quattro inferiori; diametri candidati 16, 20 e 24 mm. C30/37, B450C, copriferro staffa 30 e staffa Ø8 mm. SLU1: N=−300 kN, Mx=180 e My=5 kNm; SLU2: N=−200 kN, Mx=150 e My=10 kNm. Obiettivo η≤0.9.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Per ogni candidato si eseguono entrambe le verifiche N-Mx-My. Si scartano quelli con esiti non valutati/non soddisfatti o utilizzo superiore a 0.9, poi si seleziona l'area minima fra i rimanenti. Il controllo indipendente sul risultato ricalcola questo filtro e confronta indice e area selezionati. La ricerca è discreta e limitata ai tre layout assegnati: non ottimizza diametri continui e non verifica automaticamente taglio, SLE o dettagli.

| Quantità calcolata | Valore |
|---|---:|
| Indice selezionato (base zero) | 1 |
| As selezionata [mm²] | 2513.27412 |
| Diametro selezionato [mm] | 20 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| SLU1 | N-Mx-My | Satisfied | 0.935161059 | — | — |  |
| SLU2 | N-Mx-My | Satisfied | 0.868815427 | — | — |  |
| SLU1 | N-Mx-My | Satisfied | 0.687315285 | — | — |  |
| SLU2 | N-Mx-My | Satisfied | 0.62054976 | — | — |  |
| SLU1 | N-Mx-My | Satisfied | 0.520995522 | — | — |  |
| SLU2 | N-Mx-My | Satisfied | 0.462551963 | — | — |  |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): **Concrete Checker Options**. Gli altri componenti del grafo conservano il GUID di 0.2.2.

Il checker usa la strategia Iterative del punto di dominio, default di ReBear 0.3 (0.2.2: Intersection, che interpola il dominio discretizzato e costa 0.4–0.6 s per checker). I punti resistenti sono risolti direttamente sulla legge della sezione invece che interpolati sul dominio discretizzato; le differenze numeriche sono nella tabella, gli esiti (Satisfied/NotSatisfied) e le scelte non cambiano.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/20-ricerca-armatura.json`); soglia 1E-09 relativo, 1E-10 assoluto.

| Grandezza | 0.2.2 | 0.3 | Δ relativo (ass. se un valore è nullo) |
|---|---:|---:|---:|
| SLU1 / N-Mx-My [1] η | 0.9355373 | 0.935161059 | 4.0E-4 |
| SLU1 / N-Mx-My [2] η | 0.687471856 | 0.687315285 | 2.3E-4 |
| SLU1 / N-Mx-My [3] η | 0.521183698 | 0.520995522 | 3.6E-4 |
| SLU2 / N-Mx-My [1] η | 0.869273737 | 0.868815427 | 5.3E-4 |
| SLU2 / N-Mx-My [2] η | 0.620693418 | 0.62054976 | 2.3E-4 |
| SLU2 / N-Mx-My [3] η | 0.462639205 | 0.462551963 | 1.9E-4 |

## Componenti e collegamenti

Componenti numerati come sulla tela, da sinistra a destra e dall'alto in basso. I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Standard**

- tutti i valori di default

**01 — Coordinate System**

- Name = `Global XY`
- Origin = `0,0,0`
- P1 = `1,0,0`
- P2 = `0,1,0`

**02 — Database Concrete**

- Type = `3`

**03 — Database Rebar Steel**

- tutti i valori di default

**04 — Force component**

- Name = `SLU1`
- Axial Force = `-300`
- Bending X = `180`
- Bending Y = `5`
- Coordinate System ← 01 Coordinate System / Coordinate System

**05 — Force component**

- Name = `SLU2`
- Axial Force = `-200`
- Bending X = `150`
- Bending Y = `10`
- Coordinate System ← 01 Coordinate System / Coordinate System

**06 — Rectangular RC Section**

- Width = `300`
- Height = `500`
- Link cover = `30`
- Link diameter = `8`
- Bar diameter = `16`
- Top bars = `4`
- Bottom bars = `4`
- Side bars per face = `0`
- Concrete ← 02 Database Concrete / Concrete Material
- Steel ← 03 Database Rebar Steel / Rebar Steel Material

**07 — Concrete Checker Options**

- Failure Analysis Types = `0`
- Coordinate System ← 01 Coordinate System / Coordinate System

**08 — Rectangular RC Section**

- Width = `300`
- Height = `500`
- Link cover = `30`
- Link diameter = `8`
- Bar diameter = `20`
- Top bars = `4`
- Bottom bars = `4`
- Side bars per face = `0`
- Concrete ← 02 Database Concrete / Concrete Material
- Steel ← 03 Database Rebar Steel / Rebar Steel Material

**09 — Rectangular RC Section**

- Width = `300`
- Height = `500`
- Link cover = `30`
- Link diameter = `8`
- Bar diameter = `24`
- Top bars = `4`
- Bottom bars = `4`
- Side bars per face = `0`
- Concrete ← 02 Database Concrete / Concrete Material
- Steel ← 03 Database Rebar Steel / Rebar Steel Material

**10 — Concrete Checker**

- Reinforced Concrete Section ← 06 Rectangular RC Section / RC section
- Standard ← 00 Standard / Standard
- Options ← 07 Concrete Checker Options / Checker Options

**11 — Concrete Checker**

- Reinforced Concrete Section ← 08 Rectangular RC Section / RC section
- Standard ← 00 Standard / Standard
- Options ← 07 Concrete Checker Options / Checker Options

**12 — Concrete Checker**

- Reinforced Concrete Section ← 09 Rectangular RC Section / RC section
- Standard ← 00 Standard / Standard
- Options ← 07 Concrete Checker Options / Checker Options

**13 — Reinforcement Candidate Search**

- Target utilization = `0.9`
- Run = `True`
- Candidate checkers ← 10 Concrete Checker / Concrete Checker
- Candidate checkers ← 11 Concrete Checker / Concrete Checker
- Candidate checkers ← 12 Concrete Checker / Concrete Checker
- Forces ← 04 Force component / Force
- Forces ← 05 Force component / Force

**14 — Check Report**

- Results ← 13 Reinforcement Candidate Search / All results

**15 — Check Result**

- Result ← 13 Reinforcement Candidate Search / All results
