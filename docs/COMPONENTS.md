# ReBear 0.3 — catalogo delle porte

58 componenti, generato dal plugin compilato (`--write-palette`). Unità: geometria mm, aree mm², tensioni MPa, forze delle porte kN e momenti kNm salvo diversa indicazione, compressione negativa. Un valore mancante è `NaN`, mai zero.

## 1 Materials

### Standard

Profilo normativo GPC: 0 NTC2018, 1 EN 1992-1-1, 2 UNI EN 1992-1-1.

`3def8fa3-e056-4075-8661-bd51575c00ee`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Type | item | Norma: 0 NTC2018, 1 EN 1992-1-1, 2 UNI EN 1992-1-1. |
| outputs[0] Standard | item | Norma GPC. |

### Database Concrete

Calcestruzzo del database GPC (EN 1992-1-1); una nuova istanza per ogni uscita.

`e2aa1e8b-3fa9-49a5-8d5e-991d4eac5bc6`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Type | item | Classe di resistenza, 0 = C20/25 … 13 = C90/105. |
| outputs[0] Concrete Material | item | Calcestruzzo GPC. |

### Database Rebar Steel

Acciaio per barre ordinarie del database GPC.

`606ebfd0-d9e7-407e-aaa3-eb8579fe2f6b`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Type | item | Qualità dell'acciaio (indice della lista). |
| inputs[1] Behaviour | item | 0 elastico-perfettamente plastico; 1 elastico-incrudente. |
| outputs[0] Rebar Steel Material | item | Acciaio GPC. |

### Database Bar Steel

Acciaio per barre da precompressione del database GPC.

`23875d38-d43a-4051-b19c-a6ae39139f7b`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Type | item | Qualità dell'acciaio (indice della lista). |
| inputs[1] Behaviour | item | 0 elastico-perfettamente plastico; 1 elastico-incrudente. |
| outputs[0] Bar Steel Material | item | Acciaio GPC. |

### Database Tendon Steel

Acciaio per trefoli del database GPC.

`c709c2fa-5e63-4cf6-b3a2-bd505a149a4d`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Type | item | Qualità dell'acciaio (indice della lista). |
| inputs[1] Behaviour | item | 0 elastico-perfettamente plastico; 1 elastico-incrudente. |
| outputs[0] Tendon Steel Material | item | Acciaio GPC. |

### Concrete Material Design Value

Valori caratteristici e di progetto del calcestruzzo per la norma; MPa, moduli positivi.

`b7e2c3a0-0300-4e1b-9d1a-000000000006` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete Material | item | Calcestruzzo GPC. |
| inputs[1] Standard | item | Norma GPC. |
| outputs[0] Name | item | Classe. |
| outputs[1] fck | item | Resistenza caratteristica cilindrica, MPa. |
| outputs[2] fcm | item | Resistenza media, MPa. |
| outputs[3] Elastic modulus | item | Modulo elastico in compressione, MPa. |
| outputs[4] εc2 | item | Deformazione al limite elastico in compressione (adimensionale, modulo). |
| outputs[5] εcu | item | Deformazione ultima in compressione (adimensionale, modulo). |
| outputs[6] fcd | item | Resistenza di progetto a compressione, MPa. |
| outputs[7] fctd | item | Resistenza di progetto a trazione, MPa. |

### Steel Material Design Value

Valori caratteristici e di progetto dell'acciaio per la norma; MPa, deformazioni adimensionali.

`b7e2c3a0-0300-4e1b-9d1a-000000000007` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Steel Material | item | Acciaio GPC. |
| inputs[1] Standard | item | Norma GPC. |
| outputs[0] Name | item | Qualità. |
| outputs[1] Elastic modulus | item | Modulo elastico, MPa. |
| outputs[2] fyk | item | Snervamento caratteristico, MPa. |
| outputs[3] fu | item | Resistenza a rottura, MPa. |
| outputs[4] Et | item | Modulo incrudente, MPa. |
| outputs[5] εy | item | Deformazione di snervamento caratteristica. |
| outputs[6] εu | item | Deformazione ultima caratteristica. |
| outputs[7] fyd | item | Snervamento di progetto, MPa. |
| outputs[8] fud | item | Tensione di progetto alla deformazione ultima, MPa. |
| outputs[9] εyd | item | Deformazione di snervamento di progetto. |
| outputs[10] εud | item | Deformazione ultima di progetto. |

## 2 Sections

### Rebar Element

Barra ordinaria circolare nel piano XY della sezione; diametro in mm.

`b7e2c3a0-0300-4e1b-9d1a-000000000008` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Position | item | Centro della barra in XY, mm (Z ignorata se nulla). |
| inputs[1] Diameter | item | Diametro, mm. |
| inputs[2] Rebar Material | item | Acciaio per barre ordinarie. |
| outputs[0] Rebar Element | item | Barra GPC con preview. |

### Rebar Path Layout

Distribuisce barre lungo la curva dei loro centri in XY, per numero o passo massimo. Preparare il percorso con il copriferro desiderato.

