namespace SpesaSmartAgent.Models;

public class TargetStore
{
    public string Id { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class BasketProduct
{
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("keywords")]
    public List<string> Keywords { get; set; } = new();

    [System.Text.Json.Serialization.JsonPropertyName("benchmark_price")]
    public decimal? BenchmarkPrice { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("benchmark_desc")]
    public string? BenchmarkDesc { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("enable_price_comparison")]
    public bool EnablePriceComparison { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("unit_type")]
    public string? UnitType { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("note")]
    public string? Note { get; set; }
}

public class StockProductRule
{
    public string Name { get; set; } = string.Empty;
    public int MinDiscountPct { get; set; } = 25;
    public List<string> AlertKeywords { get; set; } = new();
}

public class BasketConfig
{
    public List<BasketProduct> Fissi { get; set; } = new();
    public List<BasketProduct> Occasionali { get; set; } = new();
    public List<StockProductRule> Scorta { get; set; } = new();
}

public class MatchedOffer
{
    public string StoreName { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public string MatchedProductName { get; set; } = string.Empty;
    public string FlyerSnippet { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public decimal? BenchmarkPrice { get; set; }
    public string? BenchmarkDesc { get; set; }
    public bool BeatsBenchmark => BenchmarkPrice.HasValue && Price.HasValue && Price.Value <= BenchmarkPrice.Value;
    public int? DiscountPercentage { get; set; }
    public bool IsStockAlert { get; set; }
    public string PageNumber { get; set; } = string.Empty;
}
