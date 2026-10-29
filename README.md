# ReBear

**Reinforced Concrete Sections for Grasshopper**

Plugin autonomo per la modellazione e la verifica di sezioni in calcestruzzo armato con le librerie **GPC**. La versione **0.3.0** riunisce in un solo modello (`ReBearComponent`) i 32 componenti recuperati da `Rhino2FEM` (commit `66555b6c43d10c8929c9871c172f5d78069655a8` del 1 aprile 2026) e quelli aggiunti in 0.2: **58 componenti** con porte dichiarate, unità esplicite, risultati strutturati e `NaN` — mai zero — per i valori mancanti. I componenti sono organizzati nella scheda **ReBear** secondo il flusso **Materials → Sections → Input → Setup → Analysis → Checks → Detailing → Results → Display** ([mappa della palette](docs/PALETTE.md)).

![Simbolo ReBear](assets/branding/symbol.png)

Le verifiche di resistenza, taglio, torsione, tensioni SLE, fessurazione, copriferro e dettagli richiamano le librerie GPC. Il plugin contiene adattamento degli input, conversioni di unità, gestione degli esiti, report e visualizzazione; le formule di verifica sono nelle librerie. Il progetto non include lettori/esportatori SAP, Midas, Straus o altri moduli già separati da Rhino2FEM.