`98dc4c10-e882-41ed-ae13-208c325c0029`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Bar centre path | item | Curva dei centri in XY, mm; aperta o chiusa. |
| inputs[1] Steel | item | Acciaio delle barre. |
| inputs[2] Diameter | item | mm. |
| inputs[3] Count | item | 0 = usa passo massimo; altrimenti numero assegnato. |
| inputs[4] Maximum path spacing | item | Passo lungo la curva, mm. |
| outputs[0] Rebars | list | Barre per Reinforced Concrete Section. |
| outputs[1] Centres | list | Centri in XY. |
| outputs[2] Path spacing | item | Passo effettivo lungo la curva, mm. |

### Tendon Element

Trefolo o barra da precompressione nel piano XY; diametro in mm, tensione iniziale in MPa.

`b7e2c3a0-0300-4e1b-9d1a-000000000009` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Position | item | Centro in XY, mm. |
| inputs[1] Diameter | item | Diametro equivalente, mm. |
| inputs[2] Tendon Material | item | Acciaio da precompressione. |
| inputs[3] Pretension | item | Tensione di precompressione, MPa (trazione positiva). |
| outputs[0] Tendon Element | item | Trefolo GPC con preview. |

### Circular RC Section

Cerchio discretizzato con poligono inscritto. Barre su un anello; il copriferro minimo è rispettato rispetto ai lati del poligono.

`98dc4c10-e882-41ed-ae13-208c325c0025`

| Porta | Accesso | Descrizione |
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

### Rectangular RC Section

Rettangolo centrato nell'origine XY. Barre superiori/inferiori e laterali; geometria in mm.

`98dc4c10-e882-41ed-ae13-208c325c0024`

| Porta | Accesso | Descrizione |
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

### Box RC Section

Rettangolo con foro centrale. Armatura perimetrale esterna ed eventuale armatura intorno al foro.

`98dc4c10-e882-41ed-ae13-208c325c0028`

| Porta | Accesso | Descrizione |
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

### I RC Section

Sezione a I con ali anche diverse. Barre sulle facce esterne delle ali e sui lati dell'anima.

`98dc4c10-e882-41ed-ae13-208c325c0027`

| Porta | Accesso | Descrizione |
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

### T RC Section

Sezione a T con ala superiore, centrata nel rettangolo d'ingombro XY. Layout iniziale da verificare.

`98dc4c10-e882-41ed-ae13-208c325c0026`

| Porta | Accesso | Descrizione |
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

### Reinforced Concrete Section

Sezione da una faccia piana in XY (contorno e fori poligonali) e dalle barre nelle stesse coordinate, mm.

`f247cd41-e5cb-4a35-b9b8-e06b4c43afc6`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Section Name | item | Nome della sezione. |
| inputs[1] Surface | item | Una sola faccia piana parallela a XY, eventualmente forata. |
| inputs[2] Concrete Material | item | Calcestruzzo GPC. |
| inputs[3] Rebar elements | list | Barre e trefoli nelle coordinate XY della superficie. |
| outputs[0] Reinforced Concrete Section | item | Sezione GPC. |

### Section Geometry Check

Controllo geometrico di barre, copriferro e interferro. Non sostituisce Reinforcement Detailing.

`98dc4c10-e882-41ed-ae13-208c325c0030`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Section | item | Sezione GPC. |
| inputs[1] Required bar cover | item | Copriferro geometrico minimo alle barre, mm. |
| inputs[2] Required clear spacing | item | Interferro minimo assegnato, mm. |
| inputs[3] Section ID | item | Identificativo. |
| outputs[0] Result | item | Esito dei soli vincoli geometrici assegnati. |
| outputs[1] Minimum bar cover | item | mm. |
| outputs[2] Minimum clear spacing | item | mm. |
| outputs[3] Steel area | item | mm². |

### Concrete Mechanical Properties

Proprietà geometriche della sola sezione di calcestruzzo, mm.

`b7e2c3a0-0300-4e1b-9d1a-000000000010` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Reinforced Concrete Section | item | Sezione GPC. |
| outputs[0] Name | item | Nome della sezione. |
| outputs[1] Area | item | Area, mm². |
| outputs[2] Sx | item | Momento statico rispetto all'asse X globale, mm³. |
| outputs[3] Sy | item | Momento statico rispetto all'asse Y globale, mm³. |
| outputs[4] Jxx | item | Momento d'inerzia baricentrico, mm⁴. |
| outputs[5] Jyy | item | Momento d'inerzia baricentrico, mm⁴. |
| outputs[6] Jxy | item | Prodotto d'inerzia baricentrico, mm⁴. |
| outputs[7] Jp | item | Momento d'inerzia polare, mm⁴. |
| outputs[8] J11 | item | Momento d'inerzia principale massimo, mm⁴. |
| outputs[9] J22 | item | Momento d'inerzia principale minimo, mm⁴. |
| outputs[10] Principal angle | item | Angolo dell'asse principale 1 rispetto a X, gradi. |
| outputs[11] Centroid | item | Baricentro in XY, mm. |

