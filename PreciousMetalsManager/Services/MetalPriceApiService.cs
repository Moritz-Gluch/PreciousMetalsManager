using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PreciousMetalsManager.Services
{
    public class MetalPriceApiService
    {
        private static readonly string ApiUrl = "https://api.edelmetalle.de/public.json";

        public virtual async Task<string?> FetchMetalPricesRawAsync()
        {
            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync(ApiUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        public virtual async Task<MetalPriceApiResponse?> FetchMetalPricesAsync()
        {
            var json = await FetchMetalPricesRawAsync();
            if (json == null)
                return null;

            try
            {
                return JsonSerializer.Deserialize<MetalPriceApiResponse>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
