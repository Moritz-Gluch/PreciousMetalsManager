using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace PreciousMetalsManager.Services
{
    public class MetalPriceApiService
    {
        private static readonly string ApiUrl = "https://api.edelmetalle.de/public.json";

        public async Task<string?> FetchMetalPricesRawAsync()
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

        public async Task<MetalPriceApiResponse?> FetchMetalPricesAsync()
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
