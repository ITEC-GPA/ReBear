# 10 — Resistenza N-Mx-My e inviluppo degli esiti

Rettangolo 300 × 500, 8Ø20, C30/37 e B450C. Dominio plastico GPC, criterio a N costante e strategia Intersection. Tre combinazioni includono flessione deviata, domanda elevata e sforzo normale esterno al dominio.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Le resistenze si cercano lungo la direzione dei momenti richiesti mantenendo N. Il primo caso N=−300 kN, Mx=80 e My=10 kNm è moderato; il secondo Mx=400 kNm mostra un possibile superamento; il terzo N=−10000000 kN è intenzionalmente fuori campo resistente. Quest'ultimo deve risultare NotSatisfied anche senza un rapporto numerico disponibile. L'inviluppo non può eliminare il caso esterno perché η è null.

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| SLU-moderato | N-Mx-My | Satisfied | 0.317774615 | — | — |  |
| SLU-elevato | N-Mx-My | NotSatisfied | 1.52238713 | — | — |  |
| Fuori-dominio | N-Mx-My | NotSatisfied | 2864.84912 | — | — |  |

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

**07 — Force component**

- Name = `SLU-moderato`
- Axial Force = `-300`
- Shear X = `0`
- Shear Y = `0`
- Torsion = `0`
- Bending X = `80`
- Bending Y = `10`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**08 — Force component**

- Name = `SLU-elevato`
- Axial Force = `-300`
- Shear X = `0`
- Shear Y = `0`
- Torsion = `0`
- Bending X = `400`
- Bending Y = `0`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**09 — Force component**

- Name = `Fuori-dominio`
- Axial Force = `-10000000`
- Shear X = `0`
- Shear Y = `0`
- Torsion = `0`
- Bending X = `80`
- Bending Y = `0`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**10 — Batch Section Checks**

- Section ID = `S`
- N-Mx-My = `True`
- Run = `True`
- Checker ← 06 Concrete Checker / Concrete Checker
- Forces ← 07 Force component / Force
- Forces ← 08 Force component / Force
- Forces ← 09 Force component / Force

**11 — Check Envelope**

- Results ← 10 Batch Section Checks / Results

**12 — Check Report**

- Results ← 10 Batch Section Checks / Results

**13 — Check Result**

- Result ← 10 Batch Section Checks / Results
