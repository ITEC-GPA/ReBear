# 03 — Sezione a T con ala superiore

Sezione a T: bf=800, h=700, bw=300, tf=180 mm. Armatura Ø20: sei barre nell'ala superiore, tre inferiori nell'anima, due per ciascun lato dell'anima. cstaffa=30 mm, Østaffa=8 mm.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Ac = bf tf + bw(h−tf) = 800×180 + 300×520 = 300000 mm². Le barre laterali escludono quelle delle file superiore/inferiore. Il controllo comprende contenimento e distanza anche dai rientri del contorno. L'origine è il centro dell'ingombro: non coincide necessariamente con il baricentro. Non viene dedotto un modello di taglio dall'ala.

| Quantità calcolata | Valore |
|---|---:|
| Ac [mm²] | 300000 |
| As [mm²] | 4084.07045 |
| Numero barre | 13 |
| cbar,min [mm] | 38 |
| Interferro minimo [mm] | 82 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Geometry | Geometry | Satisfied | — | — | — |  |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Tutti i componenti del grafo conservano il `ComponentGuid` di 0.2.2.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/03-sezione-t.json`); soglia 1E-09 relativo, 1E-10 assoluto.

Nessuna variazione numerica oltre la soglia e nessun esito diverso.

## Componenti e collegamenti

Componenti numerati come sulla tela, da sinistra a destra e dall'alto in basso. I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- tutti i valori di default

**02 — T RC Section**

- Flange width = `800`
- Height = `700`
- Web width = `300`
- Flange thickness = `180`
- Link cover = `30`
- Link diameter = `8`
- Bar diameter = `20`
- Top bars = `6`
- Bottom bars = `3`
- Web bars per face = `2`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**03 — Section Geometry Check**

- Required bar cover = `38`
- Required clear spacing = `25`
- Section ID = `S`
- Section ← 02 T RC Section / RC section

**04 — Check Report**

- Results ← 03 Section Geometry Check / Result

**05 — Check Result**

- Result ← 03 Section Geometry Check / Result