### Homogenized Mechanical Properties

Proprietà della sezione omogeneizzata a calcestruzzo con il coefficiente ψ delle barre, mm.

`b7e2c3a0-0300-4e1b-9d1a-000000000011` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Reinforced Concrete Section | item | Sezione GPC. |
| inputs[1] ψ factor | item | Coefficiente ψ (viscosità) delle barre; 0 = breve durata. |
| outputs[0] Name | item | Nome della sezione. |
| outputs[1] Area | item | Area, mm². |
| outputs[2] Sx | item | Momento statico rispetto all'asse X globale, mm³. |
| outputs[3] Sy | item | Momento statico rispetto all'asse Y globale, mm³. |
| outputs[4] Jxx | item | Momento d'inerzia baricentrico, mm⁴. |
| outputs[5] Jyy | item | Momento d'inerzia baricentrico, mm⁴. |
| outputs[6] Jxy | item | Prodotto d'inerzia baricentrico, mm⁴. |
| outputs[7] Jp | item | Momento d'inerzia polare, mm⁴. |
| outputs[8] J11 | item | Momento d'inerzia principale massimo, mm⁴. |
| outputs[9] J22 | item | Momento d'inerzia principale minimo, mm⁴. |
| outputs[10] Principal angle | item | Angolo dell'asse principale 1 rispetto a X, gradi. |
| outputs[11] Centroid | item | Baricentro in XY, mm. |

## 3 Input

### Coordinate System

Riferimento delle azioni da origine e due punti sugli assi X e Y, mm.

`8f839216-244a-4891-a1d5-bb0fb42f7909`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Name | item | Nome del riferimento. |
| inputs[1] Origin | item | Origine, mm. |
| inputs[2] P1 | item | Punto sull'asse X. |
| inputs[3] P2 | item | Punto nel piano XY dalla parte delle Y positive. |
| outputs[0] Coordinate System | item | Riferimento GPC. |

### Section Coordinate System

Riferimento con assi X e Y globali nel baricentro della sezione (omogeneizzato per sezioni composte).

`b7e2c3a0-0300-4e1b-9d1a-000000000012` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Section | item | Sezione GPC. |
| outputs[0] Coordinate System | item | Riferimento baricentrico con assi globali. |

### Force component

Combinazione di azioni nel riferimento assegnato; Units 1 = kN/kNm (default), 0 = N/Nmm. Compressione negativa.

`ddd3c603-f931-4db5-ab11-6d1da9ffd01e`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Name | item | Nome univoco della combinazione; vuoto = "Caso k" (k = iterazione). |
| inputs[1] Axial Force | item | Sforzo normale, compressione negativa. |
| inputs[2] Shear X | item | Taglio lungo X. |
| inputs[3] Shear Y | item | Taglio lungo Y. |
| inputs[4] Torsion | item | Momento torcente. |
| inputs[5] Bending X | item | Momento attorno a X. |
| inputs[6] Bending Y | item | Momento attorno a Y. |
| inputs[7] Coordinate System | item | Riferimento delle azioni. |
| inputs[8] Units | item | 0 = N, Nmm; 1 = kN, kNm. |
| outputs[0] Force | item | Azioni GPC in N e Nmm. |

### Load Envelope

Minimi/massimi per ramo, con combinazioni concomitanti. Trasporta tutte le azioni negli assi e origine del primo caso.

`98dc4c10-e882-41ed-ae13-208c325c0001`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Forces | list | Combinazioni nello stesso ramo; N/Nmm. |
| outputs[0] Extremum | list | N min/max, Vx min/max, Vy min/max, T min/max, Mx min/max, My min/max. |
| outputs[1] Value | list | Forze kN; momenti kNm. |
| outputs[2] Case index | list | Indice originale, base zero. |
| outputs[3] Concomitant forces | list | Tutta la combinazione governante; non un vettore sintetico di massimi. |

### Creep and Homogenization

Conversione GPC fra coefficiente viscoso assegnato φ e rapporto modulare n. Non stima φ da età o umidità.

`98dc4c10-e882-41ed-ae13-208c325c0021`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete | item | Calcestruzzo europeo, Ecm. |
| inputs[1] Steel | item | Acciaio, Es. |
| inputs[2] Input is n | item | True rapporto modulare; false coefficiente viscoso. |
| inputs[3] Value | item | φ oppure n. |
| outputs[0] Creep coefficient | item | φ assegnato/ricavato. |
| outputs[1] Modular ratio | item | Rapporto Es/Ec,eff. |
| outputs[2] Effective concrete modulus | item | MPa. |

### Psi coefficient

Coefficiente ψ corrispondente al rapporto di omogeneizzazione n assegnato (GPC).

