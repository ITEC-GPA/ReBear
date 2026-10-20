# ReBear 0.2 — nuovi componenti

29 componenti aggiuntivi. I 32 componenti storici mantengono GUID e porte. I contratti nuovi sono fissati in `tests/Compatibility/components-0.2.json`.

## 1 Results

| Componente | Funzione | GUID |
|---|---|---|
| Check Envelope | Inviluppo per sezione e verifica nel ramo. I risultati non valutati impediscono un esito positivo completo. | `98dc4c10-e882-41ed-ae13-208c325c0002` |
| Check Report | Tabella CSV e JSON versionato; nessuna scrittura automatica su disco. | `98dc4c10-e882-41ed-ae13-208c325c0004` |
| Check Result | Legge un risultato strutturato senza sostituire i dati mancanti con zero. | `98dc4c10-e882-41ed-ae13-208c325c0003` |
| Load Envelope | Minimi/massimi per ramo, con combinazioni concomitanti. Trasporta tutte le azioni negli assi e origine del primo caso. | `98dc4c10-e882-41ed-ae13-208c325c0001` |

## 2 Checks

| Componente | Funzione | GUID |
|---|---|---|
| Batch Section Checks | Tutte le combinazioni del ramo, esecuzione seriale e inviluppi. Le opzioni scollegate escludono la relativa verifica. | `98dc4c10-e882-41ed-ae13-208c325c0012` |
| Section Resistance Check | Verifica N-Mx-My GPC con esito strutturato e convergenza esplicita. | `98dc4c10-e882-41ed-ae13-208c325c0009` |
| Serviceability Check v2 | Limiti tensionali e fessurazione su analisi GPC, con risultati per gli inviluppi. | `98dc4c10-e882-41ed-ae13-208c325c0011` |
| Shear Check v2 | Verifica GPC con risultato strutturato; mantiene il nome della combinazione. | `98dc4c10-e882-41ed-ae13-208c325c0006` |
| Shear Options | Parametri di una direzione nel riferimento del checker. Acciai indipendenti; d esplicito in mm. | `98dc4c10-e882-41ed-ae13-208c325c0005` |
| SLE Options | Contesto SLE esplicito per limiti tensionali e fessurazione. | `98dc4c10-e882-41ed-ae13-208c325c0010` |
| Torsion Options | Profilo sottile esplicito e staffe chiuse a 90°. Area longitudinale aggiuntiva a quella per flessione. | `98dc4c10-e882-41ed-ae13-208c325c0007` |
| Torsion and Shear | Torsione e interazioni GPC con entrambi i tagli concomitanti. | `98dc4c10-e882-41ed-ae13-208c325c0008` |

## 3 Detailing

| Componente | Funzione | GUID |
|---|---|---|
| Anchorage and Lap | Ancoraggio rettilineo o sovrapposizione di barre tese ad aderenza migliorata; coefficienti α1…α5 = 1. | `98dc4c10-e882-41ed-ae13-208c325c0014` |
| Concrete Cover | Copriferro nominale GPC per le esposizioni e il diametro effettivo specificati. | `98dc4c10-e882-41ed-ae13-208c325c0013` |
| Detailing Options | Ipotesi per le regole di dettaglio GPC. Acciaio unico delle armature; geometria riferita a XY. | `98dc4c10-e882-41ed-ae13-208c325c0016` |
| Plate Detailing Options | Dati delle armature secondarie/orizzontali non ricavabili dalla sezione 2D. | `98dc4c10-e882-41ed-ae13-208c325c0015` |
| Reinforcement Detailing | Minimi/massimi, interferro, staffe e copriferro secondo il profilo GPC; rende visibili le regole in sospeso. | `98dc4c10-e882-41ed-ae13-208c325c0017` |

## 4 Analysis

| Componente | Funzione | GUID |
|---|---|---|
| Domain Slice at N | Sezione Mx–My del dominio a N assegnato; ogni punto è risolto e controllato da GPC. | `98dc4c10-e882-41ed-ae13-208c325c0019` |
| Effective Cracked Section | Regioni efficaci calcolate da GPC per la fessurazione; non ricostruisce formule o zone per approssimazione. | `98dc4c10-e882-41ed-ae13-208c325c0020` |
| Creep and Homogenization | Conversione GPC fra coefficiente viscoso assegnato φ e rapporto modulare n. Non stima φ da età o umidità. | `98dc4c10-e882-41ed-ae13-208c325c0021` |
| Moment Curvature | Analisi GPC a N costante. Checker non lineare; curvatura 1/m e momenti kNm. | `98dc4c10-e882-41ed-ae13-208c325c0018` |
| Reinforcement Candidate Search | Sceglie la minima As fra i checker candidati che soddisfano tutte le combinazioni N-Mx-My. Non verifica taglio, SLE o dettagli. | `98dc4c10-e882-41ed-ae13-208c325c0022` |

