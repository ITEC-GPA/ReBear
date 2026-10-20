# ReBear — grafica

**ReBear** richiama *rebar* (armatura) e *bear* (orso). Sottotitolo: **Reinforced Concrete Sections**.

- `symbol.svg` / `symbol.png`: simbolo geometrico dell'orso, con staffa e quattro barre. Versione semplificata per la palette del plugin, riproducibile e modificabile.
- `../../src/ConcreteSectionChecker.Grasshopper/Resources/Icons/Plugin.png`: simbolo incorporato a 24 px.
- `../icons/`: 61 icone tecniche originali, una per componente, in SVG e PNG a 96 px.
- `../icon-gallery.png`: tavola di controllo visivo.
- `logo-mark.png`: proposta raster dell'orso creata con ImageGen, con fondo trasparente.
- `prompt.txt`: prompt della proposta raster.

Palette: blu scuro `#283E4A`, verde petrolio `#008B8B`, grigio calcestruzzo `#DDE5E8`, ambra `#D99235`; blu e rosso distinguono assi e valori nelle icone tecniche.

Le icone sono disegnate a partire da primitive vettoriali, non ritagliate dalla proposta raster. Per rigenerarle, dalla radice del repository:

```powershell
dotnet run --project tools/IconGenerator -c Release
```

Il generatore usa GDI+ su Windows e produce anche le risorse effettive a 24 px. Non modifica `logo-mark.png`. Il simbolo della palette privilegia la leggibilità alle dimensioni di un componente Grasshopper.