`7480e84d-4e82-4be1-a021-425dff4803e1`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete Material | item | Calcestruzzo GPC. |
| inputs[1] Steel material | item | Acciaio delle barre. |
| inputs[2] Homogenized factor | item | Rapporto di omogeneizzazione Es/Ec,eff. |
| outputs[0] ψ factor | item | Coefficiente ψ delle barre. |

## 4 Setup

### Concrete Checker Options

Opzioni del checker GPC. Default: dominio plastico, strategia Iterative, analisi lineare, 64 direzioni del dominio.

`b7e2c3a0-0300-4e1b-9d1a-000000000003` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Coordinate System | item | Riferimento delle azioni del checker. |
| inputs[1] Failure Domain Types | item | 0 elastico, 1 plastico. |
| inputs[2] Failure Analysis Types | item | Criterio di ricerca del punto resistente. |
| inputs[3] Domain Point Strategy Types | item | 0 Iterative (default, 2–4 ms per checker); 1 Intersection (dominio discretizzato precalcolato, 0.4–0.6 s per checker). |
| inputs[4] Stress Analysis Types | item | 0 non lineare, 1 lineare. |
| inputs[5] ψ rebar | item | Coefficiente ψ delle barre per l'analisi lineare. |
| inputs[6] ψ tendon | item | Coefficiente ψ dei trefoli. |
| inputs[7] Consider tensile concrete | item | Considera il calcestruzzo teso. |
| inputs[8] Theta discretization | item | Direzioni del dominio 3D (8–360); usata dalla strategia Intersection e dal dominio 3D. |
| outputs[0] Checker Options | item | Opzioni GPC. |

### Concrete Checker

Checker GPC di una sezione con norma e opzioni (copia privata delle opzioni).

`c485957b-3d8e-4f19-8176-f30ffcda952e`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Reinforced Concrete Section | item | Sezione GPC. |
| inputs[1] Standard | item | Norma GPC. |
| inputs[2] Options | item | Da Concrete Checker Options. |
| outputs[0] Concrete Checker | item | Checker GPC. |

### Circular Shear Options

Opzioni di taglio di una pila circolare (modello GPC NTC 2018 per pile circolari); usare Shear Check con Direction = 2 (risultante).

`b7e2c3a0-0300-4e1b-9d1a-000000000002` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Standard | item | Norma NTC 2018. |
| inputs[1] Link steel | item | Acciaio delle staffe. |
| inputs[2] Longitudinal steel | item | Acciaio delle barre longitudinali. |
| inputs[3] Diameter | item | Diametro, mm. |
| inputs[4] Link cover | item | Copriferro alla superficie esterna della staffa, mm. |
| inputs[5] Link diameter | item | Diametro staffe, mm. |
| inputs[6] Legs | item | Bracci efficaci. |
| inputs[7] Spacing | item | Passo delle staffe, mm. |
| inputs[8] Link angle | item | Inclinazione, gradi [45,90]. |
| inputs[9] Bar diameter | item | Diametro barre longitudinali, mm. |
| inputs[10] Cot theta | item | 0 = scelta GPC; altrimenti valore assegnato. |
| inputs[11] Aggregate | item | Diametro massimo aggregato, mm. |
| outputs[0] Options | item | bw = D, d = D − c − Øw − Ø/2, z/d GPC. |

### Rectangular Shear Options

Opzioni di taglio X e Y di una sezione rettangolare b × h (geometria dei componenti di taglio 0.2). Diametro staffe 0 = senza staffe.

`b7e2c3a0-0300-4e1b-9d1a-000000000001` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Standard | item | Norma: braccio z/d del profilo GPC. |
| inputs[1] Link steel | item | Acciaio delle staffe. |
| inputs[2] Longitudinal steel | item | Acciaio delle barre longitudinali. |
| inputs[3] Base | item | Dimensione X, mm. |
| inputs[4] Height | item | Dimensione Y, mm. |
| inputs[5] Bar cover | item | Copriferro alla superficie delle barre longitudinali, mm. |
| inputs[6] Link diameter | item | Diametro staffe, mm; 0 = nessuna staffa. |
| inputs[7] Legs X | item | Bracci efficaci per il taglio X. |
| inputs[8] Legs Y | item | Bracci efficaci per il taglio Y. |
| inputs[9] Spacing | item | Passo delle staffe, mm. |
| inputs[10] Link angle X | item | Inclinazione staffe per X, gradi [45,90]. |
| inputs[11] Link angle Y | item | Inclinazione staffe per Y, gradi [45,90]. |
| inputs[12] Bar diameter X | item | Diametro barre tese per il taglio X, mm. |
| inputs[13] Bar diameter Y | item | Diametro barre tese per il taglio Y, mm. |
| inputs[14] Bar count X | item | Barre tese per il taglio X. |
| inputs[15] Bar count Y | item | Barre tese per il taglio Y. |
| inputs[16] Cot theta | item | 0 = scelta GPC; altrimenti valore assegnato. |
| inputs[17] Aggregate | item | Diametro massimo aggregato, mm. |
| outputs[0] Options X | item | Taglio X: bw = H, d = B − c − ØX/2. |
| outputs[1] Options Y | item | Taglio Y: bw = B, d = H − c − ØY/2. |

