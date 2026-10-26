# Migrazione Rhino2FEM → ReBear

## Provenienza e compatibilità

Sorgente: commit `66555b6c43d10c8929c9871c172f5d78069655a8`, 1 aprile 2026, cartella `Fem2Rhino.Grasshopper.Reading/ConcreteChecker`.

Recuperati 32 componenti e 12 tipi di dato. `tests/Compatibility/components-66555b.json` conserva GUID, SHA-256 dei sorgenti originali e corpi delle registrazioni di input/output. Non rigenerare questa fotografia per far passare un test: descrive il contratto storico.

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

## Verifiche automatiche

Il comando `build/Verify.ps1` comprende:

- GUID univoci e registrazioni delle porte confrontati con il sorgente del commit originale;
- integrità SHA-256 del set GPC;
- taglio senza staffe con un controllo scalare indipendente;
- mappatura delle due direzioni e materiali sui tre profili storici;
- trazione, area esplicita, modello circolare, dati non validi e assenza di staffe;
- tensioni da compressione uniforme confrontate con la soluzione della sezione omogeneizzata;
- durata, esposizione, fessurazione e decompressione;
- resistenza SLU convergente, domanda nulla e N fuori dominio;
- caricamento dei 32 componenti, dimensioni delle icone e contratto dei tipi di dato vuoti;
- chiamata delle implementazioni dei componenti attraverso `IGH_DataAccess`, verificando indici, conversioni, messaggi e `NaN`.

Le tolleranze dei confronti sono nel codice dei test: normalmente errore relativo `1e-9`, `1e-6` per la soluzione tensionale e `1e-3` per l'equilibrio di N nel punto resistente iterativo. Non sono tolleranze di progetto assegnate all'utente.

## Accettazione da eseguire in Rhino

Questa procedura richiede il vero host ed esempi storici rappresentativi; non è sostituita dai test console.

1. Avviare Rhino 8 con il solo nuovo assembly per questi GUID.
2. Aprire una copia di una definizione `.gh` precedente: controllare componenti mancanti, collegamenti e valori persistenti.
3. Ricalcolare un caso SLU e uno SLE noti, verificando assi, segni, unità, stato di convergenza e differenze riportate sopra.
4. Controllare preview, mesh, colori e salvataggio/riapertura del file.
5. Ripetere con gli altri plugin GPC che devono convivere nella sessione.

Non è stata introdotta una nuova serializzazione persistente dei modelli GPC: il recupero di dati personalizzati internalizzati nei vecchi file deve essere verificato con file reali. Le sezioni vengono normalmente ricostruite dai componenti a monte.
