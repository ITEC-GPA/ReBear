# 01 — Sezione rettangolare 300 × 500

Sezione in C30/37 e B450C, quattro barre Ø20 superiori e quattro inferiori. Copriferro alla staffa 30 mm e staffa Ø8. L'esempio costruisce la sezione ed esegue il controllo geometrico delle barre.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Distanza dei centri dal bordo: a = 30 + 8 + 20/2 = 48 mm. Ac = 300 × 500 = 150000 mm²; As = 8 × π20²/4. Il copriferro alla barra è 38 mm. L'interferro della fila è (300 − 2×48)/3 − 20 = 48 mm. Il controllo automatico confronta aree e interferro con queste espressioni.

| Quantità calcolata | Valore |
|---|---:|
| Ac [mm²] | 150000 |
| As [mm²] | 2513.27412 |
| Numero barre | 8 |
| cbar,min [mm] | 38 |
| Interferro minimo [mm] | 48 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Geometry | Geometry | Satisfied | — | — | — |  |

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

**03 — Section Geometry Check**

- Required bar cover = `38`
- Required clear spacing = `25`
- Section ID = `S`
- Section ← 02 Rectangular RC Section / RC section

**04 — Check Report**

- Results ← 03 Section Geometry Check / Result

**05 — Check Result**

- Result ← 03 Section Geometry Check / Result
