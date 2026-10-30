# 10 — Resistenza N-Mx-My e inviluppo degli esiti

Rettangolo 300 × 500, 8Ø20, C30/37 e B450C. Dominio plastico GPC, criterio a N costante e strategia Iterative (default di ReBear 0.3). Tre combinazioni includono flessione deviata, domanda elevata e sforzo normale esterno al dominio.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Le resistenze si cercano lungo la direzione dei momenti richiesti mantenendo N. Il primo caso N=−300 kN, Mx=80 e My=10 kNm è moderato; il secondo Mx=400 kNm mostra un possibile superamento; il terzo N=−10000000 kN è intenzionalmente fuori campo resistente. Quest'ultimo deve risultare NotSatisfied anche senza un rapporto numerico disponibile. L'inviluppo non può eliminare il caso esterno perché η è null.

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| SLU-moderato | N-Mx-My | Satisfied | 0.317680515 | — | — |  |
| SLU-elevato | N-Mx-My | NotSatisfied | 1.52210676 | — | — |  |
| Fuori-dominio | N-Mx-My | NotSatisfied | 2864.98979 | — | — |  |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): **Concrete Checker Options**. Gli altri componenti del grafo conservano il GUID di 0.2.2.

Il checker usa la strategia Iterative del punto di dominio, default di ReBear 0.3 (0.2.2: Intersection, che interpola il dominio discretizzato e costa 0.4–0.6 s per checker). I punti resistenti sono risolti direttamente sulla legge della sezione invece che interpolati sul dominio discretizzato; le differenze numeriche sono nella tabella, gli esiti (Satisfied/NotSatisfied) e le scelte non cambiano.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/10-resistenza-inviluppo.json`); soglia 1E-09 relativo, 1E-10 assoluto.

| Grandezza | 0.2.2 | 0.3 | Δ relativo (ass. se un valore è nullo) |
|---|---:|---:|---:|
| SLU-moderato / N-Mx-My η | 0.317753025 | 0.317680515 | 2.3E-4 |
| SLU-elevato / N-Mx-My η | 1.52227149 | 1.52210676 | 1.1E-4 |
| Fuori-dominio / N-Mx-My η | 2864.84708 | 2864.98979 | 5.0E-5 |

## Componenti e collegamenti

Componenti numerati come sulla tela, da sinistra a destra e dall'alto in basso. I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- tutti i valori di default

**02 — Standard**

- tutti i valori di default

**03 — Coordinate System**

- Name = `Global XY`
- Origin = `0,0,0`
- P1 = `1,0,0`
- P2 = `0,1,0`

**04 — Rectangular RC Section**

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

**05 — Concrete Checker Options**

- Failure Analysis Types = `0`
- Coordinate System ← 03 Coordinate System / Coordinate System

**06 — Force component**

- Name = `SLU-moderato`
- Axial Force = `-300`
- Bending X = `80`
- Bending Y = `10`
- Coordinate System ← 03 Coordinate System / Coordinate System

**07 — Force component**

- Name = `SLU-elevato`
- Axial Force = `-300`
- Bending X = `400`
- Coordinate System ← 03 Coordinate System / Coordinate System

**08 — Force component**

- Name = `Fuori-dominio`
- Axial Force = `-10000000`
- Bending X = `80`
- Coordinate System ← 03 Coordinate System / Coordinate System

**09 — Concrete Checker**

- Reinforced Concrete Section ← 04 Rectangular RC Section / RC section
- Standard ← 02 Standard / Standard
- Options ← 05 Concrete Checker Options / Checker Options

**10 — Batch Section Checks**

- Section ID = `S`
- Run = `True`
- Checker ← 09 Concrete Checker / Concrete Checker
- Forces ← 06 Force component / Force
- Forces ← 07 Force component / Force
- Forces ← 08 Force component / Force

**11 — Check Envelope**

- Results ← 10 Batch Section Checks / Results

**12 — Check Report**

- Results ← 10 Batch Section Checks / Results

**13 — Check Result**

- Result ← 10 Batch Section Checks / Results
