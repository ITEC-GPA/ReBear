# 11 — SLE in compressione uniforme e combinazione frequente

Sezione 300 × 500 con 8Ø20, N=−300 kN e momenti nulli. Per questo confronto analitico si imposta ψrebar=0, analisi lineare e calcestruzzo teso escluso. Si esaminano la combinazione caratteristica e quella frequente.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Con n=Es/Ec e Ah=Ac+(n−1)As, la compressione uniforme vale σc=N/Ah e |σs|=n|σc|. Il generatore confronta le tensioni effettive GPC con tali valori (tolleranza relativa 10⁻⁶). Il controllo caratteristico restituisce un rapporto; per la combinazione frequente i limiti tensionali non definiti dal profilo restano NotRequired. Non interpretare tale esito come una capacità infinita.

| Quantità calcolata | Valore |
|---|---:|
| Area omogeneizzata [mm²] | 162794.502 |
| σc [MPa] | -1.84281414 |
| |σs| [MPa] | 11.224158 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| SLE-CHAR | SLE stress | Satisfied | 0.102378563 | — | — |  |
| SLE-CHAR | SLE cracking | NotRequired | — | — | — | MPa |
| SLE-CHAR | SLE stress | NotRequired | — | — | — |  |
| SLE-CHAR | SLE cracking | Satisfied | 0 | 0 | 0.3 | mm |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): **Concrete Checker Options**. Gli altri componenti del grafo conservano il GUID di 0.2.2.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/11-tensioni-sle.json`); soglia 1E-09 relativo, 1E-10 assoluto.

Nessuna variazione numerica oltre la soglia e nessun esito diverso.

## Componenti e collegamenti

Componenti numerati come sulla tela, da sinistra a destra e dall'alto in basso. I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- tutti i valori di default

**02 — Standard**

- tutti i valori di default

**03 — SLE Options**

- Combination = `2`
- Exposure = `XC4`
- Longitudinal bar cover = `38`

**04 — SLE Options**

- Combination = `1`
- Exposure = `XC4`
- Longitudinal bar cover = `38`

**05 — Coordinate System**

- Name = `Global XY`
- Origin = `0,0,0`
- P1 = `1,0,0`
- P2 = `0,1,0`

**06 — Rectangular RC Section**

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

**07 — Concrete Checker Options**

- Failure Analysis Types = `0`
- ψ rebar = `0`
- Coordinate System ← 05 Coordinate System / Coordinate System

**08 — Force component**

- Name = `SLE-CHAR`
- Axial Force = `-300`
- Coordinate System ← 05 Coordinate System / Coordinate System

**09 — Concrete Checker**

- Reinforced Concrete Section ← 06 Rectangular RC Section / RC section
- Standard ← 02 Standard / Standard
- Options ← 07 Concrete Checker Options / Checker Options

**10 — Stress Analysis**

- Concrete Checker ← 09 Concrete Checker / Concrete Checker
- Forces ← 08 Force component / Force

**11 — Serviceability Check**

- Section ID = `S`
- Stress result ← 10 Stress Analysis / Stress Analysis Results
- Options ← 03 SLE Options / Options

**12 — Serviceability Check**

- Section ID = `S-frequente`
- Stress result ← 10 Stress Analysis / Stress Analysis Results
- Options ← 04 SLE Options / Options

**13 — Check Report**

- Results ← 11 Serviceability Check / Results

**14 — Check Result**

- Result ← 11 Serviceability Check / Results

**15 — Check Report**

- Results ← 12 Serviceability Check / Results

**16 — Check Result**

- Result ← 12 Serviceability Check / Results
