# 13 — Copriferro per esposizioni concorrenti

Calcestruzzo C30/37 e profilo EN 1992-1-1 disponibile nel set GPC. Esposizioni concorrenti XC4 e XD1, vita 50 anni, diametro effettivo Ø20, aggregato 20 mm e Δcdev=10 mm. Non si applicano riduzioni di classe, controllo speciale, abrasione o contatto col terreno.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.3.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Classe strutturale S4 senza modificatori: cmin,dur governa su XD1 (35 mm), rispetto a XC4 (30 mm). cmin,b=20 mm; cmin=max(10,20,35)=35 mm e cnom=35+10=45 mm. Il copriferro adottato 40 mm è insufficiente: l'esito atteso è NotSatisfied, con domanda 45 e disponibile 40 mm. Il riferimento è il profilo GPC fissato, non una selezione automatica dell'edizione normativa più recente.

| Quantità calcolata | Valore |
|---|---:|
| cmin,b [mm] | 20 |
| cmin,dur [mm] | 35 |
| cnom [mm] | 45 |

| Caso | Verifica | Esito GPC | η | Domanda | Capacità/limite | Unità |
|---|---|---|---:|---:|---:|---|
| Durability | Concrete cover | NotSatisfied | — | 45 | 40 | mm |

Esiti e dettagli integrali, riferimenti restituiti da GPC e precisione completa sono in `results.json`. Un rapporto non definito non equivale a zero; nei dettagli di armatura Actual/Limit conservano il verso della singola regola descritto nel messaggio.

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Differenze rispetto a ReBear 0.2.2

Componenti con `ComponentGuid` nuovo in 0.3 (porte o significato cambiati, vedere [MIGRATION](../../docs/MIGRATION.md)): **Concrete Cover**. Gli altri componenti del grafo conservano il GUID di 0.2.2.

Confronto automatico con i risultati pubblicati in 0.2.2 (`tools/ExampleGenerator/baseline-0.2.2/13-copriferro.json`); soglia 1E-09 relativo, 1E-10 assoluto.

Nessuna variazione numerica oltre la soglia e nessun esito diverso.

## Componenti e collegamenti

I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Standard**

- Type = `1`

**02 — Concrete Cover**

- Exposures = `XC4, XD1`
- Specification bar diameter = `20`
- Actual bar diameter = `20`
- Adopted cover = `40`
- Section ID = `S`
- Standard ← 01 Standard / Standard
- Concrete ← 00 Database Concrete / Concrete Material

**03 — Check Report**

- Results ← 02 Concrete Cover / Result

**04 — Check Result**

- Result ← 02 Concrete Cover / Result
