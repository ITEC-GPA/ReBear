# 12 — Fessurazione e regioni efficaci

Rettangolo 300 × 500, 8Ø20, analisi lineare con ψrebar=1.5. Caso quasi permanente N=−100 kN e Mx=100 kNm, esposizione XC4, barre non sensibili ad aderenza migliorata, lunga durata. Il copriferro di fessurazione è alla barra: 30+8=38 mm.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Stress Analysis determina la sezione fessurata. Serviceability Check v2 applica i limiti tensionali e il modello di fessurazione GPC; Effective Cracked Section visualizza le regioni Ac,eff e le barre contribuenti. Il controllo automatico richiede larghezza finita non negativa, almeno una regione e η = w/limite quando il criterio è quello di ampiezza. Nei dettagli sono disponibili kt, aree efficaci e passaggi del motore: non si sostituiscono con formule locali nel plugin.

| Quantità calcolata | Valore |
|---|---:|
| Regioni efficaci | 1 |
| Ac,eff totale [mm²] | 31476.6856 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| SLE-QP | SLE stress | Satisfied | 0.549805788 | — | — |  |
| SLE-QP | SLE cracking | Satisfied | 0.678948224 | 0.135789645 | 0.2 | mm |

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

- Name = `SLE-QP`
- Axial Force = `-100`
- Shear X = `0`
- Shear Y = `0`
- Torsion = `0`
- Bending X = `100`
- Bending Y = `0`
- Units = `1`
- Coordinate System ← 05 Coordinate System / Coordinate System

**08 — Stress Analysis**

- Concrete Checker ← 06 Concrete Checker / Concrete Checker
- Forces ← 07 Force component / Force

**09 — SLE Options**

- Combination = `0`
- Exposure = `XC4`
- Sensitive reinforcement = `False`
- Short term = `False`
- Ribbed bars = `True`
- Longitudinal bar cover = `38`
- Concentric rings = `False`

**10 — Serviceability Check v2**

- Section ID = `S`
- Stress result ← 08 Stress Analysis / Stress Analysis Results
- Options ← 09 SLE Options / Options

**11 — Check Report**

- Results ← 10 Serviceability Check v2 / Results

**12 — Check Result**

- Result ← 10 Serviceability Check v2 / Results

**13 — Effective Cracked Section**

- Crack data ← 10 Serviceability Check v2 / Crack data
