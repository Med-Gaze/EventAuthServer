using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace med.common.library.configuration.service
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private readonly ModuleSettings settings;
        private readonly string TanentBaseUrl;

        public ConfigurationService(HttpClient httpClient, IOptions<ModuleSettings> settings, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.httpClient = httpClient;
            this.settings = settings.Value;
            this.TanentBaseUrl = this.configuration.GetSection("APIServiceBaseUrl:Tanent").Value;
        }
        public async Task<bool> SeedModules()
        {
            var json = JsonConvert.SerializeObject(settings);
            //construct content to send
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{this.TanentBaseUrl}/api/v1/module"),
                Content = content
            };

            var response = await this.httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new Exception(msg);
            }
            return true;

        }
    }
}
