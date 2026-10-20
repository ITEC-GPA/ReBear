using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;

internal static partial class Program
{
    internal static readonly string Root = FindRoot();
    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "ConcreteSectionChecker.sln"))) directory=directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture=CultureInfo.InvariantCulture;
            Geometries(); EnvelopesExample(); ShearExamples(); ResistanceExample(); SleExamples();
            CoverExample(); AnchorageExample(); DetailingExamples(); AnalysisExamples();
            Require(Index.Count==20,"Exactly 20 examples");
            var index=new StringBuilder("# ReBear — 20 esempi svolti\n\nOgni cartella contiene una definizione nativa `.gh`, la corrispondente `.ghx` testuale, la spiegazione passo per passo, i risultati numerici `results.json` e il grafo dei collegamenti `graph.json`.\n\n");
            index.AppendLine("## Apertura\n\n1. Caricare tutta la cartella `bin` di ReBear 0.2.0 in Grasshopper/Rhino 8, modalità .NET 8.\n2. Aprire `definition.gh` dell'esempio desiderato; non occorrono altri plugin né file esterni.\n3. I dati sono salvati nelle porte: tasto destro → Set… per modificarli. I Run sono attivi. Leggere il README della cartella per ipotesi, unità e verifiche.\n4. Confrontare risultati e messaggi con `results.json`. I casi volutamente non soddisfatti o non valutabili sono spiegati.\n\nCalcoli adapter/GPC e controlli numerici eseguiti automaticamente. Archivi GH riletti con controllo di identità e connessioni. **Ricalcolo, salvataggio e preview dentro Rhino restano da provare nell'host.**\n");
            index.AppendLine("| N. | Esempio | File GH |\n|---|---|---|");
            foreach(var item in Index.OrderBy(x=>x.Folder))
                index.AppendLine($"| {item.Folder[..2]} | [{item.Title}]({item.Folder}/README.md) | [Apri definizione]({item.Folder}/definition.gh) |");
            index.AppendLine("\n## Riproduzione\n\nDa PowerShell nella radice del repository:\n\n```powershell\npowershell -File build/Examples.ps1\n```\n\nLo script compila in Release, ricalcola i 20 esempi e rigenera le cartelle. Non avvia Rhino. I file GH contengono identificatori di istanza nuovi a ogni generazione; i GUID dei tipi di componente rimangono stabili. Nessun export ANTHEA è incluso.\n\nIl generatore usa i serializer ufficiali dei componenti Grasshopper e un contenitore di definizione conforme alla struttura osservata nell'archivio storico. Evita l'inizializzazione di GH_Document, che richiede Rhino nativo. I risultati vengono calcolati separatamente dagli adapter, con gli stessi input dei grafi; questo non costituisce un test del ricalcolo nell'host.\n");
            File.WriteAllText(Path.Combine(Root,"examples/README.md"),index.ToString().TrimEnd()+Environment.NewLine);
            Console.WriteLine("TOTAL: 20 examples generated and numerically checked.");
            return 0;
        }
        catch(Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
