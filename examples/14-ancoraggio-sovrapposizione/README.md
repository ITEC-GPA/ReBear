# 14 — Ancoraggio e sovrapposizione di barre tese

C30/37, B450C Ø20 e σsd=300 MPa, EN 1992-1-1, condizioni di buona aderenza. Barra rettilinea tesa, coefficienti α1…α5 unitari. Lunghezza disponibile 1000 mm; per la sovrapposizione si assume il 100% delle barre sovrapposte e interferro 20 mm.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Con fctk,0.05=0.7×0.3×fck^(2/3), fbd=2.25 fctk,0.05/γc e lb,rqd=Øσsd/(4fbd), si verifica indipendentemente la lunghezza base. Il ramo di ancoraggio e quello di sovrapposizione richiamano separatamente GPC, che applica i minimi e i coefficienti pertinenti. La lunghezza di sovrapposizione deve risultare non inferiore a quella di ancoraggio in questa configurazione. Nessuna riduzione per ganci o confinamento è assunta.

| Quantità calcolata | Valore |
|---|---:|
| fbd [MPa] | 3.04129156 |
| lb,rqd [mm] | 493.211509 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Detailing | Anchorage | Satisfied | — | 493.211509 | 1000 | mm |
| Detailing | Lap | Satisfied | — | 739.817263 | 1000 | mm |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Componenti e collegamenti

I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- Type = `1`

**02 — Standard**

- Type = `1`

**03 — Anchorage and Lap**

- Diameter = `20`
- Design stress = `300`
- Available length = `1000`
- Good bond = `True`
- Lap = `False`
- Lapped percentage = `100`
- Lap clear distance = `20`
- Section ID = `S`
- Standard ← 02 Standard / Standard
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**04 — Check Report**

- Results ← 03 Anchorage and Lap / Result

**05 — Check Result**

- Result ← 03 Anchorage and Lap / Result

**06 — Anchorage and Lap**

- Diameter = `20`
- Design stress = `300`
- Available length = `1000`
- Good bond = `True`
- Lap = `True`
- Lapped percentage = `100`
- Lap clear distance = `20`
- Section ID = `S`
- Standard ← 02 Standard / Standard
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**07 — Check Report**

- Results ← 06 Anchorage and Lap / Result

**08 — Check Result**

- Result ← 06 Anchorage and Lap / Result