## 6 Sections

| Componente | Funzione | GUID |
|---|---|---|
| Box RC Section | Rettangolo con foro centrale. Armatura perimetrale esterna ed eventuale armatura intorno al foro. | `98dc4c10-e882-41ed-ae13-208c325c0028` |
| Circular RC Section | Cerchio discretizzato con poligono inscritto. Barre su un anello; il copriferro minimo è rispettato rispetto ai lati del poligono. | `98dc4c10-e882-41ed-ae13-208c325c0025` |
| I RC Section | Sezione a I con ali anche diverse. Barre sulle facce esterne delle ali e sui lati dell'anima. | `98dc4c10-e882-41ed-ae13-208c325c0027` |
| Rebar Path Layout | Distribuisce barre lungo la curva dei loro centri in XY, per numero o passo massimo. Preparare il percorso con il copriferro desiderato. | `98dc4c10-e882-41ed-ae13-208c325c0029` |
| Rectangular RC Section | Rettangolo centrato nell'origine XY. Barre superiori/inferiori e laterali; geometria in mm. | `98dc4c10-e882-41ed-ae13-208c325c0024` |
| Section Geometry Check | Controllo geometrico di barre, copriferro e interferro. Non sostituisce Reinforcement Detailing. | `98dc4c10-e882-41ed-ae13-208c325c0030` |
| T RC Section | Sezione a T con ala superiore, centrata nel rettangolo d'ingombro XY. Layout iniziale da verificare. | `98dc4c10-e882-41ed-ae13-208c325c0026` |

## Porte e unità

### Anchorage and Lap

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Standard | item | Norma GPC. |
| inputs[1] Concrete | item | Calcestruzzo. |
| inputs[2] Steel | item | Acciaio della barra. |
| inputs[3] Diameter | item | Diametro mm. |
| inputs[4] Design stress | item | Tensione di trazione σsd MPa, ≤ fyd. |
| inputs[5] Available length | item | Lunghezza disponibile mm. |
| inputs[6] Good bond | item | Condizioni di buona aderenza. |
| inputs[7] Lap | item | True sovrapposizione; false ancoraggio. |
| inputs[8] Lapped percentage | item | Percentuale barre sovrapposte. |
| inputs[9] Lap clear distance | item | Interferro fra barre sovrapposte mm. |
| inputs[10] Section ID | item | Identificativo. |
| outputs[0] Result | item | Esito GPC. |
| outputs[1] Required length | item | Lunghezza richiesta mm. |
| outputs[2] Bond strength | item | Resistenza di aderenza MPa. |

### Batch Section Checks

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker con opzioni coerenti con le verifiche richieste. |
| inputs[1] Forces | list | Combinazioni del ramo. |
| inputs[2] Section ID | item | Identificativo sezione. |
| inputs[3] N-Mx-My | item | Eseguire la verifica di resistenza. |
| inputs[4] Shear X | item, opzionale | Opzioni taglio X. |
| inputs[5] Shear Y | item, opzionale | Opzioni taglio Y. |
| inputs[6] Torsion | item, opzionale | Opzioni torsione. |
| inputs[7] SLE | item, opzionale | Opzioni SLE; usare un ramo con combinazioni omogenee. |
| inputs[8] Run | item | Avvia il calcolo. False disabilita il componente. |
| outputs[0] Results | list | Un risultato per caso e verifica, inclusi i non valutati. |
| outputs[1] CSV | item | Tabella dei risultati. |
| outputs[2] Envelope | list | Sintesi per verifica. |

