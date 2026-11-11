using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = true };
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        try
        {
            var html = await client.GetStringAsync("https://www.lidl.it/l/it/volantini");
            Console.WriteLine($"Lidl volantini page loaded (length: {html.Length})");
            var matches = Regex.Matches(html, @"href=""(/l/it/volantini/[^""]+)""");
            Console.WriteLine($"Trovati {matches.Count} link volantino:");
            foreach (Match m in matches)
            {
                Console.WriteLine(" - https://www.lidl.it" + m.Groups[1].Value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore: " + ex.Message);
        }
    }
}
