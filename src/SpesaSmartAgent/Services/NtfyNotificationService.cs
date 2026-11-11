using System.Net.Http;
using System.Text;

namespace SpesaSmartAgent.Services;

public class NtfyNotificationService
{
    private readonly HttpClient _httpClient;

    public NtfyNotificationService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<bool> SendReportAsync(string topic, string title, string message)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return false;

        try
        {
            var url = $"https://ntfy.sh/{topic}";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(message, Encoding.UTF8, "text/plain")
            };

            request.Headers.Add("Title", title);
            request.Headers.Add("Priority", "high");
            request.Headers.Add("Tags", "shopping_cart,euro");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore invio notifica ntfy: {ex.Message}");
            return false;
        }
    }
}
