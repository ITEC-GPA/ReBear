# ReBear

**Reinforced Concrete Sections for Grasshopper**

Plugin autonomo per la modellazione e la verifica di sezioni in calcestruzzo armato. Recupera i **32 componenti e 12 tipi di dato** di `Rhino2FEM` al commit `66555b6c43d10c8929c9871c172f5d78069655a8` del **1 aprile 2026**. Tutti i `ComponentGuid` e le registrazioni delle porte storiche sono conservati. La versione **0.2.0** aggiunge **29 componenti**, per un totale di **61**. Componenti storici e nuovi sono organizzati insieme nella scheda **ReBear**, secondo il flusso **Materials → Sections → Input → Setup → Analysis → Checks → Detailing → Results → Display**. La [mappa della palette](docs/PALETTE.md) riporta tutti i componenti e il loro ordine d'uso.

![Simbolo ReBear](assets/branding/symbol.png)

Le verifiche di taglio, tensioni SLE e fessurazione richiamano le librerie **GPC**. Il plugin contiene adattamento degli input, conversioni di unità, gestione degli esiti e visualizzazione; le formule di verifica sono nelle librerie. Il progetto non include lettori/esportatori SAP, Midas, Straus o altri moduli già separati da Rhino2FEM.

## Compilazione e verifiche

Windows, SDK .NET 8 o successivo compatibile con il target .NET 8; Rhino 8 in modalità .NET 8. I riferimenti SDK RhinoCommon/Grasshopper sono fissati a `8.27.26019.16021`. Rhino 7 e Rhino in modalità .NET Framework non sono target di questa versione.

```powershell
dotnet build ConcreteSectionChecker.sln -c Release
pwsh -File build/Verify.ps1
pwsh -File build/Package.ps1
```

Il nome della soluzione e i namespace tecnici `ConcreteSectionChecker` restano stabili; l'assembly distribuito è **`ReBear.gha`**. Output:

```text
bin/ReBear.gha
bin/*.dll
bin/ReBear.deps.json
bin/ReBear.runtimeconfig.json
bin/release-set.json
artifacts/packages/ReBear-0.2.1-<timestamp>.zip
```

Ogni compilazione Release, da Visual Studio o `dotnet build`, aggiorna direttamente la cartella `bin` alla radice con il plugin e tutte le sue dipendenze. È la cartella pronta da caricare in Grasshopper. Le compilazioni Debug non sovrascrivono la Release pubblicata.

I file intermedi e gli output di test e strumenti sono concentrati in `obj` alla radice, separati per progetto e configurazione. Dopo la pubblicazione della Release vengono eliminate le vecchie cartelle `bin`/`obj` dei sottoprogetti e i pacchetti precedenti in `artifacts/packages`. `Package.ps1` genera quindi un nuovo ZIP. La pulizia non segue link o junction e segnala eventuali file in uso, senza chiudere Rhino automaticamente.

`Verify.ps1` esegue i controlli numerici, confronta GUID e porte con la fotografia del commit originale, verifica gli hash delle sei DLL GPC e carica i 61 componenti con le librerie Grasshopper. Controlla anche icone, dati vuoti, conversioni di unità e uscite dei componenti di verifica. I test sono un eseguibile console: usare lo script o `dotnet run --project tests/ConcreteSectionChecker.Checks -c Release -- --grasshopper`, non soltanto `dotnet test`.

Il caricamento in processo di test non sostituisce l'apertura di una definizione storica dentro Rhino. La procedura di accettazione nell'host è descritta in [MIGRATION.md](docs/MIGRATION.md).

## Installazione

1. Chiudere Rhino. Estrarre l'intera cartella `ReBear` dello ZIP in una cartella delle librerie Grasshopper, individuabile da **File → Special Folders → Components Folder**.
2. Se Windows blocca file scaricati, sbloccare lo ZIP nelle proprietà prima di estrarlo.
3. Disabilitare il vecchio assembly che contiene gli stessi 32 componenti: due plugin con gli stessi `ComponentGuid` non possono essere caricati insieme. Nel progetto storico era `Fem2Rhino.Grasshopper.Checkers.gha`. Non disabilitare indiscriminatamente gli altri plugin già separati.
4. Conservare accanto a `ReBear.gha` tutte le DLL del pacchetto. RhinoCommon e Grasshopper sono forniti dall'host e non vengono distribuiti.
5. Riavviare Rhino/Grasshopper e verificare la scheda **ReBear**.

Se altri plugin caricano GPC nello stesso processo, allineare le rispettive dipendenze allo stesso set di rilascio: avere DLL con lo stesso nome e contenuto differente può impedire la risoluzione corretta degli assembly.

## Flusso di lavoro

`Standard` + materiali → barre/trefoli + superficie della sezione → `Reinforced Concrete Section` → `Concrete Checker Options` → `Concrete Checker`.

Le azioni create con `Force component` alimentano `Stress Analysis`, il dominio resistente e le verifiche di taglio. I risultati tensionali alimentano i controlli SLE e la visualizzazione.