### Shear Options

Parametri di una direzione nel riferimento del checker. Acciai indipendenti; d esplicito in mm.

`98dc4c10-e882-41ed-ae13-208c325c0005`

| Porta | Accesso | Descrizione |
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

### Torsion Options

Profilo sottile esplicito e staffe chiuse a 90°. Area longitudinale aggiuntiva a quella per flessione.

`98dc4c10-e882-41ed-ae13-208c325c0007`

| Porta | Accesso | Descrizione |
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

### SLE Options

Contesto SLE esplicito per limiti tensionali e fessurazione.

`98dc4c10-e882-41ed-ae13-208c325c0010`

| Porta | Accesso | Descrizione |
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

### Plate Detailing Options

Dati delle armature secondarie/orizzontali non ricavabili dalla sezione 2D.

`98dc4c10-e882-41ed-ae13-208c325c0015`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Secondary steel | item | Somma delle due facce, mm²/m. |
| inputs[1] Secondary spacing | item | Passo mm; 0 dato mancante, esito da completare. |
| inputs[2] Critical region | item | Zona critica della soletta. |
| outputs[0] Plate data | item | Opzioni piastra/parete. |

### Detailing Options

Ipotesi per le regole di dettaglio GPC. Acciaio unico delle armature; geometria riferita a XY.

`98dc4c10-e882-41ed-ae13-208c325c0016`

| Porta | Accesso | Descrizione |
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

## 5 Analysis

### Stress Analysis

Analisi tensionale GPC (lineare o non lineare secondo il checker) di ogni combinazione della lista.

`2c7939e3-5f53-4d53-989b-a56b65b773e4`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete Checker | item | Checker GPC. |
| inputs[1] Forces | list | Combinazioni. |
| outputs[0] Stress Analysis Results | list | Analisi tensionali, una per combinazione. |
| outputs[1] Strain Plane Results | list | Estremi tensionali e deformativi, uno per combinazione. |

### 3D Failure Domain Check

Dominio resistente 3D GPC del checker. Punti e mesh in (Mx [kNm], My [kNm], N [kN]).

`a4c3e502-6c1e-4954-a9ef-7ac9e8bab5ef`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Concrete Checker | item | Checker GPC. |
| outputs[0] 3D Failure Domain Analysis | item | Dominio GPC con il checker che l'ha prodotto. |
| outputs[1] 3D Failure Domain Points | list | Punti del dominio (Mx kNm, My kNm, N kN). |
| outputs[2] 3D Failure Domain Mesh | item | Mesh del dominio, stesse unità. |

### Domain Slice at N

Sezione Mx–My del dominio a N assegnato, in background; ogni punto è risolto e controllato da GPC.

`98dc4c10-e882-41ed-ae13-208c325c0019`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker e tipo di dominio. |
| inputs[1] Axial force | item | kN, compressione negativa. |
| inputs[2] Directions | item | 8–360 direzioni; controllare la sensibilità alla discretizzazione. |
| inputs[3] Run | item | Avvia il calcolo. |
| outputs[0] Mx-My curve | item | Polilinea chiusa in kNm. |
| outputs[1] Mx | list | kNm. |
| outputs[2] My | list | kNm. |
| outputs[3] Solved N | list | N effettivamente equilibrato, kN. |

### Moment Curvature

Analisi GPC a N costante in background. Checker non lineare; curvatura 1/m e momenti kNm.

`98dc4c10-e882-41ed-ae13-208c325c0018`

| Porta | Accesso | Descrizione |
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

## 6 Checks

### 3D Failure Domain Analysis

Verifica N-Mx-My delle combinazioni con il checker del dominio; punti di domanda e resistenza per la visualizzazione. Report con Check Report.

`b7e2c3a0-0300-4e1b-9d1a-000000000013` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] 3D Failure Domain Analysis | item | Da 3D Failure Domain Check. |
| inputs[1] Forces | list | Combinazioni. |
| inputs[2] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Results | list | Esito N-Mx-My per combinazione (Check Report / Check Envelope). |
| outputs[1] External Force | list | Domanda (Mx kNm, My kNm, N kN) nel riferimento della combinazione. |
| outputs[2] Resistance Force | list | Punto resistente (Mx kNm, My kNm, N kN); non impostato se assente. |
| outputs[3] Utilization | list | Rapporto di utilizzo GPC; NaN se non disponibile. |

### Section Resistance Check

Verifica N-Mx-My GPC con esito strutturato e convergenza esplicita.

`98dc4c10-e882-41ed-ae13-208c325c0009`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker SLU/SLV. |
| inputs[1] Force | item | Azioni N/Nmm. |
| inputs[2] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Result | item | Esito N-Mx-My. |

### Shear Check

Verifica a taglio GPC di una direzione con risultato strutturato: 0 = X (V1/M2), 1 = Y (V2/M1), 2 = risultante (sezioni circolari).

