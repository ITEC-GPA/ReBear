# 20 — Scelta della minima armatura fra tre candidati

Tre sezioni 300 × 500 mm, tutte con quattro barre superiori e quattro inferiori; diametri candidati 16, 20 e 24 mm. C30/37, B450C, copriferro staffa 30 e staffa Ø8 mm. SLU1: N=−300 kN, Mx=180 e My=5 kNm; SLU2: N=−200 kN, Mx=150 e My=10 kNm. Obiettivo η≤0.9.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.1 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Per ogni candidato si eseguono entrambe le verifiche N-Mx-My. Si scartano quelli con esiti non valutati/non soddisfatti o utilizzo superiore a 0.9, poi si seleziona l'area minima fra i rimanenti. Il controllo indipendente sul risultato ricalcola questo filtro e confronta indice e area selezionati. La ricerca è discreta e limitata ai tre layout assegnati: non ottimizza diametri continui e non verifica automaticamente taglio, SLE o dettagli.

| Quantità calcolata | Valore |
|---|---:|
| Indice selezionato (base zero) | 1 |
| As selezionata [mm²] | 2513.27412 |
| Diametro selezionato [mm] | 20 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| SLU1 | N-Mx-My | Satisfied | 0.9355373 | — | — |  |
| SLU2 | N-Mx-My | Satisfied | 0.869273737 | — | — |  |
| SLU1 | N-Mx-My | Satisfied | 0.687471856 | — | — |  |
| SLU2 | N-Mx-My | Satisfied | 0.620693418 | — | — |  |
| SLU1 | N-Mx-My | Satisfied | 0.521183698 | — | — |  |
| SLU2 | N-Mx-My | Satisfied | 0.462639205 | — | — |  |

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
- Bar diameter = `16`
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

**07 — Rectangular RC Section**

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

**08 — Rectangular RC Section**

- Width = `300`
- Height = `500`
- Link cover = `30`
- Link diameter = `8`
- Bar diameter = `24`
- Top bars = `4`
- Bottom bars = `4`
- Side bars per face = `0`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**09 — Concrete Checker**

- Reinforced Concrete Section ← 07 Rectangular RC Section / RC section
- Standard ← 03 Standard / Standard
- Options ← 04 Concrete Checker Options / Checker Options

**10 — Concrete Checker**

- Reinforced Concrete Section ← 08 Rectangular RC Section / RC section
- Standard ← 03 Standard / Standard
- Options ← 04 Concrete Checker Options / Checker Options

**11 — Force component**

- Name = `SLU1`
- Axial Force = `-300`
- Shear X = `0`
- Shear Y = `0`
- Torsion = `0`
- Bending X = `180`
- Bending Y = `5`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**12 — Force component**

- Name = `SLU2`
- Axial Force = `-200`
- Shear X = `0`
- Shear Y = `0`
- Torsion = `0`
- Bending X = `150`
- Bending Y = `10`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**13 — Reinforcement Candidate Search**

- Target utilization = `0.9`
- Run = `True`
- Candidate checkers ← 06 Concrete Checker / Concrete Checker
- Candidate checkers ← 09 Concrete Checker / Concrete Checker
- Candidate checkers ← 10 Concrete Checker / Concrete Checker
- Forces ← 11 Force component / Force
- Forces ← 12 Force component / Force

**14 — Check Report**

- Results ← 13 Reinforcement Candidate Search / All results

**15 — Check Result**

- Result ← 13 Reinforcement Candidate Search / All results