### Box RC Section

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete | item | Materiale calcestruzzo. |
| inputs[1] Steel | item | Acciaio delle barre longitudinali. |
| inputs[2] Width | item | mm. |
| inputs[3] Height | item | mm. |
| inputs[4] Hole width | item | mm. |
| inputs[5] Hole height | item | mm. |
| inputs[6] Link cover | item | Copriferro alla superficie esterna della staffa, mm. |
| inputs[7] Link diameter | item | Diametro staffa usato per posizionare le barre, mm; non definisce il modello di taglio. |
| inputs[8] Bar diameter | item | Diametro delle barre longitudinali, mm. |
| inputs[9] Outer bars per horizontal side | item | Include gli angoli. |
| inputs[10] Outer bars per vertical side | item | Include gli angoli, senza duplicarli. |
| inputs[11] Inner bars per horizontal side | item | 0 senza anello interno, altrimenti almeno 2. |
| inputs[12] Inner bars per vertical side | item | 0 senza anello interno, altrimenti almeno 2. |
| outputs[0] RC section | item | Sezione GPC compatibile con Concrete Checker. |
| outputs[1] Concrete preview | item | Brep XY, mm. |
| outputs[2] Rebars | list | Barre GPC con preview. |
| outputs[3] Concrete area | item | Area lorda al netto dei fori, mm². |
| outputs[4] Steel area | item | Area delle barre, mm². |
| outputs[5] Minimum bar cover | item | Distanza effettiva del calcestruzzo dalla superficie delle barre, mm. |

### Check Envelope

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Results | list | Risultati strutturati ReBear. |
| outputs[0] Group | list | Sezione / verifica. |
| outputs[1] Status | list | Stato complessivo. |
| outputs[2] Governing | list | Caso governante, anche quando non valutabile; null se tutti non richiesti. |
| outputs[3] Pending | list | Numero di casi non valutati. |
| outputs[4] Count | list | Numero totale dei casi. |

### Check Report

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Results | list | Risultati nello stesso ramo. |
| outputs[0] CSV | item | Separatore ;, numeri invariant, campi con escaping. |
| outputs[1] JSON | item | Schema ReBear.CheckReport v1 e versione motore. |
| outputs[2] Summary | list | Sintesi degli inviluppi. |

### Check Result

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Result | item | Risultato ReBear. |
| outputs[0] Section | item | Identificativo sezione. |
| outputs[1] Case | item | Combinazione. |
| outputs[2] Check | item | Verifica. |
| outputs[3] Status | item | Satisfied, NotSatisfied, NotEvaluated, NotRequired. |
| outputs[4] Utilization | item | NaN se non definito. |
| outputs[5] Demand | item | Domanda; NaN se non applicabile. |
| outputs[6] Capacity | item | Capacità/limite; NaN se non applicabile. |
| outputs[7] Unit | item | Unità di domanda e capacità. |
| outputs[8] Message | item | Stato e limitazioni. |
| outputs[9] Reference | item | Riferimento restituito da GPC. |
| outputs[10] Details | item | Dettagli GPC. |

### Circular RC Section

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete | item | Materiale calcestruzzo. |
| inputs[1] Steel | item | Acciaio delle barre longitudinali. |
| inputs[2] Diameter | item | Diametro nominale, mm. |
| inputs[3] Link cover | item | Copriferro alla superficie esterna della staffa, mm. |
| inputs[4] Link diameter | item | Diametro staffa usato per posizionare le barre, mm; non definisce il modello di taglio. |
| inputs[5] Bar diameter | item | Diametro delle barre longitudinali, mm. |
| inputs[6] Bar count | item | Numero barre, almeno 3. |
| inputs[7] Polygon sides | item | Multiplo di 4 fra 16 e 720; controllare sensibilità della discretizzazione. |
| outputs[0] RC section | item | Sezione GPC compatibile con Concrete Checker. |
| outputs[1] Concrete preview | item | Brep XY, mm. |
| outputs[2] Rebars | list | Barre GPC con preview. |
| outputs[3] Concrete area | item | Area lorda al netto dei fori, mm². |
| outputs[4] Steel area | item | Area delle barre, mm². |
| outputs[5] Minimum bar cover | item | Distanza effettiva del calcestruzzo dalla superficie delle barre, mm. |

