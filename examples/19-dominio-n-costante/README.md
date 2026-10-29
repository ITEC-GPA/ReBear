# 19 — Sezione Mx–My del dominio a N assegnato

Sezione rettangolare simmetrica, N=−300 kN e 16 direzioni. Ogni direzione viene risolta a N costante; l'uscita è una polilinea chiusa nel piano dei momenti in kNm.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

I vettori di prova sono Mx=cosθ e My=sinθ, campionati uniformemente. Per ogni punto si controlla lo sforzo normale effettivamente equilibrato, con tolleranza relativa 10⁻³ (0.3 kN per questo esempio). I punti opposti devono mostrare la simmetria della sezione e delle armature: si confrontano Mx(θ) e −Mx(θ+π), analogamente per My, entro l'1%. La polilinea è una discretizzazione: raddoppiare le direzioni per valutarne la sensibilità.

| Quantità calcolata | Valore |
|---|---:|
| Mx massimo [kNm] | 262.793655 |
| My massimo [kNm] | 125.208397 |
| Residuo N massimo [kN] | 0.000629480769 |

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): **Concrete Checker Options**. Gli altri componenti del grafo conservano il GUID di 0.2.2.

Il checker usa la strategia Iterative del punto di dominio, default di ReBear 0.3 (0.2.2: Intersection, che interpola il dominio discretizzato e costa 0.4–0.6 s per checker). I punti resistenti sono risolti direttamente sulla legge della sezione invece che interpolati sul dominio discretizzato; le differenze numeriche sono nella tabella, gli esiti (Satisfied/NotSatisfied) e le scelte non cambiano.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/19-dominio-n-costante.json`); soglia 1E-09 relativo, 1E-10 assoluto.

| Grandezza | 0.2.2 | 0.3 | Δ relativo (ass. se un valore è nullo) |
|---|---:|---:|---:|
| Mx massimo [kNm] | 262.765218 | 262.793655 | 1.1E-4 |
| My massimo [kNm] | 125.198017 | 125.208397 | 8.3E-5 |
| Residuo N massimo [kN] | 0 | 0.000629480769 | ass. 6.3E-4 |
| Serie numerica (Δ massimo / valore massimo della serie) | 48 valori | 40 diversi | 1.7E-3 |

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

**07 — Domain Slice at N**

- Axial force = `-300`
- Directions = `16`
- Run = `True`
- Checker ← 06 Concrete Checker / Concrete Checker