`98dc4c10-e882-41ed-ae13-208c325c0006`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker della sezione. |
| inputs[1] Force | item | Combinazione, N/Nmm. |
| inputs[2] Options | item | Da Shear Options, Rectangular Shear Options o Circular Shear Options. |
| inputs[3] Direction | item | 0 = X (V1/M2), 1 = Y (V2/M1), 2 = risultante √(V1²+V2²). |
| inputs[4] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Result | item | Risultato strutturato ReBear. |

### Torsion and Shear

Torsione e interazioni GPC con entrambi i tagli concomitanti.

`98dc4c10-e882-41ed-ae13-208c325c0008`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Checker della sezione. |
| inputs[1] Force | item | Combinazione completa, N/Nmm. |
| inputs[2] Torsion options | item | Da Torsion Options. |
| inputs[3] Shear X options | item, opzionale | Richieste se Vx non nullo. |
| inputs[4] Shear Y options | item, opzionale | Richieste se Vy non nullo. |
| inputs[5] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Result | item | Utilizzo massimo fra torsione e interazioni; dettagli in Check Result. |

### Serviceability Check

Limiti tensionali e fessurazione su analisi GPC, con risultati per gli inviluppi.

`98dc4c10-e882-41ed-ae13-208c325c0011`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Stress result | item | Risultato di Stress Analysis. |
| inputs[1] Options | item | Da SLE Options. |
| inputs[2] Section ID | item | Identificativo per gli inviluppi. |
| outputs[0] Results | list | Tensioni e fessurazione. |
| outputs[1] Crack data | item | Risultato GPC per Effective Cracked Section. |

### Batch Section Checks

Tutte le combinazioni del ramo, in background con annullamento, e inviluppi. Le opzioni scollegate escludono la relativa verifica.

`98dc4c10-e882-41ed-ae13-208c325c0012`

| Porta | Accesso | Descrizione |
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
| outputs[1] CSV | item | Tabella dei risultati (stesso formato di Check Report). |
| outputs[2] Envelope | list | Sintesi per verifica. |

### Reinforcement Candidate Search

Sceglie la minima As fra i checker candidati che soddisfano tutte le combinazioni N-Mx-My; candidati in parallelo, in background. Non verifica taglio, SLE o dettagli.

`98dc4c10-e882-41ed-ae13-208c325c0022`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Candidate checkers | list | 1–100 candidati espliciti; vincoli geometrici definiti a monte. |
| inputs[1] Forces | list | Combinazioni N-Mx-My, massimo 1000. |
| inputs[2] Target utilization | item | 0 < η ≤ 1. |
| inputs[3] Run | item | Avvia la ricerca. |
| outputs[0] Selected index | item | -1 se nessun candidato idoneo. |
| outputs[1] Selected checker | item | Checker selezionato. |
| outputs[2] All results | list | Esiti di ogni candidato e combinazione. |
| outputs[3] Selected steel area | item | mm², NaN se nessun candidato. |

## 7 Detailing

### Concrete Cover

Copriferro nominale GPC: requisito della specifica (diametro di riferimento) e delle barre reali; esito non valutato senza barre reali.

`b7e2c3a0-0300-4e1b-9d1a-000000000004` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Standard | item | Norma GPC. |
| inputs[1] Concrete | item | Calcestruzzo. |
| inputs[2] Exposures | list | Classi ambientali concorrenti, es. XC3, XD1. |
| inputs[3] Specification bar diameter | item | Diametro della specifica dei materiali (requisito di aderenza di riferimento), mm. |
| inputs[4] Actual bar diameter | item, opzionale | Diametro effettivo delle barre, mm; scollegato = barre reali non note (esito da completare). |
| inputs[5] Adopted cover | item | Copriferro nominale disponibile, mm. |
| inputs[6] Design life | item | Anni, secondo il profilo GPC. |
| inputs[7] Deviation | item | Tolleranza esecutiva Δcdev, mm. |
| inputs[8] Aggregate | item | Aggregato massimo, mm. |
| inputs[9] Slab geometry (EN) | item | EN: geometria a soletta, riduzione di una classe strutturale. |
| inputs[10] Special quality control (EN) | item | EN: controllo di qualità speciale, riduzione di una classe strutturale. |
| inputs[11] Strength class reduction (EN) | item | EN: riduzione di classe strutturale per la resistenza del calcestruzzo. |
| inputs[12] Plate element (NTC) | item | NTC: elemento a piastra (valori tabellari ridotti). |
| inputs[13] Quality control reduction (NTC) | item | NTC: riduzione per produzione con controllo di qualità. |
| inputs[14] Rough surface | item | Superficie irregolare. |
| inputs[15] Abrasion | item | 0 nessuna; 1,2,3 classi GPC. |
| inputs[16] Ground | item | 0 cassero; 1 terreno preparato; 2 terreno diretto. |
| inputs[17] Pertinent minimum strength | item, opzionale | Resistenza minima pertinente per NTC, MPa. |
| inputs[18] Section ID | item | Identificativo. |
| outputs[0] Result | item | Confronto richiesto/disponibile. |
| outputs[1] Required nominal | item | Copriferro nominale richiesto (massimo fra specifica e barre reali), mm. |
| outputs[2] Durability minimum | item | Per Detailing Options, mm. |
| outputs[3] Cover data | item | Risultato GPC completo (barre reali se note). |