### Concrete Cover

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Standard | item | Norma GPC. |
| inputs[1] Concrete | item | Calcestruzzo. |
| inputs[2] Exposures | list | Classi ambientali concorrenti, es. XC3, XD1. |
| inputs[3] Actual bar diameter | item | Diametro effettivo rilevante per il copriferro, mm. |
| inputs[4] Adopted cover | item | Copriferro nominale disponibile, mm. |
| inputs[5] Design life | item | Anni, secondo il profilo GPC. |
| inputs[6] Deviation | item | Tolleranza esecutiva, mm. |
| inputs[7] Aggregate | item | Aggregato massimo, mm. |
| inputs[8] Plate element | item | Elemento a piastra / geometria di soletta. |
| inputs[9] Special quality control | item | Controllo speciale confermato; abilita la riduzione GPC. |
| inputs[10] Strength class reduction | item | Abilita il modificatore per resistenza della classe strutturale EN. |
| inputs[11] Rough surface | item | Superficie irregolare. |
| inputs[12] Abrasion | item | 0 nessuna; 1,2,3 classi GPC. |
| inputs[13] Ground | item | 0 cassero; 1 terreno preparato; 2 terreno diretto. |
| inputs[14] Pertinent minimum strength | item, opzionale | Resistenza minima pertinente per NTC, MPa; opzionale. |
| inputs[15] Section ID | item | Identificativo. |
| outputs[0] Result | item | Confronto richiesto/disponibile. |
| outputs[1] Required nominal | item | Copriferro nominale richiesto mm. |
| outputs[2] Durability minimum | item | Per Detailing Options, mm. |
| outputs[3] Cover data | item | Risultato GPC completo. |

### Detailing Options

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Member kind | item | 0 trave, 1 pilastro, 2 soletta, 3 parete. |
| inputs[1] Top tension width | item | Larghezza zona tesa superiore, mm. |
| inputs[2] Bottom tension width | item | Larghezza zona tesa inferiore, mm. |
| inputs[3] Web width | item | Larghezza anima mm. |
| inputs[4] Link diameter | item | Diametro staffa mm, 0 senza staffe. |
| inputs[5] Link spacing | item | Passo staffe mm. |
| inputs[6] Link legs | item | Numero bracci. |
| inputs[7] Nominal cover | item | Copriferro alle staffe; alle barre se senza staffe, mm. |
| inputs[8] Aggregate | item | Aggregato massimo mm. |
| inputs[9] Cover deviation | item | Tolleranza copriferro mm. |
| inputs[10] Durability minimum | item, opzionale | Da Concrete Cover; scollegato = controllo da completare. |
| inputs[11] Lap zone | item | Zona di sovrapposizione. |
| inputs[12] Compression bars restrained | item | Confermare il trattenimento delle barre compresse. |
| inputs[13] End zones confirmed | item | Confermare dettagli delle zone di estremità richiesti da GPC. |
| inputs[14] Plate data | item, opzionale | Richiesto per soletta/parete. |
| outputs[0] Options | item | Opzioni dettagli. |

### Domain Slice at N

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker e tipo di dominio. |
| inputs[1] Axial force | item | kN, compressione negativa. |
| inputs[2] Directions | item | 8–360 direzioni; controllare la sensibilità alla discretizzazione. |
| inputs[3] Run | item | Avvia il calcolo. |
| outputs[0] Mx-My curve | item | Polilinea chiusa in kNm. |
| outputs[1] Mx | list | kNm. |
| outputs[2] My | list | kNm. |
| outputs[3] Solved N | list | N effettivamente equilibrato, kN. |

### Effective Cracked Section

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Crack data | item | Uscita Crack data di Serviceability Check v2. |
| outputs[0] Regions | list | Chiavi delle regioni. |
| outputs[1] Effective concrete area | list | mm². |
| outputs[2] Effective steel area | list | mm². |
| outputs[3] Crack width | list | mm; NaN se non valutata. |
| outputs[4] Regions preview | list | Brep delle regioni, con fori quando presenti. |
| outputs[5] Bar indices | list | Indici base zero delle barre ordinarie contribuenti. |

### Creep and Homogenization

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete | item | Calcestruzzo europeo, Ecm. |
| inputs[1] Steel | item | Acciaio, Es. |
| inputs[2] Input is n | item | True rapporto modulare; false coefficiente viscoso. |
| inputs[3] Value | item | φ oppure n. |
| outputs[0] Creep coefficient | item | φ assegnato/ricavato. |
| outputs[1] Modular ratio | item | Rapporto Es/Ec,eff. |
| outputs[2] Effective concrete modulus | item | MPa. |

