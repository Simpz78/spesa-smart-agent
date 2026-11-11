using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using SpesaSmartAgent.Models;

namespace SpesaSmartAgent.Services;

public class PdfFlyerScanner
{
    private static readonly Regex PriceRegex = new(@"(\d{1,3}[,\.]\d{2})\s*€?", RegexOptions.Compiled);
    private static readonly Regex DiscountRegex = new(@"-?(\d{2})%", RegexOptions.Compiled);

    public List<MatchedOffer> ScanFlyerPdf(string pdfPath, TargetStore store, BasketConfig basket)
    {
        var results = new List<MatchedOffer>();

        if (!File.Exists(pdfPath))
            return results;

        using var document = PdfDocument.Open(pdfPath);

        foreach (var page in document.GetPages())
        {
            var text = page.Text;
            if (string.IsNullOrWhiteSpace(text))
                continue;

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 2)
                            .ToList();

            // Scansione prodotti fissi
            foreach (var item in basket.Fissi)
            {
                CheckAndAddMatches(results, store, item.Name, item.Keywords, lines, page.Number, basket.Scorta, isFixed: true);
            }

            // Scansione occasionali
            foreach (var item in basket.Occasionali)
            {
                CheckAndAddMatches(results, store, item.Name, item.Keywords, lines, page.Number, basket.Scorta, isFixed: false);
            }
        }

        return results;
    }

    private void CheckAndAddMatches(
        List<MatchedOffer> results,
        TargetStore store,
        string productName,
        List<string> keywords,
        List<string> lines,
        int pageNumber,
        List<StockProductRule> scortaRules,
        bool isFixed)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            bool matched = keywords.Any(k => line.Contains(k, StringComparison.OrdinalIgnoreCase));
            if (!matched)
                continue;

            // Raccoglie contesto (la riga stessa + 1-2 righe successive per prezzo/sconto)
            var contextLines = lines.Skip(Math.Max(0, i - 1)).Take(4).ToList();
            var fullContext = string.Join(" | ", contextLines);

            // Cerca prezzo e sconto
            decimal? price = null;
            var priceMatch = PriceRegex.Match(fullContext);
            if (priceMatch.Success && decimal.TryParse(priceMatch.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedPrice))
            {
                price = parsedPrice;
            }

            int? discount = null;
            var discMatch = DiscountRegex.Match(fullContext);
            if (discMatch.Success && int.TryParse(discMatch.Groups[1].Value, out var parsedDisc))
            {
                discount = parsedDisc;
            }

            // Verifica regola scorta
            var scortaRule = scortaRules.FirstOrDefault(s => s.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
            bool isStockAlert = false;
            if (scortaRule != null)
            {
                if (discount.HasValue && discount.Value >= scortaRule.MinDiscountPct)
                {
                    isStockAlert = true;
                }
                else if (scortaRule.AlertKeywords.Any(k => fullContext.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    isStockAlert = true;
                }
            }

            // Evita duplicati identici sulla stessa pagina
            if (!results.Any(r => r.StoreName == store.Name && r.MatchedProductName == productName && r.PageNumber == $"p. {pageNumber}"))
            {
                results.Add(new MatchedOffer
                {
                    StoreName = store.Name,
                    Chain = store.Chain,
                    MatchedProductName = productName,
                    FlyerSnippet = fullContext,
                    Price = price,
                    DiscountPercentage = discount,
                    IsStockAlert = isStockAlert,
                    PageNumber = $"p. {pageNumber}"
                });
            }
        }
    }
}
