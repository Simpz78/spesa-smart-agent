using System.Text;
using System.Text.Json;
using SpesaSmartAgent.Models;
using SpesaSmartAgent.Services;

Console.WriteLine("==================================================");
Console.WriteLine("      SPESA SMART AGENT - BOLOGNA & CASALECCHIO   ");
Console.WriteLine("==================================================");

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

// Genera volantini campione se assenti
var esselungaPdf = Path.Combine(downloadsDir, "esselunga_casalecchio_sample.pdf");
if (!File.Exists(esselungaPdf))
{
    SpesaSmartAgent.Tools.TestPdfGenerator.CreateSampleFlyer(esselungaPdf);
}

var lidlPdf = Path.Combine(downloadsDir, "lidl_stalingrado_sample.pdf");
if (!File.Exists(lidlPdf))
{
    SpesaSmartAgent.Tools.LidlTestPdfGenerator.CreateSampleFlyer(lidlPdf);
}

var scanner = new PdfFlyerScanner();
var allMatches = new List<MatchedOffer>();

var pdfFiles = Directory.GetFiles(downloadsDir, "*.pdf");
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

// Generazione Report in memoria
var sb = new StringBuilder();
sb.AppendLine("🛒 REPORT SETTIMANALE SPESA SMART");
sb.AppendLine($"📅 Data: {DateTime.Now:dd/MM/yyyy HH:mm}");
sb.AppendLine("📍 Supermercato Casa Base: ESSELUNGA (Casalecchio di Reno)");
sb.AppendLine("==================================================");

// 1. ALERT SCORTA ESSELUNGA
var esselungaStockAlerts = allMatches
    .Where(m => m.IsStockAlert && m.Chain.Equals("Esselunga", StringComparison.OrdinalIgnoreCase))
    .ToList();

if (esselungaStockAlerts.Count > 0)
{
    sb.AppendLine("\n🚨 ALERT SCORTA DA FARE DA ESSELUNGA:");
    foreach (var alert in esselungaStockAlerts)
    {
        var priceStr = alert.Price.HasValue ? $"€ {alert.Price.Value:F2}" : "Prezzo n.d.";
        var discStr = alert.DiscountPercentage.HasValue ? $"-{alert.DiscountPercentage}%" : "";
        sb.AppendLine($" • {alert.MatchedProductName}: {priceStr} {discStr} ({alert.PageNumber})");
    }
}

// 2. CONFRONTO PREZZI (Solo sui 4 prodotti selezionati: Tonno, Passata, Pollo al forno, Carta cucina)
var comparisonProducts = basket.Fissi.Where(p => p.EnablePriceComparison).ToList();
sb.AppendLine("\n⚖️ CONFRONTO MIRATO (ESSELUNGA vs ALTRI STORE):");

foreach (var prod in comparisonProducts)
{
    var essMatch = allMatches.FirstOrDefault(m => m.Chain.Contains("Esselunga", StringComparison.OrdinalIgnoreCase) && m.MatchedProductName.Equals(prod.Name, StringComparison.OrdinalIgnoreCase));
    var competitorMatches = allMatches.Where(m => !m.Chain.Contains("Esselunga", StringComparison.OrdinalIgnoreCase) && m.MatchedProductName.Equals(prod.Name, StringComparison.OrdinalIgnoreCase)).ToList();

    sb.AppendLine($"\n🔹 {prod.Name.ToUpper()}:");
    if (essMatch != null && essMatch.Price.HasValue)
    {
        sb.AppendLine($"   🏠 Esselunga (Base): € {essMatch.Price.Value:F2} (sconto {essMatch.DiscountPercentage ?? 0}%)");
    }
    else if (prod.BenchmarkPrice.HasValue)
    {
        sb.AppendLine($"   🏠 Esselunga (Scaffale fisso): € {prod.BenchmarkPrice.Value:F2} ({prod.BenchmarkDesc})");
    }
    else
    {
        sb.AppendLine("   🏠 Esselunga: non presente a volantino questa settimana");
    }

    // Controlla se un concorrente batte Esselunga
    var cheaperCompetitor = competitorMatches
        .Where(c => c.Price.HasValue)
        .OrderBy(c => c.Price!.Value)
        .FirstOrDefault();

    decimal? basePrice = essMatch?.Price ?? prod.BenchmarkPrice;

    if (cheaperCompetitor != null && basePrice.HasValue && cheaperCompetitor.Price < basePrice)
    {
        var diff = basePrice.Value - cheaperCompetitor.Price!.Value;
        sb.AppendLine($"   ⚡ CONVIENE ALTROVE! 🥇 {cheaperCompetitor.StoreName}: € {cheaperCompetitor.Price.Value:F2}");
        sb.AppendLine($"      👉 Risparmio: -€ {diff:F2} a {prod.UnitType ?? "pezzo"}");
    }
    else if (cheaperCompetitor != null)
    {
        sb.AppendLine($"   ✔️ Resta su Esselunga ({cheaperCompetitor.Chain} costa di più: € {cheaperCompetitor.Price!.Value:F2})");
    }
    else
    {
        sb.AppendLine("   ✔️ Nessuna offerta concorrente migliore: compra tranquillamente da Esselunga.");
    }
}

// 3. PRODOTTI FISSI ESSELUNGA
var otherEsselunga = allMatches
    .Where(m => m.Chain.Equals("Esselunga", StringComparison.OrdinalIgnoreCase) && !m.IsStockAlert && !comparisonProducts.Any(c => c.Name.Equals(m.MatchedProductName, StringComparison.OrdinalIgnoreCase)))
    .ToList();

if (otherEsselunga.Count > 0)
{
    sb.AppendLine("\n🛒 ALTRI PRODOTTI DEL TUO PANIERE DA ESSELUNGA:");
    foreach (var item in otherEsselunga)
    {
        var priceStr = item.Price.HasValue ? $"€ {item.Price.Value:F2}" : "";
        var discStr = item.DiscountPercentage.HasValue ? $"(-{item.DiscountPercentage}%)" : "";
        sb.AppendLine($" • {item.MatchedProductName} {priceStr} {discStr} ({item.PageNumber})");
    }
}

var reportText = sb.ToString();
Console.WriteLine(reportText);

// Salvataggio su file reports/
var reportsDir = Path.Combine(projectRoot, "reports");
Directory.CreateDirectory(reportsDir);
var reportFile = Path.Combine(reportsDir, $"report_{DateTime.Now:yyyy-MM-dd}.txt");
File.WriteAllText(reportFile, reportText);
Console.WriteLine($"\nReport salvato in: {reportFile}");

// Invio notifica ntfy se presente configurazione topic
var ntfyTopic = Environment.GetEnvironmentVariable("NTFY_TOPIC");
if (!string.IsNullOrWhiteSpace(ntfyTopic))
{
    Console.WriteLine($"Invio notifica ntfy su topic: {ntfyTopic}...");
    var ntfy = new NtfyNotificationService();
    var sent = await ntfy.SendReportAsync(ntfyTopic, "Spesa Smart - Report Offerte", reportText);
    Console.WriteLine(sent ? "Notifica inviata con successo!" : "Errore durante l'invio della notifica.");
}
else
{
    Console.WriteLine("\n[Info ntfy] Per ricevere la notifica sullo smartphone, imposta la variabile d'ambiente NTFY_TOPIC.");
}