### I RC Section

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete | item | Materiale calcestruzzo. |
| inputs[1] Steel | item | Acciaio delle barre longitudinali. |
| inputs[2] Top flange width | item | mm. |
| inputs[3] Bottom flange width | item | mm. |
| inputs[4] Height | item | mm. |
| inputs[5] Web width | item | mm. |
| inputs[6] Top flange thickness | item | mm. |
| inputs[7] Bottom flange thickness | item | mm. |
| inputs[8] Link cover | item | Copriferro alla superficie esterna della staffa, mm. |
| inputs[9] Link diameter | item | Diametro staffa usato per posizionare le barre, mm; non definisce il modello di taglio. |
| inputs[10] Bar diameter | item | Diametro delle barre longitudinali, mm. |
| inputs[11] Top bars | item | Barre superiori. |
| inputs[12] Bottom bars | item | Barre inferiori. |
| inputs[13] Web bars per face | item | Barre laterali nell'anima. |
| outputs[0] RC section | item | Sezione GPC compatibile con Concrete Checker. |
| outputs[1] Concrete preview | item | Brep XY, mm. |
| outputs[2] Rebars | list | Barre GPC con preview. |
| outputs[3] Concrete area | item | Area lorda al netto dei fori, mm². |
| outputs[4] Steel area | item | Area delle barre, mm². |
| outputs[5] Minimum bar cover | item | Distanza effettiva del calcestruzzo dalla superficie delle barre, mm. |

### Load Envelope

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Forces | list | Combinazioni nello stesso ramo; N/Nmm. |
| outputs[0] Extremum | list | N min/max, Vx min/max, Vy min/max, T min/max, Mx min/max, My min/max. |
| outputs[1] Value | list | Forze kN; momenti kNm. |
| outputs[2] Case index | list | Indice originale, base zero. |
| outputs[3] Concomitant forces | list | Tutta la combinazione governante; non un vettore sintetico di massimi. |

### Moment Curvature

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker non lineare. |
| inputs[1] Axial force | item | kN, compressione negativa. |
| inputs[2] Moment direction | item | Gradi negli assi del checker. |
| inputs[3] Steps | item | 10–500 passi. |
| inputs[4] End fraction | item | Frazione del momento limite (0,1]. |
| inputs[5] Axial tolerance | item | Tolleranza sull'equilibrio di N, kN. |
| inputs[6] Run | item | Avvia il calcolo. |
| outputs[0] Diagram | item | Polilinea (χ in 1/m, M in kNm). |
| outputs[1] Curvature | list | 1/m. |
| outputs[2] Moment | list | kNm. |
| outputs[3] Yield curvature | item | 1/m, NaN se non raggiunta. |
| outputs[4] Ultimate curvature | item | 1/m, NaN se non raggiunta. |
| outputs[5] Status | item | Convergenza, eventuale interruzione e limiti. |
| outputs[6] Response | item | Risultato GPC completo. |

### Plate Detailing Options

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Secondary steel | item | Somma delle due facce, mm²/m. |
| inputs[1] Secondary spacing | item | Passo mm; 0 dato mancante, esito da completare. |
| inputs[2] Critical region | item | Zona critica della soletta. |
| outputs[0] Plate data | item | Opzioni piastra/parete. |

### Rebar Path Layout

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Bar centre path | item | Curva dei centri in XY, mm; aperta o chiusa. |
| inputs[1] Steel | item | Acciaio delle barre. |
| inputs[2] Diameter | item | mm. |
| inputs[3] Count | item | 0 = usa passo massimo; altrimenti numero assegnato. |
| inputs[4] Maximum path spacing | item | Passo lungo la curva, mm. |
| outputs[0] Rebars | list | Barre per Reinforced Concrete Section. |
| outputs[1] Centres | list | Centri in XY. |
| outputs[2] Path spacing | item | Passo effettivo lungo la curva, mm. |

