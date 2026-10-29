# 04 — Sezione a I con ali differenti

Sezione a I: bt=600, bb=500, h=800, bw=240, tt=tb=180 mm. Ø20 con cinque barre superiori, quattro inferiori e due per ogni lato dell'anima. Copriferro esterno staffa 30 mm, staffa Ø8.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Ac = bt tt + bb tb + bw(h−tt−tb) = 303600 mm². As è la somma delle 13 barre Ø20. Si controllano i rientri, il copriferro e l'interferro. L'asse dell'ingombro rimane il riferimento geometrico; per analisi baricentriche predisporre il riferimento mediante Section Coordinate System (assi globali nel baricentro).

| Quantità calcolata | Valore |
|---|---:|
| Ac [mm²] | 303600 |
| As [mm²] | 4084.07045 |
| Numero barre | 13 |
| cbar,min [mm] | 38 |
| Interferro minimo [mm] | 94.6666667 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Geometry | Geometry | Satisfied | — | — | — |  |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Tutti i componenti del grafo conservano il `ComponentGuid` di 0.2.2.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/04-sezione-i.json`); soglia 1E-09 relativo, 1E-10 assoluto.

Nessuna variazione numerica oltre la soglia e nessun esito diverso.

## Componenti e collegamenti

I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- Type = `1`
- Behaviour = `0`

**02 — I RC Section**

- Top flange width = `600`
- Bottom flange width = `500`
- Height = `800`
- Web width = `240`
- Top flange thickness = `180`
- Bottom flange thickness = `180`
- Link cover = `30`
- Link diameter = `8`
- Bar diameter = `20`
- Top bars = `5`
- Bottom bars = `4`
- Web bars per face = `2`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**03 — Section Geometry Check**

- Required bar cover = `38`
- Required clear spacing = `25`
- Section ID = `S`
- Section ← 02 I RC Section / RC section

**04 — Check Report**

- Results ← 03 Section Geometry Check / Result

**05 — Check Result**

- Result ← 03 Section Geometry Check / Result
