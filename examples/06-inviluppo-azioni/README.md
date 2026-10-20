# 06 — Inviluppo con combinazioni concomitanti

Tre combinazioni nominali A, B e C nello stesso riferimento globale. Nessun coefficiente di combinazione è generato dal componente: le azioni sono già combinate. L'obiettivo è conservare l'associazione fra l'estremo e le altre cinque azioni dello stesso caso.

## Esecuzione

Aprire `definition.gh` (oppure l'equivalente testuale `definition.ghx`) con ReBear 0.2.0 in Rhino 8/.NET 8. Il file è autonomo: nessun Excel o plugin esterno aggiuntivo. Le porte contengono i dati persistenti; i componenti con Run sono già attivi. Geometria in **mm**, tensioni in **MPa**, azioni delle porte in **kN/kNm**, compressione **negativa**.

## Svolgimento e controllo

Si confrontano min/max separatamente per N, Vx, Vy, T, Mx e My. Il minimo N è −500 kN nel caso B; il massimo Mx è 120 kNm nel caso C. Il vettore restituito per Mx max conserva N=−100 kN e Vy=120 kN di C, anche se il minimo N appartiene a B. L'indice è base zero e gli estremi ripetuti mantengono la prima occorrenza.

| Quantità calcolata | Valore |
|---|---:|
| N minimo [kN] | -500 |
| Mx massimo [kNm] | 120 |

Calcolo numerico eseguito dal generatore con le stesse chiamate adapter usate dal plugin. Eseguiti i controlli automatici descritti e la rilettura dell'archivio GH con verifica dei GUID e dei collegamenti. **Ricalcolo e preview nell'host Rhino da verificare dall'utente.** Questo esempio copre soltanto le verifiche indicate, non la progettazione completa dell'elemento.

## Componenti e collegamenti

I valori modificabili sono salvati nelle porte (tasto destro sulla porta → Set…). I valori non elencati mantengono il default del componente.

**00 — Database Concrete**

- Type = `3`

**01 — Database Rebar Steel**

- Type = `1`
- Behaviour = `0`

**02 — Rectangular RC Section**

- Width = `300`
- Height = `500`
- Link cover = `30`
- Link diameter = `8`
- Bar diameter = `20`
- Top bars = `4`
- Bottom bars = `4`
- Side bars per face = `0`
- Concrete ← 00 Database Concrete / Concrete Material
- Steel ← 01 Database Rebar Steel / Rebar Steel Material

**03 — Force component**

- Name = `A`
- Axial Force = `-300`
- Shear X = `60`
- Shear Y = `90`
- Torsion = `2`
- Bending X = `45`
- Bending Y = `15`
- Units = `1`
- Coordinate System ← 04 Coordinate System / Coordinate System

**04 — Coordinate System**

- Name = `Global XY`
- Origin = `0,0,0`
- P1 = `1,0,0`
- P2 = `0,1,0`

**05 — Force component**

- Name = `B`
- Axial Force = `-500`
- Shear X = `-80`
- Shear Y = `40`
- Torsion = `-4`
- Bending X = `100`
- Bending Y = `-25`
- Units = `1`
- Coordinate System ← 04 Coordinate System / Coordinate System

**06 — Force component**

- Name = `C`
- Axial Force = `-100`
- Shear X = `20`
- Shear Y = `120`
- Torsion = `7`
- Bending X = `120`
- Bending Y = `35`
- Units = `1`
- Coordinate System ← 04 Coordinate System / Coordinate System

**07 — Load Envelope**

- Forces ← 03 Force component / Force
- Forces ← 05 Force component / Force
- Forces ← 06 Force component / Force