### Rectangular RC Section

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete | item | Materiale calcestruzzo. |
| inputs[1] Steel | item | Acciaio delle barre longitudinali. |
| inputs[2] Width | item | Dimensione X, mm. |
| inputs[3] Height | item | Dimensione Y, mm. |
| inputs[4] Link cover | item | Copriferro alla superficie esterna della staffa, mm. |
| inputs[5] Link diameter | item | Diametro staffa usato per posizionare le barre, mm; non definisce il modello di taglio. |
| inputs[6] Bar diameter | item | Diametro delle barre longitudinali, mm. |
| inputs[7] Top bars | item | Barre fila superiore. |
| inputs[8] Bottom bars | item | Barre fila inferiore. |
| inputs[9] Side bars per face | item | Barre su ciascun lato, escluse le barre delle file superiore/inferiore. |
| outputs[0] RC section | item | Sezione GPC compatibile con Concrete Checker. |
| outputs[1] Concrete preview | item | Brep XY, mm. |
| outputs[2] Rebars | list | Barre GPC con preview. |
| outputs[3] Concrete area | item | Area lorda al netto dei fori, mm². |
| outputs[4] Steel area | item | Area delle barre, mm². |
| outputs[5] Minimum bar cover | item | Distanza effettiva del calcestruzzo dalla superficie delle barre, mm. |

### Reinforcement Detailing

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Sezione e norma. |
| inputs[1] Options | item | Da Detailing Options. |
| inputs[2] Minimum axial force | item | Massima compressione fra le combinazioni, kN negativa. |
| inputs[3] Section ID | item | Identificativo. |
| outputs[0] Results | list | Un risultato per regola. |
| outputs[1] CSV | item | Dettagli e confronti GPC. |

### Reinforcement Candidate Search

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Candidate checkers | list | 1–100 candidati espliciti; vincoli geometrici definiti a monte. |
| inputs[1] Forces | list | Combinazioni N-Mx-My, massimo 1000. |
| inputs[2] Target utilization | item | 0 < η ≤ 1. |
| inputs[3] Run | item | Avvia ricerca seriale. |
| outputs[0] Selected index | item | -1 se nessun candidato idoneo. |
| outputs[1] Selected checker | item | Checker selezionato. |
| outputs[2] All results | list | Esiti di ogni candidato e combinazione. |
| outputs[3] Selected steel area | item | mm², NaN se nessun candidato. |

### Section Resistance Check

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker SLU/SLV. |
| inputs[1] Force | item | Azioni N/Nmm. |
| inputs[2] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Result | item | Esito N-Mx-My. |

### Section Geometry Check

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Section | item | Sezione GPC. |
| inputs[1] Required bar cover | item | Copriferro geometrico minimo alle barre, mm. |
| inputs[2] Required clear spacing | item | Interferro minimo assegnato, mm. |
| inputs[3] Section ID | item | Identificativo. |
| outputs[0] Result | item | Esito dei soli vincoli geometrici assegnati. |
| outputs[1] Minimum bar cover | item | mm. |
| outputs[2] Minimum clear spacing | item | mm. |
| outputs[3] Steel area | item | mm². |

### Serviceability Check v2

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Stress result | item | Risultato di Stress Analysis. |
| inputs[1] Options | item | Da SLE Options. |
| inputs[2] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Results | list | Tensioni e fessurazione. |
| outputs[1] Crack data | item | Risultato GPC per Effective Cracked Section. |

### Shear Check v2

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker della sezione. |
| inputs[1] Force | item | Combinazione, N/Nmm. |
| inputs[2] Options | item | Da Shear Options. |
| inputs[3] Direction | item | 0 = X (V1/M2), 1 = Y (V2/M1). |
| inputs[4] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Result | item | Risultato strutturato ReBear. |

### Shear Options

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Link steel | item | Acciaio delle staffe. |
| inputs[1] Longitudinal steel | item | Acciaio delle barre longitudinali: Es per i modelli GPC. |
| inputs[2] Web width | item | Larghezza resistente ortogonale al taglio, mm. |
| inputs[3] Effective depth | item | Altezza utile lungo la direzione del taglio, mm. |
| inputs[4] Longitudinal area | item | Armatura longitudinale tesa e ancorata, mm². |
| inputs[5] Link area | item | Somma delle aree dei bracci nella direzione, mm²; 0 senza staffe. |
| inputs[6] Spacing | item | Passo mm; positivo anche senza staffe. |
| inputs[7] Link angle | item | Gradi [45,90]. |
| inputs[8] Cot theta | item | 0 = scelta GPC; altrimenti valore assegnato. |
| inputs[9] Lever factor | item | Rapporto esplicito (0,0.9]. |
| inputs[10] Aggregate | item | Diametro massimo aggregato, mm. |
| inputs[11] Axial eccentricity | item | Eccentricità MC2010 rispetto a metà z, mm; positiva verso il corrente teso. |
| outputs[0] Options | item | Opzioni taglio di una direzione. |