### Anchorage and Lap

Ancoraggio rettilineo o sovrapposizione di barre tese ad aderenza migliorata; coefficienti α1…α5 = 1.

`98dc4c10-e882-41ed-ae13-208c325c0014`

| Porta | Accesso | Descrizione |
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

### Reinforcement Detailing

Minimi/massimi, interferro, staffe e copriferro secondo il profilo GPC; rende visibili le regole in sospeso.

`98dc4c10-e882-41ed-ae13-208c325c0017`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Checker | item | Sezione e norma. |
| inputs[1] Options | item | Da Detailing Options. |
| inputs[2] Minimum axial force | item | Massima compressione fra le combinazioni, kN negativa. |
| inputs[3] Section ID | item | Identificativo. |
| outputs[0] Results | list | Un risultato per regola. |
| outputs[1] CSV | item | Dettagli e confronti GPC (stesso formato di Check Report). |

## 8 Results

### Check Result

Legge un risultato strutturato senza sostituire i dati mancanti con zero.

`98dc4c10-e882-41ed-ae13-208c325c0003`

| Porta | Accesso | Descrizione |
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

### Effective Cracked Section

Regioni efficaci calcolate da GPC per la fessurazione; non ricostruisce formule o zone per approssimazione.

`98dc4c10-e882-41ed-ae13-208c325c0020`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Crack data | item | Uscita Crack data di Serviceability Check. |
| outputs[0] Regions | list | Chiavi delle regioni. |
| outputs[1] Effective concrete area | list | mm². |
| outputs[2] Effective steel area | list | mm². |
| outputs[3] Crack width | list | mm; NaN se non valutata. |
| outputs[4] Regions preview | list | Brep delle regioni, con fori quando presenti. |
| outputs[5] Bar indices | list | Indici base zero delle barre ordinarie contribuenti. |

### Stress Analysis Summary

Estremi tensionali e deformativi di un'analisi; NaN se la grandezza non esiste. Collegare Summary a Check Report per CSV/JSON.

`b7e2c3a0-0300-4e1b-9d1a-000000000005` (nuovo in 0.3)

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Strain Plane Result | item | Uscita Strain Plane Results di Stress Analysis. |
| outputs[0] Summary | item | Riepilogo per Check Report. |
| outputs[1] Id | item | Posizione della combinazione in Stress Analysis. |
| outputs[2] Case | item | Nome della combinazione. |
| outputs[3] N [kN] | item | Azione della combinazione. |
| outputs[4] Mx [kNm] | item | Azione della combinazione. |
| outputs[5] My [kNm] | item | Azione della combinazione. |
| outputs[6] σc min [MPa] | item | Tensione, MPa (compressione negativa); NaN se assente. |
| outputs[7] σc max [MPa] | item | Tensione, MPa (compressione negativa); NaN se assente. |
| outputs[8] σs min [MPa] | item | Tensione, MPa (compressione negativa); NaN se assente. |
| outputs[9] σs max [MPa] | item | Tensione, MPa (compressione negativa); NaN se assente. |
| outputs[10] σp min [MPa] | item | Tensione, MPa (compressione negativa); NaN se assente. |
| outputs[11] σp max [MPa] | item | Tensione, MPa (compressione negativa); NaN se assente. |
| outputs[12] σss min [MPa] | item | Tensione, MPa (compressione negativa); NaN se assente. |
| outputs[13] σss max [MPa] | item | Tensione, MPa (compressione negativa); NaN se assente. |
| outputs[14] εc min [‰] | item | Deformazione, ‰ (accorciamento negativo); NaN se assente. |
| outputs[15] εc max [‰] | item | Deformazione, ‰ (accorciamento negativo); NaN se assente. |
| outputs[16] εs min [‰] | item | Deformazione, ‰ (accorciamento negativo); NaN se assente. |
| outputs[17] εs max [‰] | item | Deformazione, ‰ (accorciamento negativo); NaN se assente. |
| outputs[18] εp min [‰] | item | Deformazione, ‰ (accorciamento negativo); NaN se assente. |
| outputs[19] εp max [‰] | item | Deformazione, ‰ (accorciamento negativo); NaN se assente. |
| outputs[20] εss min [‰] | item | Deformazione, ‰ (accorciamento negativo); NaN se assente. |
| outputs[21] εss max [‰] | item | Deformazione, ‰ (accorciamento negativo); NaN se assente. |
| outputs[22] d [mm] | item | Altezza utile nella direzione dell'asse neutro, mm. |
| outputs[23] x [mm] | item | Profondità dell'asse neutro, mm; NaN se non definita. |
| outputs[24] θ [°] | item | Inclinazione dell'asse neutro, gradi. |

