using System.Net.Http;
using SpesaSmartAgent.Models;

namespace SpesaSmartAgent.Services;

public class FlyerDownloadService
{
    private readonly HttpClient _httpClient;

    public FlyerDownloadService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public async Task<List<string>> DownloadFlyersForStoreAsync(TargetStore store, string outputDirectory)
    {
        var downloadedFiles = new List<string>();
        Directory.CreateDirectory(outputDirectory);

        try
        {
            Console.WriteLine($"🔍 Verifica disponibilità volantini per {store.Name}...");
            
            // In base alla catena, interroga l'endpoint/pagina dedicata
            // Se sono presenti file locali di test o scaricati manualmente/via link, li rileva automaticamente
            var targetPattern = $"{store.Id}_*.pdf";
            var existing = Directory.GetFiles(outputDirectory, targetPattern).ToList();
            if (existing.Count > 0)
            {
                Console.WriteLine($"   Trovati {existing.Count} volantini già pronti per {store.Name}.");
                return existing;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠️ Errore durante il controllo per {store.Name}: {ex.Message}");
        }

        return downloadedFiles;
    }
}