### SLE Options

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Combination | item | 0 quasi permanente; 1 frequente; 2 caratteristica. |
| inputs[1] Exposure | item | Codice della classe ambientale, per esempio XC3. |
| inputs[2] Sensitive reinforcement | item | Sensibilità delle armature nel criterio GPC. |
| inputs[3] Short term | item | True breve durata; false lunga durata. |
| inputs[4] Ribbed bars | item | Barre ad aderenza migliorata. |
| inputs[5] Longitudinal bar cover | item | Copriferro alla superficie delle barre longitudinali (include il diametro staffa), mm. |
| inputs[6] Design crack limit | item, opzionale | Limite assegnato, mm; scollegato = norma. |
| inputs[7] Spacing override | item, opzionale | Spaziatura assegnata mm; scollegato = geometria. |
| inputs[8] Formula cover override | item, opzionale | Copriferro della formula GPC mm; scollegato = c. |
| inputs[9] Concentric rings | item | Conferma disposizione in anelli concentrici per sezioni circolari. |
| outputs[0] Options | item | Opzioni SLE. |
| outputs[1] Exposure catalogue | list | Codici GPC disponibili. |

### T RC Section

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete | item | Materiale calcestruzzo. |
| inputs[1] Steel | item | Acciaio delle barre longitudinali. |
| inputs[2] Flange width | item | mm. |
| inputs[3] Height | item | mm. |
| inputs[4] Web width | item | mm. |
| inputs[5] Flange thickness | item | mm. |
| inputs[6] Link cover | item | Copriferro alla superficie esterna della staffa, mm. |
| inputs[7] Link diameter | item | Diametro staffa usato per posizionare le barre, mm; non definisce il modello di taglio. |
| inputs[8] Bar diameter | item | Diametro delle barre longitudinali, mm. |
| inputs[9] Top bars | item | Barre superiori nell'ala. |
| inputs[10] Bottom bars | item | Barre inferiori nell'anima. |
| inputs[11] Web bars per face | item | Barre aggiuntive su ciascun lato dell'anima. |
| outputs[0] RC section | item | Sezione GPC compatibile con Concrete Checker. |
| outputs[1] Concrete preview | item | Brep XY, mm. |
| outputs[2] Rebars | list | Barre GPC con preview. |
| outputs[3] Concrete area | item | Area lorda al netto dei fori, mm². |
| outputs[4] Steel area | item | Area delle barre, mm². |
| outputs[5] Minimum bar cover | item | Distanza effettiva del calcestruzzo dalla superficie delle barre, mm. |

### Torsion Options

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Enclosed area | item | Area racchiusa dalla linea media, mm². |
| inputs[1] Midline perimeter | item | Perimetro linea media, mm. |
| inputs[2] Effective thickness | item | Spessore efficace, mm. |
| inputs[3] Link steel | item | Acciaio delle staffe chiuse. |
| inputs[4] Longitudinal steel | item | Acciaio longitudinale per torsione. |
| inputs[5] One leg area | item | Area di un solo braccio di staffa chiusa, mm². |
| inputs[6] Spacing | item | Passo staffe mm. |
| inputs[7] Torsion longitudinal area | item | Area aggiuntiva disponibile per torsione, distribuita sul profilo, mm². |
| inputs[8] Cot theta | item | Inclinazione comune a taglio/torsione. |
| inputs[9] Hollow, both faces reinforced | item | Sezione cava con armature su entrambe le facce delle pareti. |
| inputs[10] Closed links and distributed bars confirmed | item | Confermare staffe chiuse e barre longitudinali distribuite sul profilo, con barre agli angoli. |
| outputs[0] Options | item | Opzioni torsione. |

### Torsion and Shear

| Porta | Tipo di accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker della sezione. |
| inputs[1] Force | item | Combinazione completa, N/Nmm. |
| inputs[2] Torsion options | item | Da Torsion Options. |
| inputs[3] Shear X options | item, opzionale | Richieste se Vx non nullo. |
| inputs[4] Shear Y options | item, opzionale | Richieste se Vy non nullo. |
| inputs[5] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Result | item | Utilizzo massimo fra torsione e interazioni; dettagli in Check Result. |

