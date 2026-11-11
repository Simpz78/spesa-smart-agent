using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        var url = "https://www.tuttoesselunga.com/volantino-esselunga-5-18-novembre-2026-sconti-fino-al-50/";

        try
        {
            var html = await client.GetStringAsync(url);
            Console.WriteLine($"Scaricata pagina HTML ({html.Length} caratteri)");
            var matches = Regex.Matches(html, @"https://[^\s""']+(?:volantino|esselunga)[^\s""']*\.(?:jpg|jpeg|png|webp|pdf)", RegexOptions.IgnoreCase);
            Console.WriteLine($"Trovate {matches.Count} immagini/file del volantino reale:");
            int count = 0;
            foreach (Match m in matches)
            {
                Console.WriteLine(" - " + m.Value);
                if (++count >= 15) break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore: " + ex.Message);
        }
    }
}
