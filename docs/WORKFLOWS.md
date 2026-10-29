# Convenzioni dei flussi di lavoro

## Inviluppi e combinazioni

Load Envelope tratta la lista del ramo corrente. Trasporta le azioni nel riferimento e nell'origine del primo caso e restituisce 12 estremi, ciascuno con indice e **intera combinazione concomitante**. Non usare un vettore ottenuto unendo i massimi indipendenti per la verifica della sezione. Le combinazioni senza nome diventano `Caso k`: k è la posizione nella lista (Batch, Stress Analysis, ricerca) oppure l'iterazione del componente (Force component e verifiche singole). Conviene comunque attribuire nomi univoci a monte, perché due componenti diversi possono generare lo stesso `Caso 1`.

Check Envelope raggruppa per Section ID e tipo di verifica. Un fallimento prevale; in assenza di fallimenti un caso non valutato impedisce l'esito complessivo positivo. I casi non richiesti restano distinti. Il rapporto mancante è null nel JSON e NaN nelle porte numeriche, mai zero. Copriferro, ancoraggi e dettagli conservano il confronto GPC e non inventano un rapporto uniforme fra minimi e massimi.

## Report

Check Report è l'unico report: riceve gli esiti (`Check Result`) di qualunque verifica oppure i riepiloghi di Stress Analysis Summary, non i due tipi insieme. Il CSV usa `;` come separatore, tutti i campi fra virgolette con escaping, numeri in formato invariant (punto decimale) e un campo vuoto per il valore mancante, indipendentemente dalla lingua di Windows. Il JSON ha schema `ReBear.CheckReport` o `ReBear.StressReport`, versione 1, e la versione del motore GPC. Nessun file viene scritto su disco.

## Taglio e torsione

- Shear Check verifica una direzione: **0 = X, V1/M2; 1 = Y, V2/M1; 2 = risultante** √(V1² + V2²) per sezioni circolari. Usare più istanze oppure Batch Section Checks con le opzioni X e Y.
- Shear Options riceve bw, d, Asl, somma delle aree dei bracci Asw, passo e acciaio delle staffe indipendente. Le quantità sono riferite agli assi di analisi del checker e non vengono dedotte dalla sagoma T/I/cava.
- Rectangular Shear Options costruisce le due direzioni di un rettangolo B × H con la convenzione dei componenti 0.2: copriferro alla superficie delle barre longitudinali, X con bw = H e d = B − c − ØX/2, Y con bw = B e d = H − c − ØY/2, z/d dal profilo GPC. Diametro staffe 0 = elemento senza armatura a taglio.
- Circular Shear Options usa il copriferro alla staffa esterna, d = D − c − Øw − Ø/2 e il braccio GPC per pile circolari NTC 2018 (altri profili sono respinti).
- σcp è calcolato dall'area della sezione del checker.
- Torsion and Shear usa T, Vx e Vy concomitanti, con una cotangente comune fornita dalle opzioni di torsione. Le opzioni dei tagli devono avere staffe a 90°, passo e resistenza dell'acciaio coerenti. Una componente di taglio inferiore a 1e-9 della risultante (o a 1e-6 N), residuo della rotazione degli assi, è considerata nulla.
- Ak, uk e tef sono assegnati esplicitamente dal progettista. At è l'area di **un solo braccio** chiuso; ΣAsl è l'armatura aggiuntiva disponibile per torsione, distribuita sul profilo. Confermare chiusura delle staffe e barre agli angoli.

I checker trasformano le azioni nel riferimento del checker, includendo l'eccentricità fra origini. Le azioni GPC interne sono N/Nmm; le uscite sono kN/kNm. La compressione è negativa. Section Coordinate System e Section Resistance Summary usano gli assi globali nel baricentro; negli assi globali un Mx positivo comprime le fibre con y negativa.

## Batch, SLE e analisi

Un ramo contiene una sezione e una lista di combinazioni. Le opzioni scollegate escludono il relativo controllo; Run abilita il calcolo. Tenere separate le combinazioni SLU, quasi permanenti, frequenti e caratteristiche: il batch non genera combinazioni né assegna coefficienti alle azioni.