| Grandezza | Convenzione |
|---|---|
| Coordinate, diametri, copriferro, passo | mm |
| Aree | mm² |
| Tensioni e moduli elastici | MPa = N/mm² |
| Azioni interne GPC | N e Nmm |
| Input `Force component`, `Units=0` | forze in N; momenti in Nmm |
| Input `Force component`, `Units=1` (default) | forze in kN; momenti in kNm |
| Sforzo normale | compressione negativa |
| Resistenze numeriche dei componenti di taglio | kN |
| Apertura delle fessure | mm |
| Working Ratio | adimensionale; `NaN` se non disponibile/non applicabile |

La geometria deve essere espressa in mm, indipendentemente dalle unità del documento Rhino. La superficie della sezione deve avere **una faccia piana parallela a XY**, con contorno e fori poligonali chiusi; le barre usano le stesse coordinate XY.

Nei componenti di taglio, `Base` è la dimensione X e `Height` quella Y; `V1/M2` servono la direzione X, `V2/M1` quella Y. Le azioni devono già essere nel riferimento della geometria; questi componenti non ruotano automaticamente i vettori.

Per il taglio rettangolare, l'input storico del copriferro è la distanza dal bordo alla **superficie della barra longitudinale**. Nel componente circolare è il copriferro alla **staffa esterna**. Questa differenza storica è mantenuta e va considerata nella preparazione degli input. Il materiale delle staffe è quello delle barre della sezione: la vecchia interfaccia non dispone di una porta indipendente.

## Dipendenze e architettura

| Progetto/cartella | Responsabilità |
|---|---|
| `src/ConcreteSectionChecker.Grasshopper` | Componenti, tipi Grasshopper, preview, icone, report |
| `src/ConcreteSectionChecker.Adapters` | Adattamento ai contratti GPC, validazione, convenzioni e unità |
| `lib/GPC` | Set binario GPC fissato, con manifest e hash |
| `tests/ConcreteSectionChecker.Checks` | Prove numeriche e compatibilità con il sorgente storico |
| `tools/IconGenerator` | Generatore riproducibile delle icone SVG/PNG e del simbolo |
| `tools/ExampleGenerator` | Generatore e controlli numerici dei 20 esempi `.gh`/`.ghx`, tramite `build/Examples.ps1` |

ReBear **0.2.1** usa il set GPC rilasciato il **26 ottobre 2026**, recuperato da `ANTHEA/lib/Checker`, con `candidate=false` e hash verificati. Le revisioni Model `2acc60ef` e Checker `799a6355` coincidono con i rispettivi `origin/master`, verificati sul remoto il 26 ottobre 2026.

| Assembly | Versione |
|---|---|
| GPCUtilities | 3.0.0.0 |
| GPCGeometry | 2.3.0.0 |
| DelaunayMesh | 2.0.0.14 |
| GPCModel | 5.21.0.1 |
| GPCModelData | 0.3.0.2 |
| GPCChecker.Concrete | 0.21.0.0 |
| MathNet.Numerics, via NuGet | 5.0.0 |

`lib/GPC/release-set.json` è il manifest completo del rilascio di origine: il plugin ne usa solo le sei DLL elencate. I progetti non dipendono dai percorsi dei repository fratelli. Per aggiornare GPC, sostituire il set coerente e il relativo manifest, quindi rieseguire le prove; non aggiornare una DLL isolata.

Il nuovo set usa per default la legge parabola-rettangolo analitica del calcestruzzo, conservando nella libreria la modalità tabulata storica. ReBear segue il default GPC: risultati non lineari, domini e momento-curvatura possono differire dalla versione 0.2.0. GUID, porte e collegamenti restano compatibili; vedere le [note di aggiornamento](docs/MIGRATION.md#aggiornamento-021--26-ottobre-2026).

Gli ID normativi storici restano `0=NTC2018`, `1=EN 1992-1-1`, `2=UNI EN 1992-1-1`. Sono gli identificatori dei profili implementati nella versione GPC fissata: non implicano automaticamente l'uso dell'ultima edizione di una norma. ReBear non aggiunge una propria interpretazione normativa. Modello, riferimenti e dettagli restituiti da GPC sono riportati nei report dei controlli migrati.

## Compatibilità e campo delle verifiche

- [Migrazione, differenze nei risultati e accettazione](docs/MIGRATION.md).
- [Catalogo dei 32 componenti e GUID originali](docs/COMPONENTS.md).
- [Palette completa, sottogruppi e ordine di utilizzo](docs/PALETTE.md).
- [I 29 nuovi componenti, porte e convenzioni](docs/NEW_COMPONENTS.md).
- [Flussi di lavoro, convenzioni e limiti dei nuovi controlli](docs/WORKFLOWS.md).
- [20 esempi Grasshopper svolti, con risultati e spiegazioni](examples/README.md).
- [Sviluppi successivi e funzionalità differite](docs/ROADMAP.md).
- [Grafica e rigenerazione delle icone](assets/branding/README.md).

La conservazione dei GUID permette a Grasshopper di identificare i componenti storici. Non promette l'identità numerica con il vecchio codice: la migrazione corregge alcuni comportamenti e segue le API GPC attuali. Anche i report CSV di taglio/SLE hanno intestazioni aggiornate; le porte e la loro posizione restano invariate.
