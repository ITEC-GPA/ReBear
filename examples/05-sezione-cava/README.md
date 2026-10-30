# 05 — Sezione cava con due corone

Ingombro 1000 × 1200 mm e foro centrale 500 × 700 mm. Barre Ø20, copriferro alle staffe 35 mm e staffe Ø10. Corona esterna: nx=6, ny=8; interna: nx=4, ny=6. Gli angoli di ogni corona vengono contati una sola volta.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Ac = 1000×1200 − 500×700 = 850000 mm². Numero barre = [2×6+2×(8−2)] + [2×4+2×(6−2)] = 40. Il controllo misura il copriferro sia dal bordo esterno sia dal foro, verificando che nessuna barra cada nella cavità. Non si assume automaticamente un profilo resistente a torsione.

| Quantità calcolata | Valore |
|---|---:|
| Ac [mm²] | 850000 |
| As [mm²] | 12566.3706 |
| Numero barre | 40 |
| cbar,min [mm] | 45 |
| Interferro minimo [mm] | 120.035273 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Geometry | Geometry | Satisfied | — | — | — |  |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Tutti i componenti del grafo conservano il `ComponentGuid` di 0.2.2.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/05-sezione-cava.json`); soglia 1E-09 relativo, 1E-10 assoluto.

Nessuna variazione numerica oltre la soglia e nessun esito diverso.

## Componenti e collegamenti

Componenti numerati come sulla tela, da sinistra a destra e dall'alto in basso. I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- tutti i valori di default

**02 — Box RC Section**

- Width = `1000`
- Height = `1200`
- Hole width = `500`
- Hole height = `700`
- Link cover = `35`
- Link diameter = `10`
- Bar diameter = `20`
- Outer bars per horizontal side = `6`
- Outer bars per vertical side = `8`
- Inner bars per horizontal side = `4`
- Inner bars per vertical side = `6`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**03 — Section Geometry Check**

- Required bar cover = `45`
- Required clear spacing = `25`
- Section ID = `S`
- Section ← 02 Box RC Section / RC section

**04 — Check Report**

- Results ← 03 Section Geometry Check / Result

**05 — Check Result**

- Result ← 03 Section Geometry Check / Result