Batch Section Checks, Reinforcement Candidate Search, Domain Slice at N e Moment Curvature calcolano in background su una copia privata del checker: Rhino resta utilizzabile, il componente mostra "Calcolo in corso…" e scrive le uscite al termine. Modificare un ingresso annulla il calcolo in corso e ne avvia uno nuovo; il menu contestuale offre "Annulla calcolo". La ricerca verifica i candidati in parallelo, ciascuno con il proprio checker.

Per SLE usare analisi lineare, coefficienti di omogeneizzazione coerenti e opzioni di combinazione/esposizione/durata esplicite. Il copriferro SLE è alla superficie delle barre longitudinali. Nel batch l'analisi ausiliaria non fessurata della decompressione usa un solo checker per sezione e riferimento. Effective Cracked Section conserva un elemento di preview per regione, anche quando la preview non è disponibile.

Stress Analysis Summary riporta gli estremi di tensione (MPa) e deformazione (‰) di calcestruzzo, barre, trefoli e profili in acciaio: una grandezza senza il relativo materiale è `NaN`. View Strain Analysis Results e Result Color Mesh disegnano l'asse neutro attraverso tutta la sezione; con un piano senza curvatura (azioni nulle o compressione uniforme) non c'è asse neutro, la mesh non viene suddivisa e il componente lo segnala.

Moment Curvature richiede il checker non lineare. Il diagramma ha χ in 1/m e M in kNm; controllare Status e le eventuali interruzioni. Domain Slice at N restituisce anche gli sforzi normali effettivamente equilibrati. Verificare la sensibilità al numero di direzioni prima di usare la polilinea come dominio continuo. Con la strategia Iterative (default) i punti sono risolti sulla legge della sezione; con Intersection sono interpolati sul dominio discretizzato con θ discretization direzioni.

Reinforcement Candidate Search sceglie la minima area fra i candidati espliciti che soddisfano tutti i casi N-Mx-My e il rapporto obiettivo. A parità di area mantiene il primo. Non sostituisce verifiche SLE, taglio, dettagli o stabilità.

## Geometria e dettagli

I builder lavorano in mm sul piano XY; le sagome sono centrate rispetto al rettangolo d'ingombro, che per T/I asimmetriche può non coincidere col baricentro. Copriferro in ingresso **alla superficie esterna della staffa**; distanza del centro barra dal bordo = c + Østaffa + Øbarra/2. Le staffe non vengono modellate come barre longitudinali.

La sezione circolare è un poligono inscritto. Il raggio delle barre è ridotto rispetto all'apotema per conservare il copriferro anche sui lati del poligono; area e inerzie dipendono dal numero di segmenti. La sezione cava ha un solo foro rettangolare centrale; sono possibili una corona esterna e una interna.

I builder rifiutano barre esterne, sovrapposte o a contatto. Section Geometry Check controlla i limiti geometrici assegnati; Reinforcement Detailing richiama invece le regole del profilo GPC. Per solette e pareti servono anche le armature secondarie/orizzontali, che la sezione 2D non rappresenta. Le conferme e i dati mancanti producono esiti non valutati, non superati.

Concrete Cover confronta il copriferro adottato con il requisito GPC della specifica dei materiali (diametro di riferimento) e con quello delle barre reali (diametro effettivo); senza diametro effettivo l'esito resta da completare, salvo un copriferro già insufficiente. I modificatori EN (geometria a soletta, controllo di qualità speciale, resistenza) e NTC (elemento a piastra, riduzione per controllo di qualità) hanno porte distinte.

Anchorage and Lap riguarda barre tese rettilinee ad aderenza migliorata, con α1…α5 unitari. Non include dettagli di piegatura, ganci o ancoraggio dei cavi.

## Dati internalizzati

Norme, materiali, riferimenti, opzioni del checker, barre, sezioni, checker, azioni, opzioni di taglio/torsione/SLE/dettagli ed esiti `Check Result` sono salvati nel file con schema versionato. I materiali sono salvati per nome del database GPC e verificati alla lettura. I risultati di calcolo (analisi tensionali, piani di deformazione, domini, fessurazione, momento–curvatura) non sono salvati: dopo la riapertura il dato è segnalato come non salvato e va ricalcolato collegando i componenti a monte.
