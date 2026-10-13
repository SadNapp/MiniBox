using System.Net.Http;

namespace MiniBox.API;

public static class HealthCheck
{
    public static async Task<int> RunAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            using var response = await client.GetAsync("http://127.0.0.1:8080/health/live");
            return response.IsSuccessStatusCode ? 0 : 1;
        }
        catch { return 1; }
    }
}
