# 18 — Momento–curvatura a N costante

Rettangolo 300 × 500, 8Ø20, N=−300 kN e direzione del momento 0° (Mx). Checker non lineare, dominio plastico, 20 passi e frazione finale 1. Tolleranza assiale assegnata 1 kN.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Il motore cerca il momento limite a N costante e risolve gli stati intermedi. Le curvature GPC in 1/mm sono moltiplicate per 1000 per ottenere 1/m; i momenti Nmm sono divisi per 10⁶ per ottenere kNm. Il controllo automatico verifica equilibrio assiale del punto limite entro 1 kN, almeno due punti e valori finiti. Leggere Status e InterruptedAtStep: una curva interrotta resta parziale e non viene presentata come interamente convergente. Per uso progettuale aumentare i passi e confrontare curva, snervamento e limite.

| Quantità calcolata | Valore |
|---|---:|
| Momento limite [kNm] | 262.793655 |
| Residuo N al limite [kN] | 8.62801663E-06 |
| Numero punti | 22 |
| χy [1/m] | 0.00752270156 |
| χu [1/m] | 0.0374991089 |

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): **Concrete Checker Options**. Gli altri componenti del grafo conservano il GUID di 0.2.2.

Il checker usa la strategia Iterative del punto di dominio, default di ReBear 0.3 (0.2.2: Intersection, che interpola il dominio discretizzato e costa 0.4–0.6 s per checker). I punti resistenti sono risolti direttamente sulla legge della sezione invece che interpolati sul dominio discretizzato; le differenze numeriche sono nella tabella, gli esiti (Satisfied/NotSatisfied) e le scelte non cambiano.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/18-momento-curvatura.json`); soglia 1E-09 relativo, 1E-10 assoluto.

| Grandezza | 0.2.2 | 0.3 | Δ relativo (ass. se un valore è nullo) |
|---|---:|---:|---:|
| Momento limite [kNm] | 262.765218 | 262.793655 | 1.1E-4 |
| Residuo N al limite [kN] | 0 | 8.62801663E-06 | ass. 8.6E-6 |
| χy [1/m] | 0.00752263537 | 0.00752270156 | 8.8E-6 |
| χu [1/m] | 0.0374733501 | 0.0374991089 | 6.9E-4 |
| Serie numerica (Δ massimo / valore massimo della serie) | 44 valori | 42 diversi | 1.1E-4 |

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
- Domain Point Strategy Types = `0`
- Stress Analysis Types = `0`
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

**07 — Moment Curvature**

- Axial force = `-300`
- Moment direction = `0`
- Steps = `20`
- End fraction = `1`
- Axial tolerance = `1`
- Run = `True`
- Checker ← 06 Concrete Checker / Concrete Checker
