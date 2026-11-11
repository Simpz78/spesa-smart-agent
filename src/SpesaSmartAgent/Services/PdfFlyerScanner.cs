using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using SpesaSmartAgent.Models;

namespace SpesaSmartAgent.Services;

public class PdfFlyerScanner
{
    private static readonly Regex PriceRegex = new(@"(?:EUR|€)\s*(\d{1,3}[,\.]\d{2})|(\d{1,3}[,\.]\d{2})\s*(?:EUR|€)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DiscountRegex = new(@"-?(\d{2})%", RegexOptions.Compiled);

    public List<MatchedOffer> ScanFlyerPdf(string pdfPath, TargetStore store, BasketConfig basket)
    {
        var results = new List<MatchedOffer>();

        if (!File.Exists(pdfPath))
            return results;

        using var document = PdfDocument.Open(pdfPath);

        foreach (var page in document.GetPages())
        {
            // Suddivide per lettere raggruppate o per segmenti di testo/frasi
            var text = page.Text;
            if (string.IsNullOrWhiteSpace(text))
                continue;

            // Riconosce i segmenti separati da a capo o da delimitatori comuni nei volantini
            var segments = Regex.Split(text, @"(?<=(?:EUR|€)\s*\d+[,\.]\d{2})|(?<=\d+[,\.]\d{2}\s*(?:EUR|€))|[\r\n]+")
                                .Select(s => s.Trim())
                                .Where(s => s.Length > 3)
                                .ToList();

            foreach (var segment in segments)
            {
                // Scansione fissi
                foreach (var item in basket.Fissi)
                {
                    TryMatchSegment(results, store, item.Name, item.Keywords, item.BenchmarkPrice, item.BenchmarkDesc, segment, page.Number, basket.Scorta);
                }

                // Scansione occasionali
                foreach (var item in basket.Occasionali)
                {
                    TryMatchSegment(results, store, item.Name, item.Keywords, item.BenchmarkPrice, item.BenchmarkDesc, segment, page.Number, basket.Scorta);
                }
            }
        }

        return results;
    }

    private void TryMatchSegment(
        List<MatchedOffer> results,
        TargetStore store,
        string productName,
        List<string> keywords,
        decimal? benchmarkPrice,
        string? benchmarkDesc,
        string segment,
        int pageNumber,
        List<StockProductRule> scortaRules)
    {
        bool matched = keywords.Any(k => segment.Contains(k, StringComparison.OrdinalIgnoreCase));
        if (!matched)
            return;

        // Estrae il prezzo
        decimal? price = null;
        var pMatch = PriceRegex.Match(segment);
        if (pMatch.Success)
        {
            var valStr = !string.IsNullOrEmpty(pMatch.Groups[1].Value) ? pMatch.Groups[1].Value : pMatch.Groups[2].Value;
            if (decimal.TryParse(valStr.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            {
                price = parsed;
            }
        }

        // Estrae sconto percentuale
        int? discount = null;
        var dMatch = DiscountRegex.Match(segment);
        if (dMatch.Success && int.TryParse(dMatch.Groups[1].Value, out var parsedDisc))
        {
            discount = parsedDisc;
        }

        // Controlla trigger scorta
        var scortaRule = scortaRules.FirstOrDefault(s => s.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
        bool isStockAlert = false;
        if (scortaRule != null)
        {
            if (discount.HasValue && discount.Value >= scortaRule.MinDiscountPct)
            {
                isStockAlert = true;
            }
            else if (scortaRule.AlertKeywords.Any(k => segment.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                isStockAlert = true;
            }
        }

        // Evita duplicati identici
        if (!results.Any(r => r.StoreName == store.Name && r.MatchedProductName == productName && r.Price == price))
        {
            results.Add(new MatchedOffer
            {
                StoreName = store.Name,
                Chain = store.Chain,
                MatchedProductName = productName,
                FlyerSnippet = segment,
                Price = price,
                BenchmarkPrice = benchmarkPrice,
                BenchmarkDesc = benchmarkDesc,
                DiscountPercentage = discount,
                IsStockAlert = isStockAlert,
                PageNumber = $"p. {pageNumber}"
            });
        }
    }
}
