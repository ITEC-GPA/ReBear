# Sviluppi successivi di ReBear

La versione 0.2 realizza le estensioni della prima roadmap per cui sono disponibili contratti GPC utilizzabili: inviluppi, report, taglio v2, torsione, SLE, copriferro, ancoraggi, dettagli di travi/pilastri/solette/pareti, momento–curvatura, sezioni del dominio, regioni efficaci, omogeneizzazione, ricerca discreta e cinque builder. Catalogo: [NEW_COMPONENTS.md](NEW_COMPONENTS.md).

## Funzioni differite

| Sviluppo | Stato e lavoro necessario |
|---|---|
| Export to ANTHEA `.anthea` | **Sospeso su richiesta dell'utente.** Non presente nella Release. Prima di riprenderlo occorre verificare formato, geometrie supportate e riapertura nel software. |
| Prestress Stages | Perdite, fasi e predeformazioni richiedono un flusso dedicato e supporto coerente nei checker GPC. Taglio/torsione v2 rifiutano la precompressione. |
| Punching | Definire perimetri di controllo, carichi, eccentricità e armature; individuare o sviluppare e validare il motore GPC. |
| Fatigue | Definire spettri/intervalli di tensione e modelli GPC pertinenti. |
| Fire | Modello termico, proprietà a temperatura e metodo di verifica GPC da definire. |
| Member Stability | Occorrono elemento, vincoli, imperfezioni e secondo ordine. I controlli di sezione presenti non risolvono questo problema. |
| Viscosità e ritiro da condizioni ambientali | Creep and Homogenization converte φ/n assegnati: non stima evoluzione nel tempo, ritiro o umidità. |
| Ricerca automatica dell'armatura | La ricerca attuale seleziona fra checker candidati espliciti e controlla solo N-Mx-My. Da aggiungere generazione dei layout e vincoli simultanei di dettagli/SLE/taglio. |
| Layout su più strati | Il layout attuale distribuisce barre lungo una curva dei centri in XY. Offset del contorno, gruppi di diametri e più strati restano estensioni. |

## Infrastruttura

- Accettazione in Rhino 8 su file storici e nuovi esempi, con salvataggio/riapertura.
- Pacchetto Yak e coordinamento delle dipendenze GPC fra plugin.
- Serializzazione versionata degli oggetti di opzione per internalizzazione: oggi sono dati da ricostruire con i componenti a monte. Check Result dispone di serializzazione versionata.
- Esecuzione asincrona con cancellazione e cache: attualmente batch e ricerca sono seriali e non condividono i solver fra thread.
- Confronti numerici indipendenti più estesi e studio della sensibilità della discretizzazione.

Conservare GUID e porte dei componenti rilasciati. Le formule normative nuove appartengono a GPC; nel plugin restano adattamento, geometria, presentazione e gestione dei risultati.