Chi aggiorna definizioni 0.2 deve leggere la [tabella 0.2.2 → 0.3](docs/MIGRATION.md#componenti-022--03): 45 componenti conservano il `ComponentGuid` con porte identiche, 11 ne hanno uno nuovo (porte o significato cambiati), 5 componenti storici sono sostituiti dalle verifiche strutturate e 2 componenti sono nuovi.

## Compilazione e verifiche

Windows, SDK .NET 8 o successivo compatibile con il target .NET 8; Rhino 8 in modalità .NET 8. I riferimenti SDK RhinoCommon/Grasshopper sono fissati a `8.27.26019.16021`. Rhino 7 e Rhino in modalità .NET Framework non sono target di questa versione.

```powershell
dotnet build ConcreteSectionChecker.sln -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File build/Verify.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File build/Examples.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File build/Package.ps1
```

Il nome della soluzione e i namespace tecnici `ConcreteSectionChecker` restano stabili; l'assembly distribuito è **`ReBear.gha`**. Output:

```text
bin/ReBear.gha
bin/*.dll
bin/ReBear.deps.json
bin/ReBear.runtimeconfig.json
bin/release-set.json
artifacts/packages/ReBear-0.3.0-<timestamp>.zip
```

Ogni compilazione Release, da Visual Studio o `dotnet build`, aggiorna la cartella `bin` alla radice con il plugin e tutte le sue dipendenze: è la cartella pronta da caricare in Grasshopper. Le compilazioni Debug non sovrascrivono la Release pubblicata. I file intermedi e gli output di test e strumenti sono in `obj` alla radice, separati per progetto e configurazione. Dopo la sincronizzazione della Release vengono eliminate soltanto le vecchie cartelle `bin`/`obj` dei sottoprogetti; i pacchetti in `artifacts/packages` sono gestiti da `Package.ps1`, che elimina i precedenti solo dopo aver creato il nuovo ZIP (`-KeepPrevious` per conservarli). La pulizia non segue link o junction e segnala eventuali file in uso, senza chiudere Rhino.

`Verify.ps1` compila la Release ed esegue 40 gruppi di controlli: numerici, regressioni dei difetti corretti in 0.3, fotografia della palette (`tests/Compatibility/palette-0.3.json`) e regola dei GUID rispetto a 0.2.2, hash delle sei DLL GPC, caricamento dei 58 componenti con le librerie Grasshopper, persistenza dei dati e porte dei componenti attraverso `IGH_DataAccess`. `Examples.ps1` rigenera e controlla i 20 esempi. Il flusso di GitHub Actions `.github/workflows/verify.yml` esegue entrambi su Windows. I test sono un eseguibile console: usare gli script oppure `dotnet obj/bin/ConcreteSectionChecker.Checks/Release/net8.0-windows/ConcreteSectionChecker.Checks.dll --grasshopper`, non `dotnet test`. L'opzione `--benchmark` misura i tempi delle operazioni lunghe.

Il caricamento in processo di test non sostituisce l'apertura delle definizioni dentro Rhino: `tools/RhinoExampleProbe.py` apre i 20 esempi in Rhino 8 e registra i messaggi di ogni componente ([procedura](docs/MIGRATION.md#accettazione-da-eseguire-in-rhino)).

## Installazione

1. Chiudere Rhino. Estrarre l'intera cartella `ReBear` dello ZIP in una cartella delle librerie Grasshopper, individuabile da **File → Special Folders → Components Folder**.
2. Se Windows blocca file scaricati, sbloccare lo ZIP nelle proprietà prima di estrarlo.
3. Disabilitare il vecchio assembly che contiene gli stessi componenti storici: due plugin con gli stessi `ComponentGuid` non possono essere caricati insieme. Nel progetto storico era `Fem2Rhino.Grasshopper.Checkers.gha`. Non disabilitare indiscriminatamente gli altri plugin già separati.
4. Conservare accanto a `ReBear.gha` tutte le DLL del pacchetto e `release-set.json`. RhinoCommon e Grasshopper sono forniti dall'host e non vengono distribuiti.
5. Riavviare Rhino/Grasshopper e verificare la scheda **ReBear**.

Se altri plugin caricano GPC nello stesso processo, devono usare lo stesso set di rilascio: il primo assembly caricato vale per tutti. Al caricamento ReBear confronta versione e SHA-256 delle DLL GPC presenti con `release-set.json` e, se differiscono, lo segnala nella riga di comando di Rhino e con un avviso su ogni componente.

## Flusso di lavoro

`Standard` + materiali → barre/trefoli + superficie della sezione (o un builder) → `Reinforced Concrete Section` → `Concrete Checker Options` → `Concrete Checker`.

Le azioni create con `Force component` alimentano `Stress Analysis`, le verifiche di resistenza (`Section Resistance Check`, `3D Failure Domain Analysis`), di taglio (`Shear Check` con `Shear Options`, `Rectangular Shear Options` o `Circular Shear Options`), torsione e SLE (`Serviceability Check` con `SLE Options`). `Batch Section Checks` esegue tutte le verifiche di un ramo. Tutti gli esiti sono `Check Result` strutturati: `Check Envelope` li raggruppa e `Check Report` produce l'unico report CSV/JSON (anche dei riepiloghi tensionali di `Stress Analysis Summary`). Batch, ricerca delle armature, sezione del dominio e momento–curvatura sono calcolati in background: il componente mostra "Calcolo in corso…", un nuovo calcolo annulla il precedente e il menu contestuale offre "Annulla calcolo".

| Grandezza | Convenzione |
|---|---|
| Coordinate, diametri, copriferro, passo | mm |
| Aree | mm² |
| Tensioni e moduli elastici | MPa = N/mm² |
| Deformazioni | adimensionali; ‰ nei riepiloghi tensionali |
| Angoli | gradi |
| Azioni interne GPC | N e Nmm |
| Input `Force component`, `Units=0` | forze in N; momenti in Nmm |
| Input `Force component`, `Units=1` (default) | forze in kN; momenti in kNm |
| Uscite di verifica e di analisi | kN, kNm, curvature in 1/m |
| Sforzo normale | compressione negativa |
| Momenti negli assi globali | Mx positivo comprime le fibre con y negativa (convenzione GPC) |
| Valori mancanti | `NaN` nelle porte, campo vuoto nel CSV, `null` nel JSON; mai zero |
| Combinazioni senza nome | `Caso k`, k = posizione nella lista o iterazione del componente |

La geometria deve essere espressa in mm, indipendentemente dalle unità del documento Rhino. La superficie della sezione deve avere **una faccia piana nel piano XY**, con contorno e fori poligonali chiusi; le barre usano le stesse coordinate XY. `Section Coordinate System` fornisce un riferimento con assi globali nel baricentro.

Nelle opzioni di taglio la direzione X usa V1/M2 e la direzione Y V2/M1; le quantità (bw, d, Asl, Asw) sono riferite agli assi del checker. `Rectangular Shear Options` riprende la geometria dei componenti 0.2: copriferro alla **superficie della barra longitudinale**, bw = H per X e bw = B per Y. `Circular Shear Options` usa il copriferro alla **staffa esterna** e il modello GPC per pile circolari NTC 2018, da verificare con `Shear Check` in direzione 2 (risultante).

## Dipendenze e architettura

| Progetto/cartella | Responsabilità |
|---|---|
| `src/ConcreteSectionChecker.Grasshopper` | Componenti (`Components`, `Features`), tipi Grasshopper persistenti, calcolo in background, preview, icone |
| `src/ConcreteSectionChecker.Adapters` | Adattamento ai contratti GPC, validazione, convenzioni e unità, report CSV/JSON, mesh e colori delle tensioni, persistenza, controllo del set GPC |
| `lib/GPC` | Set binario GPC fissato, con manifest e hash |
| `tests/ConcreteSectionChecker.Checks` | Prove numeriche, regressioni, fotografia della palette, benchmark |
| `tools/IconGenerator` | Generatore riproducibile delle icone SVG/PNG e del simbolo |
| `tools/ExampleGenerator` | Generatore e controlli numerici dei 20 esempi `.gh`/`.ghx`, con confronto rispetto a 0.2.2 |
| `tools/RhinoExampleProbe.py` | Prova dei 20 esempi dentro Rhino 8 |

ReBear **0.3.0** usa il **set GPC ufficiale del 27 ottobre 2026**, con Model `2acc60ef` e Checker `4a1b0090`, invariato rispetto a 0.2.2 e comune con BridGHe 0.2.0. Il manifest contiene `candidate=false`.

| Assembly | Versione |
|---|---|
| GPCUtilities | 3.0.0.0 |
| GPCGeometry | 2.3.0.0 |
| DelaunayMesh | 2.0.0.14 |
| GPCModel | 5.21.0.1 |
| GPCModelData | 0.3.0.2 |
| GPCChecker.Concrete | 0.25.0.0 |
| MathNet.Numerics, via NuGet | 5.0.0 |

`lib/GPC/release-set.json` è il manifest completo del rilascio di origine: il plugin ne usa sei DLL e lo distribuisce per il controllo al caricamento. I progetti non dipendono dai percorsi dei repository fratelli. Per aggiornare GPC, sostituire il set coerente e il relativo manifest, quindi rieseguire le prove; non aggiornare una DLL isolata.

ReBear segue i default GPC: legge parabola-rettangolo analitica del calcestruzzo e, dalla 0.3, strategia **Iterative** del punto di dominio ovunque (Intersection resta selezionabile in `Concrete Checker Options`). Gli ID normativi storici restano `0=NTC2018`, `1=EN 1992-1-1`, `2=UNI EN 1992-1-1`: identificano i profili implementati nella versione GPC fissata, non implicano l'ultima edizione di una norma. ReBear non aggiunge una propria interpretazione normativa; modello, riferimenti e dettagli restituiti da GPC sono riportati negli esiti.

## Documentazione

- [Migrazione 0.2 → 0.3, differenze nei risultati e accettazione](docs/MIGRATION.md).
- [Catalogo dei 58 componenti con porte e unità](docs/COMPONENTS.md).
- [Palette completa, sottogruppi e ordine di utilizzo](docs/PALETTE.md).
- [Novità della versione 0.3](docs/NEW_COMPONENTS.md).
- [Flussi di lavoro, convenzioni e limiti dei controlli](docs/WORKFLOWS.md).
- [20 esempi Grasshopper svolti, con risultati e spiegazioni](examples/README.md).
- [Sviluppi successivi e funzionalità differite](docs/ROADMAP.md).
- [Grafica e rigenerazione delle icone](assets/branding/README.md).

I dati internalizzati nelle porte (norme, materiali, sezioni, checker, azioni, opzioni) sono salvati nel file con schema versionato; i risultati di calcolo non lo sono e vanno ricalcolati dai componenti a monte. I componenti segnalano esplicitamente un dato non ricostruibile.
