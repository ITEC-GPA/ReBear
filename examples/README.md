# ReBear — 20 esempi svolti

Ogni cartella contiene una definizione nativa `.gh`, la corrispondente `.ghx` testuale, la spiegazione passo per passo, i risultati numerici `results.json` e il grafo dei collegamenti `graph.json`.

## Apertura

1. Caricare tutta la cartella `bin` di ReBear 0.2.1 in Grasshopper/Rhino 8, modalità .NET 8.
2. Aprire `definition.gh` dell'esempio desiderato; non occorrono altri plugin né file esterni.
3. I dati sono salvati nelle porte: tasto destro → Set… per modificarli. I Run sono attivi. Leggere il README della cartella per ipotesi, unità e verifiche.
4. Confrontare risultati e messaggi con `results.json`. I casi volutamente non soddisfatti o non valutabili sono spiegati.

Calcoli adapter/GPC e controlli numerici eseguiti automaticamente. Archivi GH riletti con controllo di identità e connessioni. **Ricalcolo, salvataggio e preview dentro Rhino restano da provare nell'host.**

| N. | Esempio | File GH |
|---|---|---|
| 01 | [Sezione rettangolare 300 × 500](01-rettangolo/README.md) | [Apri definizione](01-rettangolo/definition.gh) |
| 02 | [Sezione circolare e rapporto di omogeneizzazione](02-cerchio/README.md) | [Apri definizione](02-cerchio/definition.gh) |
| 03 | [Sezione a T con ala superiore](03-sezione-t/README.md) | [Apri definizione](03-sezione-t/definition.gh) |
| 04 | [Sezione a I con ali differenti](04-sezione-i/README.md) | [Apri definizione](04-sezione-i/definition.gh) |
| 05 | [Sezione cava con due corone](05-sezione-cava/README.md) | [Apri definizione](05-sezione-cava/definition.gh) |
| 06 | [Inviluppo con combinazioni concomitanti](06-inviluppo-azioni/README.md) | [Apri definizione](06-inviluppo-azioni/definition.gh) |
| 07 | [Taglio senza staffe: caso valutabile e caso escluso](07-taglio-senza-staffe/README.md) | [Apri definizione](07-taglio-senza-staffe/definition.gh) |
| 08 | [Taglio X e Y in un unico batch](08-taglio-xy/README.md) | [Apri definizione](08-taglio-xy/definition.gh) |
| 09 | [Torsione con due tagli concomitanti](09-torsione-taglio/README.md) | [Apri definizione](09-torsione-taglio/definition.gh) |
| 10 | [Resistenza N-Mx-My e inviluppo degli esiti](10-resistenza-inviluppo/README.md) | [Apri definizione](10-resistenza-inviluppo/definition.gh) |
| 11 | [SLE in compressione uniforme e combinazione frequente](11-tensioni-sle/README.md) | [Apri definizione](11-tensioni-sle/definition.gh) |
| 12 | [Fessurazione e regioni efficaci](12-fessurazione/README.md) | [Apri definizione](12-fessurazione/definition.gh) |
| 13 | [Copriferro per esposizioni concorrenti](13-copriferro/README.md) | [Apri definizione](13-copriferro/definition.gh) |
| 14 | [Ancoraggio e sovrapposizione di barre tese](14-ancoraggio-sovrapposizione/README.md) | [Apri definizione](14-ancoraggio-sovrapposizione/definition.gh) |
| 15 | [Dettagli di armatura di una trave](15-dettagli-trave/README.md) | [Apri definizione](15-dettagli-trave/definition.gh) |
| 16 | [Dettagli di un pilastro con conferme mancanti](16-dettagli-pilastro/README.md) | [Apri definizione](16-dettagli-pilastro/definition.gh) |
| 17 | [Soletta: armatura principale e secondaria](17-dettagli-soletta/README.md) | [Apri definizione](17-dettagli-soletta/definition.gh) |
| 18 | [Momento–curvatura a N costante](18-momento-curvatura/README.md) | [Apri definizione](18-momento-curvatura/definition.gh) |
| 19 | [Sezione Mx–My del dominio a N assegnato](19-dominio-n-costante/README.md) | [Apri definizione](19-dominio-n-costante/definition.gh) |
| 20 | [Scelta della minima armatura fra tre candidati](20-ricerca-armatura/README.md) | [Apri definizione](20-ricerca-armatura/definition.gh) |

## Riproduzione

Da PowerShell nella radice del repository:

```powershell
powershell -File build/Examples.ps1
```

Lo script compila in Release, ricalcola i 20 esempi e rigenera le cartelle. Non avvia Rhino. I file GH contengono identificatori di istanza nuovi a ogni generazione; i GUID dei tipi di componente rimangono stabili. Nessun export ANTHEA è incluso.

Il generatore usa i serializer ufficiali dei componenti Grasshopper e un contenitore di definizione conforme alla struttura osservata nell'archivio storico. Evita l'inizializzazione di GH_Document, che richiede Rhino nativo. I risultati vengono calcolati separatamente dagli adapter, con gli stessi input dei grafi; questo non costituisce un test del ricalcolo nell'host.
