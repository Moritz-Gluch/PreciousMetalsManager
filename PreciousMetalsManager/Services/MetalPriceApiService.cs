using System;
using System.Net.Http;
using System.Threading.Tasks;

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
    }
}
