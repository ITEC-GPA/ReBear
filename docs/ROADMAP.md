# Sviluppi successivi di ReBear

La versione 0.2 ha realizzato le estensioni per cui erano disponibili contratti GPC utilizzabili: inviluppi, report, taglio, torsione, SLE, copriferro, ancoraggi, dettagli di travi/pilastri/solette/pareti, momento–curvatura, sezioni del dominio, regioni efficaci, omogeneizzazione, ricerca discreta e cinque builder. La versione 0.3 ha unificato tutti i componenti sul modello `ReBearComponent`, corretto i difetti B1–B5, introdotto il calcolo in background con annullamento, la persistenza versionata dei dati internalizzati, il controllo del set GPC caricato e la strategia Iterative ovunque. Catalogo: [COMPONENTS.md](COMPONENTS.md); novità: [NEW_COMPONENTS.md](NEW_COMPONENTS.md).

## Prossimi passi

| Sviluppo | Stato e lavoro necessario |
|---|---|
| Accettazione in Rhino 8 | Eseguire `tools/RhinoExampleProbe.py` sui 20 esempi, aprire definizioni 0.2 reali sostituendo i componenti con nuovo GUID, provare annullamento dei calcoli in background, salvataggio/riapertura con dati internalizzati e convivenza con BridGHe. |
| Set GPC con cache del dominio | Un set candidato con cache del dominio è in preparazione: adottarlo insieme a BridGHe (processo Rhino condiviso), aggiornando `lib/GPC` e il manifest e rieseguendo verifiche ed esempi. |
| Batch in parallelo | Il batch è seriale su un checker privato; con più copie del checker (2–4 ms ciascuna con Iterative) si possono suddividere i casi fra i core. |

## Funzioni differite

| Sviluppo | Stato e lavoro necessario |
|---|---|
| Export to ANTHEA `.anthea` | **Sospeso su richiesta dell'utente.** Non presente nella Release. Prima di riprenderlo occorre verificare formato, geometrie supportate e riapertura nel software. |
| Prestress Stages | Perdite, fasi e predeformazioni richiedono un flusso dedicato e supporto coerente nei checker GPC. Taglio/torsione rifiutano la precompressione. |
| Punching | Definire perimetri di controllo, carichi, eccentricità e armature; individuare o sviluppare e validare il motore GPC. |
| Fatigue | Definire spettri/intervalli di tensione e modelli GPC pertinenti. |
| Fire | Modello termico, proprietà a temperatura e metodo di verifica GPC da definire. |
| Member Stability | Occorrono elemento, vincoli, imperfezioni e secondo ordine. I controlli di sezione presenti non risolvono questo problema. |
| Viscosità e ritiro da condizioni ambientali | Creep and Homogenization converte φ/n assegnati: non stima evoluzione nel tempo, ritiro o umidità. |
| Ricerca automatica dell'armatura | La ricerca seleziona fra checker candidati espliciti e controlla solo N-Mx-My. Da aggiungere generazione dei layout e vincoli simultanei di dettagli/SLE/taglio. |
| Layout su più strati | Il layout attuale distribuisce barre lungo una curva dei centri in XY. Offset del contorno, gruppi di diametri e più strati restano estensioni. |

## Infrastruttura

- Pacchetto Yak e coordinamento delle dipendenze GPC fra plugin (oggi segnalato dal controllo al caricamento).
- Persistenza dei risultati di calcolo, se servisse riaprire file senza ricalcolo.
- Confronti numerici indipendenti più estesi e studio della sensibilità della discretizzazione.

Componenti le cui porte cambiano ricevono un nuovo `ComponentGuid` (`tests/Compatibility/palette-0.3.json` è la fotografia di riferimento). Le formule normative nuove appartengono a GPC; nel plugin restano adattamento, geometria, presentazione e gestione dei risultati.
