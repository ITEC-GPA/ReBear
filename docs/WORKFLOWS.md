# Convenzioni dei nuovi flussi

## Inviluppi e combinazioni

Load Envelope tratta la lista del ramo corrente. Trasporta le azioni nel riferimento e nell'origine del primo caso e restituisce 12 estremi, ciascuno con indice e **intera combinazione concomitante**. Non usare un vettore ottenuto unendo i massimi indipendenti per la verifica della sezione. Per combinazioni con nome vuoto il batch assegna Caso 1, Caso 2, ecc.; conviene attribuire nomi univoci a monte.

Check Envelope raggruppa per Section ID e tipo di verifica. Un fallimento prevale; in assenza di fallimenti un caso non valutato impedisce l'esito complessivo positivo. I casi non richiesti restano distinti. Il rapporto mancante è null nel JSON e NaN nelle porte numeriche, mai zero. Copriferro, ancoraggi e dettagli conservano il confronto GPC e non inventano un rapporto uniforme fra minimi e massimi.

## Taglio e torsione

- Il componente storico rettangolare verifica entrambe le direzioni.
- Shear Check v2 verifica una direzione: **0 = X, V1/M2; 1 = Y, V2/M1**. Usare due istanze oppure Batch Section Checks con due opzioni.
- Shear Options riceve bw, d, Asl, somma delle aree dei bracci Asw, passo e acciaio delle staffe indipendente. Le quantità sono riferite agli assi di analisi del checker. Non vengono dedotte dalla sagoma T/I/cava.
- Torsion and Shear usa T, Vx e Vy concomitanti, con una cotangente comune fornita dalle opzioni di torsione. Le opzioni dei tagli devono avere staffe a 90°, passo e resistenza dell'acciaio coerenti.
- Ak, uk e tef sono assegnati esplicitamente dal progettista. At è l'area di **un solo braccio** chiuso; ΣAsl è l'armatura aggiuntiva disponibile per torsione, distribuita sul profilo. Confermare chiusura delle staffe e barre agli angoli.

I nuovi checker trasformano le azioni nel riferimento del checker, includendo l'eccentricità fra origini. Le azioni GPC interne sono N/Nmm; le uscite sono kN/kNm. La compressione è negativa.

## Batch, SLE e analisi

Un ramo contiene una sezione e una lista di combinazioni. Le opzioni scollegate escludono il relativo controllo; Run abilita il calcolo. Tenere separate le combinazioni SLU, quasi permanenti, frequenti e caratteristiche: il batch non genera combinazioni né assegna coefficienti alle azioni.

Per SLE usare analisi lineare, coefficienti di omogeneizzazione coerenti e opzioni di combinazione/esposizione/durata esplicite. Il copriferro SLE è alla superficie delle barre longitudinali. Effective Cracked Section conserva un elemento di preview per regione, anche quando la preview non è disponibile.

Moment Curvature richiede il checker non lineare. Il diagramma ha χ in 1/m e M in kNm; controllare Status e le eventuali interruzioni. Domain Slice at N restituisce anche gli sforzi normali effettivamente equilibrati. Verificare la sensibilità al numero di direzioni prima di usare la polilinea come dominio continuo.

Reinforcement Candidate Search sceglie la minima area fra i candidati espliciti che soddisfano tutti i casi N-Mx-My e il rapporto obiettivo. A parità di area mantiene il primo. Non sostituisce verifiche SLE, taglio, dettagli o stabilità.

## Geometria e dettagli

I builder lavorano in mm sul piano XY; le sagome sono centrate rispetto al rettangolo d'ingombro, che per T/I asimmetriche può non coincidere col baricentro. Copriferro in ingresso **alla superficie esterna della staffa**; distanza del centro barra dal bordo = c + Østaffa + Øbarra/2. Le staffe non vengono modellate come barre longitudinali.

La sezione circolare è un poligono inscritto. Il raggio delle barre è ridotto rispetto all'apotema per conservare il copriferro anche sui lati del poligono; area e inerzie dipendono dal numero di segmenti. La sezione cava ha un solo foro rettangolare centrale; sono possibili una corona esterna e una interna.

I builder rifiutano barre esterne, sovrapposte o a contatto. Section Geometry Check controlla i limiti geometrici assegnati; Reinforcement Detailing richiama invece le regole del profilo GPC. Per solette e pareti servono anche le armature secondarie/orizzontali, che la sezione 2D non rappresenta. Le conferme e i dati mancanti producono esiti non valutati, non superati.

Anchorage and Lap riguarda barre tese rettilinee ad aderenza migliorata, con α1…α5 unitari. Non include dettagli di piegatura, ganci o ancoraggio dei cavi.

Gli oggetti di opzione vanno ricostruiti dai rispettivi componenti: non internalizzarli nei parametri Generic. I risultati strutturati Check Result hanno invece persistenza JSON versionata.
