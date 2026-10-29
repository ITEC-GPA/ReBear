# ReBear 0.3 — novità

Dalla 0.3 tutti i componenti sono descritti nel [catalogo unico](COMPONENTS.md), generato dal plugin compilato, e nella [palette](PALETTE.md). Questa pagina elenca soltanto ciò che è nuovo o cambiato rispetto a 0.2.2; la tabella completa vecchio → nuovo componente è in [MIGRATION.md](MIGRATION.md#componenti-022--03).

## Componenti nuovi

| Componente | Pannello | Funzione |
|---|---|---|
| Rectangular Shear Options | 4 Setup | Opzioni di taglio X e Y di un rettangolo B × H dalla geometria dei vecchi componenti di taglio: copriferro alla superficie delle barre, barre tese, staffe (Ø 0 = senza staffe), braccio z/d dal profilo GPC della norma. |
| Circular Shear Options | 4 Setup | Opzioni di taglio di una pila circolare con il modello GPC NTC 2018 (z/d GPC, d = D − c − Øw − Ø/2); da usare con Shear Check in direzione 2. |
| Stress Analysis Summary | 8 Results | Sostituisce Print Stress Analysis Results: estremi tensionali e deformativi come numeri (NaN se assenti) e un riepilogo per Check Report. |

## Componenti con nuovo GUID

Concrete Checker Options (porta θ discretization, enumerazioni validate), Concrete Cover (diametro di specifica ed effettivo, modificatori EN/NTC separati), 3D Failure Domain Analysis (esiti strutturati invece di "To Excel"), Section Coordinate System (assi globali), Concrete/Steel Material Design Value (uscite numeriche), Rebar Element (diametro in mm), Tendon Element (porte corrette), Concrete/Homogenized Mechanical Properties (angolo in gradi).

## Componenti rimossi

Rectangular Section Shear Check, Rectangular Section Shear Check No stirrup e Circular Section Shear Check (sostituiti da Shear Check con le opzioni rettangolari/circolari); Serviceability Stress Check e Serviceability Crack Width Check (sostituiti da Serviceability Check). Le uscite "To Excel" sono sostituite da Check Report.

## Comportamenti nuovi dei componenti esistenti

- **Shear Check** (era Shear Check v2): direzione 2 = risultante √(V1² + V2²) per le sezioni circolari.
- **Serviceability Check** (era Serviceability Check v2): il messaggio della fessurazione riporta criterio e stato GPC.
- **Batch Section Checks, Reinforcement Candidate Search, Domain Slice at N, Moment Curvature**: calcolo in background con annullamento, su una copia privata del checker; candidati della ricerca in parallelo.
- **Check Report**: unico report CSV/JSON di esiti e riepiloghi tensionali.
- **Section Resistance Summary** (era Secton Resistance Summary): momenti resistenti negli assi globali, strategia Iterative, nessun arrotondamento.
- **View Strain Analysis Results, Result Color Mesh**: piano senza curvatura gestito con un avviso; asse neutro esteso a tutta la sezione.
- **Force component** e verifiche singole: nomi "Caso k" univoci per le combinazioni senza nome.
- **Tutti**: porte con unità, messaggi che indicano la porta, errori inattesi con contesto, dati internalizzati salvati nel file.
