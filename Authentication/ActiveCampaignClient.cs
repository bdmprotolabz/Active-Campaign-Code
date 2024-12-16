using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.Authentication
{
    public class ActiveCampaignClient
    {
        private readonly HttpClient _httpClient;

        // Constructor that accepts API Key and Base URL
        public ActiveCampaignClient(string apiKey = null, string baseUrl = null)
        {
            // Use environment variables if no parameters are passed
            apiKey ??= Environment.GetEnvironmentVariable("ACTIVE_CAMPAIGN_API_KEY");
            baseUrl ??= Environment.GetEnvironmentVariable("ACTIVE_CAMPAIGN_BASE_URL");

            // Check if API Key and Base URL are set
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("API Key and Base URL must be provided.");
            }

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            _httpClient.DefaultRequestHeaders.Add("Api-Token", apiKey);
        }


        // POST request with JSON data
        public async Task<HttpResponseMessage> PostAsJsonAsync(string url, object data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(url, content);
        }

        // PUT request with JSON data
        public async Task<HttpResponseMessage> PutAsJsonAsync(string url, object data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync(url, content);
        }

        // GET request
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            return response;
        }

        // DELETE request
        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            return response;
        }
    }
}
