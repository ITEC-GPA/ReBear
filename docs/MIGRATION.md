# Migrazione Rhino2FEM → ReBear

## Aggiornamento 0.3.0 — 29 ottobre 2026

ReBear 0.3 unifica **tutti i componenti** sul modello `ReBearComponent`: porte dichiarate una sola volta con unità e descrizione, risultati strutturati (`CheckResult`), `NaN` — mai zero — per i valori mancanti, numeri come numeri (nessuna uscita testuale con valori, nessun arrotondamento). La palette passa da 61 a **58 componenti**. Il set GPC in `lib/GPC` non cambia (set ufficiale del 27 ottobre, comune con BridGHe).

### Regola dei GUID

Un componente le cui porte (nome, tipo, accesso o ordine) o il cui significato cambiano riceve un **nuovo `ComponentGuid`** e il vecchio viene rimosso: Grasshopper altrimenti ricollegherebbe in silenzio le porte spostate. Aprendo un file 0.2 questi componenti risultano mancanti e vanno sostituiti seguendo la tabella. I **45 componenti** che conservano il GUID hanno porte identiche a 0.2.2; la verifica automatica confronta la palette con `tests/Compatibility/palette-0.2.2.json` e con la fotografia completa `tests/Compatibility/palette-0.3.json`.

### Componenti 0.2.2 → 0.3

