# 02 — Sezione circolare e rapporto di omogeneizzazione

Pilastro di diametro 600 mm, 12Ø20, cstaffa=40 mm e Østaffa=10 mm. Il contorno è un poligono inscritto di 64 lati. A lato è predisposta la conversione da φ=1.5 a rapporto modulare n.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Ac = n/2 × R² sin(2π/n), con n=64 e R=300 mm; confrontare con πR² per misurare l'errore geometrico. Il raggio della corona è R cos(π/64) − (40+10+10), così il copriferro è rispettato sui lati. Per omogeneizzazione n = Es(1+φ)/Ecm ed Ec,eff=Ecm/(1+φ); è una conversione da φ assegnato, non una previsione temporale.

| Quantità calcolata | Valore |
|---|---:|
| Ac [mm²] | 282289.364 |
| As [mm²] | 3769.91118 |
| Numero barre | 12 |
| cbar,min [mm] | 50.0320785 |
| Interferro minimo [mm] | 104.046086 |
| Errore area rispetto al cerchio [%] | 0.160560696 |
| n [-] | 15.2269263 |
| Ec,eff [MPa] | 13134.6272 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Geometry | Geometry | Satisfied | — | — | — |  |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Tutti i componenti del grafo conservano il `ComponentGuid` di 0.2.2.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/02-cerchio.json`); soglia 1E-09 relativo, 1E-10 assoluto.

Nessuna variazione numerica oltre la soglia e nessun esito diverso.

## Componenti e collegamenti

Componenti numerati come sulla tela, da sinistra a destra e dall'alto in basso. I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- tutti i valori di default

**02 — Circular RC Section**

- Diameter = `600`
- Link cover = `40`
- Link diameter = `10`
- Bar diameter = `20`
- Bar count = `12`
- Polygon sides = `64`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**03 — Creep and Homogenization**

- Value = `1.5`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**04 — Section Geometry Check**

- Required bar cover = `50`
- Required clear spacing = `25`
- Section ID = `S`
- Section ← 02 Circular RC Section / RC section

**05 — Check Report**

- Results ← 04 Section Geometry Check / Result

**06 — Check Result**

- Result ← 04 Section Geometry Check / Result
