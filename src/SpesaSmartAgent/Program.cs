using System.Text.Json;
using SpesaSmartAgent.Models;
using SpesaSmartAgent.Services;

Console.WriteLine("==================================================");
Console.WriteLine("      SPESA SMART AGENT - BOLOGNA & CASALECCHIO   ");
Console.WriteLine("==================================================");

// Risolve la root di progetto controllando la directory corrente e risalendo se necessario
var current = new DirectoryInfo(Directory.GetCurrentDirectory());
string? projectRoot = null;
while (current != null)
{
    if (File.Exists(Path.Combine(current.FullName, "config_basket.json")))
    {
        projectRoot = current.FullName;
        break;
    }
    current = current.Parent;
}

projectRoot ??= Directory.GetCurrentDirectory();

var basketFile = Path.Combine(projectRoot, "config_basket.json");
var storesFile = Path.Combine(projectRoot, "config_stores.json");

if (!File.Exists(basketFile) || !File.Exists(storesFile))
{
    Console.WriteLine("Errore: file di configurazione non trovati.");
    return;
}

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var basketJson = File.ReadAllText(basketFile);
var basket = JsonSerializer.Deserialize<BasketConfig>(basketJson, jsonOptions) ?? new BasketConfig();

var storesJson = File.ReadAllText(storesFile);
var storesDoc = JsonDocument.Parse(storesJson);
var stores = storesDoc.RootElement.GetProperty("stores").Deserialize<List<TargetStore>>(jsonOptions) ?? new List<TargetStore>();

Console.WriteLine($"Caricati {stores.Count} negozi monitorati e {basket.Fissi.Count + basket.Occasionali.Count} prodotti paniere.");
Console.WriteLine();

var downloadsDir = Path.Combine(projectRoot, "downloads");
Directory.CreateDirectory(downloadsDir);

// Se la cartella è vuota, genera un PDF campione per testare il motore
var samplePdf = Path.Combine(downloadsDir, "esselunga_casalecchio_sample.pdf");
if (!File.Exists(samplePdf))
{
    Console.WriteLine("Generazione volantino campione per il test...");
    SpesaSmartAgent.Tools.TestPdfGenerator.CreateSampleFlyer(samplePdf);
}

var scanner = new PdfFlyerScanner();
var allMatches = new List<MatchedOffer>();

var pdfFiles = Directory.GetFiles(downloadsDir, "*.pdf");
if (pdfFiles.Length == 0)
{
    Console.WriteLine($"Nessun PDF presente nella cartella: {downloadsDir}");
    Console.WriteLine("Istruzioni: I volantini PDF scaricati vanno inseriti in 'downloads/'.");
}
else
{
    Console.WriteLine($"Trovati {pdfFiles.Length} volantini PDF da analizzare...\n");
    foreach (var pdf in pdfFiles)
    {
        var fileName = Path.GetFileName(pdf);
        var matchedStore = stores.FirstOrDefault(s => fileName.Contains(s.Id, StringComparison.OrdinalIgnoreCase)) 
                           ?? stores.First();

        Console.WriteLine($"Scansione: {fileName} ({matchedStore.Name})...");
        var matches = scanner.ScanFlyerPdf(pdf, matchedStore, basket);
        allMatches.AddRange(matches);
    }
}

// Stampa report
Console.WriteLine();
Console.WriteLine("==================================================");
Console.WriteLine("                REPORT PROMOZIONI                 ");
Console.WriteLine("==================================================");

var stockAlerts = allMatches.Where(m => m.IsStockAlert).ToList();
if (stockAlerts.Count > 0)
{
    Console.WriteLine("\n🚨 ALERT SCORTA ATTIVI (Sconti forti / Multipack):");
    foreach (var alert in stockAlerts)
    {
        var priceStr = alert.Price.HasValue ? $"€ {alert.Price.Value:F2}" : "Prezzo n.d.";
        var discStr = alert.DiscountPercentage.HasValue ? $"-{alert.DiscountPercentage}%" : "";
        Console.WriteLine($" • [{alert.Chain}] {alert.MatchedProductName}: {priceStr} {discStr} ({alert.PageNumber})");
        Console.WriteLine($"   Dettaglio: {alert.FlyerSnippet}");
    }
}

var otherMatches = allMatches.Where(m => !m.IsStockAlert).ToList();
if (otherMatches.Count > 0)
{
    Console.WriteLine("\n🛒 ALTRE OFFERTE DEL PANIERE:");
    foreach (var grp in otherMatches.GroupBy(m => m.StoreName))
    {
        Console.WriteLine($"\n--- {grp.Key} ---");
        foreach (var item in grp)
        {
            var priceStr = item.Price.HasValue ? $"€ {item.Price.Value:F2}" : "";
            var discStr = item.DiscountPercentage.HasValue ? $"(-{item.DiscountPercentage}%)" : "";
            
            // Segnalazione Benchmark
            string benchmarkNote = "";
            if (item.BenchmarkPrice.HasValue && item.Price.HasValue)
            {
                if (item.Price.Value > item.BenchmarkPrice.Value)
                {
                    benchmarkNote = $" ⚠️ [ATTENZIONE: {item.BenchmarkDesc} costa meno: € {item.BenchmarkPrice.Value:F2}]";
                }
                else
                {
                    benchmarkNote = $" 🎯 [CONVIENE: batte il prezzo base di € {item.BenchmarkPrice.Value:F2}]";
                }
            }

            Console.WriteLine($" • {item.MatchedProductName} {priceStr} {discStr}{benchmarkNote} ({item.PageNumber})");
        }
    }
}

Console.WriteLine("\nScansione completata!");