### Section Resistance Summary

Momenti resistenti plastici ed elastici a N costante attorno agli assi globali X/Y per il baricentro; kNm con segno, strategia Iterative.

`0a07f98a-2c47-4bc6-84c3-5833a4f6b65a`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Reinforced Concrete Section | item | Sezione GPC. |
| inputs[1] Standard | item | Norma GPC. |
| inputs[2] Axial Force [kN] | item | Sforzo normale costante, kN, compressione negativa. |
| outputs[0] MxRd+ pl [kNm] | item | Plastico, Mx positivo globale. |
| outputs[1] MyRd+ pl [kNm] | item | Plastico, My positivo globale. |
| outputs[2] MxRd- pl [kNm] | item | Plastico, Mx negativo globale. |
| outputs[3] MyRd- pl [kNm] | item | Plastico, My negativo globale. |
| outputs[4] MxRd+ el [kNm] | item | Elastico, Mx positivo globale. |
| outputs[5] MyRd+ el [kNm] | item | Elastico, My positivo globale. |
| outputs[6] MxRd- el [kNm] | item | Elastico, Mx negativo globale. |
| outputs[7] MyRd- el [kNm] | item | Elastico, My negativo globale. |

### Check Envelope

Inviluppo per sezione e verifica nel ramo. I risultati non valutati impediscono un esito positivo completo.

`98dc4c10-e882-41ed-ae13-208c325c0002`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Results | list | Risultati strutturati ReBear. |
| outputs[0] Group | list | Sezione / verifica. |
| outputs[1] Status | list | Stato complessivo. |
| outputs[2] Governing | list | Caso governante, anche quando non valutabile; null se tutti non richiesti. |
| outputs[3] Pending | list | Numero di casi non valutati. |
| outputs[4] Count | list | Numero totale dei casi. |

### Check Report

Unico report di ReBear: tabella CSV e JSON versionato di esiti (Check Result) oppure di riepiloghi tensionali (Stress Analysis Summary); nessuna scrittura su disco.

`98dc4c10-e882-41ed-ae13-208c325c0004`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Results | list | Esiti oppure riepiloghi tensionali dello stesso ramo (un solo tipo per report). |
| outputs[0] CSV | item | Separatore ;, numeri invariant, campi con escaping, valore mancante = campo vuoto. |
| outputs[1] JSON | item | Schema ReBear.CheckReport o ReBear.StressReport v1 e versione motore. |
| outputs[2] Summary | list | Sintesi degli inviluppi o degli estremi. |

## 9 Display

### View Strain Analysis Results

Tensioni ai vertici del calcestruzzo e nelle barre (MPa), asse neutro e mesh della sezione suddivisa sull'asse.

`65801921-575b-466e-8001-a7e833dbacb6`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Stress Analysis Results | item | Analisi tensionale GPC. |
| outputs[0] Concrete Points | list | Vertici del contorno, mm. |
| outputs[1] Concrete Values | list | Tensioni del calcestruzzo, MPa. |
| outputs[2] Rebar Points | list | Centri delle barre, mm. |
| outputs[3] Rebar Values | list | Tensioni delle barre, MPa. |
| outputs[4] Neutral Axis | item | Asse neutro esteso alla sezione; assente senza curvatura. |
| outputs[5] Combo Name | item | Nome della combinazione. |
| outputs[6] Mesh | item | Mesh della sezione suddivisa sull'asse neutro. |

### Color Mesh

Colora una mesh con il colore del punto più vicino (al vertice se Smooth, al baricentro della faccia altrimenti).

`1c72c75b-9ede-461a-8553-973173695f79`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Points | list | Punti campione. |
| inputs[1] Colours | list | Un colore per punto, oppure uno solo. |
| inputs[2] Mesh | item | Mesh da colorare. |
| inputs[3] Smooth | item | True colore per vertice; false per faccia. |
| outputs[0] Mesh | item | Mesh colorata. |

### Result Color Mesh

Mesh colorata delle tensioni del calcestruzzo: gradiente fra fctd (blu) e fcd (magenta), nero oltre; senza Smooth colori per segno.

`b62a91c6-b7bb-4e99-800f-1fafb45707ad`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Stress Analysis Results | item | Analisi tensionale GPC. |
| inputs[1] Smooth | item | True gradiente per vertice; false colore per faccia secondo il segno. |
| outputs[0] Mesh | item | Mesh colorata, mm. |

### Inspect Strain Analysis Results

Tensione del calcestruzzo nei punti assegnati, MPa.

`8ae4225f-e39f-4304-bbdb-2610c34dd354`

| Porta | Accesso | Descrizione |
|---|---|---|
| inputs[0] Stress Analysis Results | item | Analisi tensionale GPC. |
| inputs[1] Concrete Points | list | Punti in XY, mm. |
| outputs[0] Concrete Stress | list | Tensioni, MPa (compressione negativa). |