| Componente 0.2.2 | Componente 0.3 | GUID | Cambiamenti |
|---|---|---|---|
| Rectangular Section Shear Check | **Rectangular Shear Options** + **Shear Check** (Direction 0 e 1) | rimosso | Le opzioni X/Y si costruiscono dalla stessa geometria (copriferro alla superficie delle barre); acciaio delle staffe indipendente; σcp dall'area della sezione del checker, non da una porta separata. Report con Check Report. |
| Rectangular Section Shear Check No stirrup | **Rectangular Shear Options** (Øw = 0) + **Shear Check** (Direction 1) | rimosso | Senza staffe: Asw = 0. I nickname incrociati Vrds/Vrdc non esistono più: VRsd/VRcd sono nei dettagli del risultato. |
| Circular Section Shear Check | **Circular Shear Options** + **Shear Check** (Direction 2, risultante) | rimosso | Stesso modello GPC per pile circolari NTC (z/d GPC, V risultante); M risultante passato al motore. |
| Serviceability Stress Check | **SLE Options** + **Serviceability Check** | rimosso | Esito "SLE stress" strutturato; tensioni nei punti da View Strain Analysis Results. |
| Serviceability Crack Width Check | **SLE Options** + **Serviceability Check** + Effective Cracked Section | rimosso | Esito "SLE cracking" con criterio e stato GPC; classe ambientale come codice (XC3…) invece dell'indice. |
| Print Stress Analysis Results | **Stress Analysis Summary** | nuovo | Niente "To Excel": uscita Summary per Check Report (CSV/JSON). Estremi assenti `NaN` (B1), nessun arrotondamento; deformazioni in ‰. |
| 3D Failure Domain Analysis | **3D Failure Domain Analysis** | nuovo | Niente "To Excel": Results (un esito N-Mx-My per combinazione) e porta Section ID; punti di domanda/resistenza e utilizzo invariati. |
| Concrete Checker Options | **Concrete Checker Options** | nuovo | Porta θ discretization (8–360, default 64); enumerazioni validate (C6); default Iterative. |
| Concrete Cover | **Concrete Cover** | nuovo | Diametro della specifica e diametro effettivo separati (C7: GPC riceveva due volte il copriferro nominale); una porta per ogni modificatore EN (soletta, controllo qualità, resistenza) e NTC (piastra, riduzione per controllo qualità). Senza diametro effettivo l'esito è NotEvaluated, salvo copriferro già insufficiente. |
| Secton Coordinate System | **Section Coordinate System** | nuovo | Assi X/Y globali nel baricentro. In 0.2 gli assi erano (−X, −Y), quelli interni del solver: Mx e My dati in quel riferimento avevano segno opposto alla convenzione globale. |
| Concrete Material Design Value / Steel Material Design Value | stessi nomi | nuovo | Uscite numeriche (MPa, deformazioni adimensionali) invece di testi; ingresso "Steel Material" del secondo componente (era "Concrete Material"). |
| Rebar Element | **Rebar Element** | nuovo | Diametro in mm (numero) invece dell'indice della lista 6…32; barre fuori dal piano XY o di acciaio da precompressione respinte. |
| Tendon Element | **Tendon Element** | nuovo | Porta "Diameter" (era "Diamater"), nickname σp della tensione iniziale in MPa. |
| Concrete / Homogenized Mechanical Properties | stessi nomi | nuovo | Angolo principale in gradi (era in radianti) con porta "Principal angle". |
| Secton Resistance Summary | **Section Resistance Summary** | invariato | B4: assi globali nel baricentro. In 0.2 MxRd+ e MxRd− (e MyRd+, MyRd−) erano scambiati; strategia Iterative al posto di Intersection; nessun arrotondamento a 2 decimali. Sezione a T 800/300×700: MxRd+ pl da 430.62 a **508.48 kNm**, MxRd− pl da −508.48 a **−430.62 kNm**. |
| Shear Check v2 / Serviceability Check v2 | **Shear Check** / **Serviceability Check** | invariato | Solo il nome; Shear Check accetta Direction = 2 (risultante). |
| Batch Section Checks, Reinforcement Candidate Search, Domain Slice at N, Moment Curvature | stessi nomi | invariato | Calcolo in background con annullamento (menu contestuale "Annulla calcolo"; un nuovo calcolo annulla il precedente) su una copia privata del checker; ricerca dei candidati in parallelo. |
| Force component, Stress Analysis, verifiche singole | stessi nomi | invariato | Combinazioni senza nome: "Caso k" con k posizione nella lista o iterazione del componente (C8), non più tutte "Caso 1". Stress Analysis non ricostruisce il checker. |
| View Strain Analysis Results, Result Color Mesh | stessi nomi | invariato | B5: piano senza curvatura (azioni nulle, compressione uniforme) → nessun asse neutro, avviso, mesh non suddivisa (era un'eccezione). L'asse neutro attraversa l'intera sezione; valori non arrotondati. |
| Color Mesh | **Color Mesh** | invariato | Numero di colori diverso dai punti (e non unico): errore esplicito invece di nessuna uscita. |
| Check Report | **Check Report** | invariato | Unico report: esiti oppure riepiloghi tensionali, CSV `;` invariant con escaping, valore mancante = campo vuoto. |
| Tutti gli altri | stessi nomi | invariato | Porte identiche; descrizioni con unità; messaggi che indicano la porta. |

### Comportamenti comuni

- **Strategia del punto di dominio: Iterative ovunque.** Era già il default della porta di Concrete Checker Options; ora lo usano anche il riepilogo resistente, gli esempi e i test. Intersection resta selezionabile: precalcola il dominio discretizzato (0.4–0.6 s per checker contro 2–4 ms) e interpola i punti.
- **Errori inattesi**: invece di "Solution exception" il messaggio indica componente, metodo, iterazione e un riepilogo degli ingressi (C5).
- **Dati internalizzati** (C4): norma, materiali (per nome del database GPC, verificati alla lettura), riferimenti, opzioni del checker, barre, sezioni, checker, azioni e opzioni di taglio/torsione/SLE/dettagli sono salvati nel file con schema versionato. I risultati (analisi tensionali, domini, fessurazione) non sono salvati: alla riapertura il dato è un segnaposto non valido e il componente a valle indica di ricollegare i componenti a monte. I dati internalizzati con 0.2 non contenevano nulla e producono lo stesso messaggio.
- **Copie** (C3): le opzioni del checker sono mutabili e il solver GPC ne conserva il riferimento: Concrete Checker ne usa una copia privata e `Duplicate` copia opzioni e riferimenti. Gli altri oggetti GPC non sono modificati da ReBear e restano condivisi.
- **Set GPC caricato** (C12): al caricamento ReBear confronta versione e SHA-256 degli assembly GPC presenti nel processo con `release-set.json`; un set diverso caricato da un altro plugin produce un messaggio nella riga di comando di Rhino e un avviso su ogni componente ReBear.
- **Build** (C9): la post-build Release non elimina più `artifacts/packages`; la pulizia dei pacchetti precedenti è in `build/Package.ps1` (`-KeepPrevious` per conservarli). Rimosso `DisableFastUpToDateCheck`.
- **Convenzione dei momenti** (GPC, invariata): negli assi globali un Mx positivo comprime le fibre con y negativa.

### Differenze numeriche negli esempi

I 20 esempi sono rigenerati con i soli componenti 0.3 e superano i propri controlli; ogni README riporta il confronto con i risultati 0.2.2. Cambiano soltanto gli esempi con punti resistenti (strategia Iterative); nessun esito e nessuna scelta cambiano.

| Esempio e risultato | 0.2.2 (Intersection) | 0.3 (Iterative) | Δ relativo |
|---|---:|---:|---:|
| 10, η N-Mx-My SLU-moderato | 0.317753025 | 0.317680515 | 2.3e-4 |
| 10, η N-Mx-My SLU-elevato | 1.52227149 | 1.52210676 | 1.1e-4 |
| 18, momento limite [kNm] | 262.765218 | 262.793655 | 1.1e-4 |
| 18, curvatura ultima χu [1/m] | 0.0374733501 | 0.0374991089 | 6.9e-4 |
| 19, My massimo [kNm] | 125.198017 | 125.208397 | 8.3e-5 |
| 19, punti della sezione del dominio | — | — | ≤ 1.7e-3 del momento massimo |
| 20, η del candidato Ø20, SLU1 / SLU2 | 0.687471856 / 0.620693418 | 0.687315285 / 0.62054976 | 2.3e-4 |

Con Intersection lo sforzo normale del punto risolto coincide esattamente con quello richiesto (interpolazione sul dominio); con Iterative resta un residuo entro la tolleranza GPC (esempio 19: al massimo 6.3e-4 kN).

### Accettazione in Rhino

`tools/RhinoExampleProbe.py` apre ciascuno dei 20 esempi in Rhino 8, risolve la definizione attendendo i componenti in background e registra per componente messaggi, conteggi e campioni delle uscite in `artifacts/example-check/<esempio>.json`. L'esecuzione nell'host resta da fare.

## Provenienza e compatibilità (0.1–0.2)

Le sezioni seguenti descrivono la migrazione da Rhino2FEM e le versioni 0.2.x; dove contraddicono la sezione 0.3 vale quest'ultima.

Sorgente: commit `66555b6c43d10c8929c9871c172f5d78069655a8`, 1 aprile 2026, cartella `Fem2Rhino.Grasshopper.Reading/ConcreteChecker`.

Recuperati 32 componenti e 12 tipi di dato. `tests/Compatibility/components-66555b.json` conserva GUID, SHA-256 dei sorgenti originali e corpi delle registrazioni di input/output del contratto storico; dalla 0.3 non è più un vincolo dei test.

Sono invariati i GUID, nomi/nickname delle porte, tipi, ordine, accesso item/list e valori di default. I namespace storici dei componenti e dei tipi di dato sono conservati. Il nuovo assembly ha un proprio ID (`7ca34ed5-c328-465a-9940-b4c4f3d924b3`) e il nome `ReBear.gha`; la palette passa da Fem4Checks a ReBear.

Non vengono migrati altri moduli Rhino2FEM. I repository di origine e delle librerie non sono modificati.

## Aggiornamento 0.2.1 — 26 ottobre 2026

Aggiornato il set ufficiale GPC del 26 ottobre 2026: Model **5.21.0.1**, ModelData **0.3.0.2**, Checker.Concrete **0.21.0.0**. I commit di origine sono `2acc60efad0924b9b05aafdd75588f13842904a1` per Model e `799a63556e97f042d95d70c86db1a291ee47f2d4` per Checker, corrispondenti ai rispettivi master remoti al controllo del 26 ottobre. Utilities, Geometry e DelaunayMesh conservano gli stessi hash. Il manifest ufficiale con gli esiti delle suite delle librerie è in `lib/GPC/release-set.json`.

Il set introduce il default analitico della legge parabola-rettangolo del calcestruzzo e aggiornamenti del solver di dominio. ReBear utilizza le API e i default del motore aggiornato; non forza il precedente metodo tabulato. L'identità numerica con 0.2.0 non è quindi attesa nei calcoli non lineari. Le formule restano nelle librerie GPC, senza modifiche ai GUID o alle porte dei 61 componenti.

La Release supera i 25 gruppi di controlli numerici e di caricamento di `build/Verify.ps1`. Anche i 20 esempi ricalcolati separatamente superano i propri controlli. Risultati e spiegazioni sono aggiornati conservando le definizioni GH/GHX e verificando l'identità dei 20 grafi di input. Nessun esito `Satisfied`/`NotSatisfied`/`NotEvaluated` cambia nei casi degli esempi. Il ricalcolo nell'host Rhino rimane da eseguire.

Differenze rappresentative rilevate a parità di input:

| Esempio e risultato | ReBear 0.2.0 | ReBear 0.2.1 |
|---|---:|---:|
| 10, rapporto N-Mx-My del caso SLU moderato | 0.317774615 | 0.317753025 |
| 18, momento limite [kNm] | 262.745258 | 262.765218 |
| 18, curvatura ultima [1/m] | 0.037422928 | 0.037473350 |
| 19, My massimo del dominio [kNm] | 125.109475 | 125.198017 |

Variazioni numeriche superiori alla soglia di confronto (`1e-9` relativo, `1e-10` assoluto) compaiono negli esempi 10, 18, 19 e 20. Nell'esempio 09 cambia il riferimento testuale della verifica di torsione restituito da GPC; negli altri esempi i risultati coincidono entro tale soglia. La ricerca dell'esempio 20 seleziona ancora il candidato di indice 1, con barre Ø20 e As = 2513.274123 mm². Questi confronti descrivono i casi provati, non una stima dell'errore per sezioni generiche.

## Calcolo delegato alle librerie

| Ambito | API utilizzata | Comportamento |
|---|---|---|
| Taglio | `SectionShearCalculator`, `ShearProfiles`, `ShearLeverArm` | Materiali e profilo dal checker; conversione geometria/azioni nell'adapter |
| Limiti tensionali SLE | `StressLimitCheck`, `StressLimitContext` | Limiti e rapporti definiti dalla libreria |
| Fessurazione SLE | `SectionCrackCheck`, `CrackSectionGeometry`, `CrackRequirements` | Criterio, zona efficace, apertura, limiti ed esito GPC |
| SLU puntuale | `LegacySectionCalculation.SolveResistance` | Convergenza, domanda nulla e sforzo normale fuori dominio espliciti |
| Analisi tensionale | solver GPC e `LegacySectionCalculation.Capture` | Risultati accettati solo con analisi completata |

Gli adapter non implementano formule normative. Restano operazioni geometriche (per esempio area di una barra), conversioni di unità e mappatura dei dati. Le formule scalari presenti nei test servono a un controllo indipendente di casi semplici.

## Differenze intenzionali rispetto al codice storico

### Taglio

- Si usa il profilo normativo effettivamente scelto. Il vecchio componente non deve imporre coefficienti propri a un checker di un'altra norma.
- L'area esplicita del calcestruzzo entra nel calcolo. Una trazione assiale non viene trasformata in una compressione favorevole tramite valore assoluto.
- Le verifiche non valutabili danno `NaN` e un messaggio; l'assenza delle staffe non dà un utilizzo nullo sulle uscite relative al modello armato.
- Le direzioni X/Y conservano il significato geometrico e l'ordine storico delle uscite.
- Il modello circolare recuperato usa la risultante dei tagli e il braccio del modello GPC per pile circolari NTC. Altri profili sono respinti per quel componente: non viene esteso implicitamente un modello fuori dal suo campo.
- Precompressione, assenza del materiale delle barre e materiali longitudinali incompatibili con l'unico acciaio delle staffe non vengono interpretati come casi ordinari.
- Diametri, area, altezza utile, passo, conteggi interi e dati non finiti sono validati.
- Le due vecchie uscite `Vrds`/`Vrdc` del componente senza staffe mantengono anche i nickname storicamente incrociati. Usare l'intestazione del report per distinguere i termini GPC.

### Stati limite di esercizio

- Gli ID storici delle combinazioni (`0=quasi permanente`, `1=frequente`, `2=caratteristica`) sono mappati esplicitamente; l'ordine dell'enumerazione GPC è diverso.
- I valori di tensione vengono conservati anche quando la combinazione non richiede un rapporto di verifica. In quel caso il rapporto è `NaN`.
- La durata delle azioni è passata come proprietà a GPC. Non si usano i coefficienti invertiti presenti nel vecchio dizionario del componente.
- Le classi ambientali conservano gli ID storici e sono risolte tramite il catalogo GPC.
- La decompressione usa un'analisi ausiliaria non fessurata del checker GPC. Non viene presentata come un confronto `wk / 0`.
- La fessurazione distingue `Satisfied`, `NotSatisfied`, `NotRequired` e `NotEvaluated`. Criterio e motivazione sono visibili nel report e nell'uscita testuale.
- I limiti delle API GPC (per esempio analisi non compatibile o precompressione non supportata dal modello di apertura) sono propagati, senza introdurre formule locali sostitutive.

### Geometria e risultati

- La costruzione della sezione non mantiene più soltanto l'ultima faccia di un Brep multiplo. Richiede una singola faccia poligonale parallela a XY, eventualmente forata.
- Non viene ruotato il solo contorno del calcestruzzo lasciando le barre nel riferimento precedente.
- I tipi Grasshopper vuoti non risultano validi e non falliscono durante la descrizione. La duplicazione crea un nuovo involucro; il modello GPC sottostante resta condiviso.
- La preview delle barre usa bounding box circolari corretti. Le trasformazioni che non mantengono una barra circolare nel piano XY non sono accettate come barre valide.
- Il postprocessing del dominio mantiene il checker che ha prodotto il dominio e usa il suo calcolo GPC per la domanda. Un punto assente non è sostituito con un punto numerico apparentemente valido.

### Report

Le porte `To Excel Header` e `To Excel` restano nelle posizioni originali. Per taglio/SLE il contenuto è stato ristrutturato con esito, modello/riferimento dove restituiti da GPC e dettagli di calcolo. Separatore `;`, formato numerico invariant (punto decimale), escaping CSV. Gli script esterni che leggevano le vecchie colonne devono essere aggiornati usando l'intestazione.

## Verifiche automatiche (0.3)

Il comando `build/Verify.ps1` (40 gruppi) comprende:

- palette identica alla fotografia `palette-0.3.json` (identità, pannelli, porte) e regola dei GUID rispetto a `palette-0.2.2.json`;
- integrità SHA-256 del set GPC e confronto degli assembly caricati con `release-set.json`;
- taglio: controllo scalare indipendente senza staffe, mappatura X/Y delle opzioni rettangolari sui tre profili, trazione non valutata, modello circolare con risultante, dati non validi;
- tensioni da compressione uniforme confrontate con la sezione omogeneizzata; durata, esposizione, fessurazione, decompressione;
- resistenza SLU convergente, domanda nulla e N fuori dominio;
- regressioni B1–B5 e C2–C8 (estremi assenti, CSV in it-IT, torsione con assi ruotati di 90°, riepilogo resistente negli assi globali, piano senza curvatura, mesh e colori, copie delle opzioni, persistenza, copriferro, nomi dei casi);
- analisi ausiliaria non fessurata condivisa nel batch, ricerca in parallelo identica alla seriale, annullamento;
- caricamento dei 58 componenti, icone, tipi di dato vuoti, persistenza nei GH archive;
- chiamata dei componenti attraverso `IGH_DataAccess`, verificando indici, conversioni, messaggi, `NaN` e contesto degli errori inattesi.

Le tolleranze dei confronti sono nel codice dei test: normalmente errore relativo `1e-9`, `1e-6` per la soluzione tensionale e `1e-3` per l'equilibrio di N nel punto resistente iterativo. Non sono tolleranze di progetto assegnate all'utente. `--benchmark` misura costruzione del checker, Batch, Search, Slice e momento–curvatura con le due strategie.

## Accettazione da eseguire in Rhino

Questa procedura richiede il vero host ed esempi storici rappresentativi; non è sostituita dai test console.

1. Avviare Rhino 8 con il solo assembly ReBear 0.3 per questi GUID ed eseguire `tools/RhinoExampleProbe.py` sui 20 esempi.
2. Aprire una copia di una definizione `.gh` 0.2: sostituire i componenti mancanti secondo la tabella 0.2.2 → 0.3, controllare collegamenti e valori persistenti.
3. Ricalcolare un caso SLU e uno SLE noti, verificando assi, segni, unità, stato di convergenza e differenze riportate sopra; controllare che i componenti in background non blocchino l'interfaccia e che "Annulla calcolo" funzioni.
4. Controllare preview, mesh, colori e salvataggio/riapertura del file con dati internalizzati.
5. Ripetere con gli altri plugin GPC che devono convivere nella sessione (BridGHe): nessun avviso del controllo del set GPC.

## Aggiornamento 0.2.2 — integrazione GPC del 26 ottobre 2026

Le sei DLL in `lib/GPC` provengono dal set coerente Model `2acc60ef` / Checker `4a1b0090`, integrato su master. Concrete passa da 0.21.0.0 a **0.25.0.0**; gli altri cinque assembly usati da ReBear restano invariati. I cinque assembly comuni a ReBear e BridGHe provengono dallo stesso set. GUID, porte e formule degli adapter ReBear non sono cambiati.

Il set include le correzioni GPC di copriferro/durabilita, campo di applicazione del taglio, soglia di fessurazione basata sul copriferro reale e solver lineare, oltre alle nuove funzioni per i pali. Queste modifiche delle librerie possono influire sui casi interessati: non si dichiara identita numerica per qualsiasi modello. I **25 gruppi di controllo ReBear** sono passati senza cambiare gli attesi, inclusi i confronti analitici e il caricamento dei **61 componenti**. Le suite complessive GPC superano 9.046 test Checker e 1.493 test Model; tre prove Model con fixture native facoltative non sono state eseguite.

Il collaudo del 26 ottobre usava `candidate=true` con `build/Verify.ps1 -AllowCandidate`. Il **27 ottobre 2026** il push autorizzato di Checker/master è stato completato e il set è stato prodotto ufficialmente da master pubblicati, rieseguendo tutte le suite GPC e il controllo delle versioni rispetto al precedente rilascio. Il manifest corrente è **candidate=false**; tutte le 14 DLL del set, comprese le sei usate da ReBear, hanno SHA-256 identico al candidato collaudato. Per la Release corrente usare `build/Verify.ps1` senza opzioni per candidati. Le versioni degli assembly restano invariate perché non è cambiato il contenuto dei binari GPC.