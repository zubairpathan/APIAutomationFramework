using RestSharp;
using Microsoft.Extensions.Configuration;

namespace ApiClient.Clients
{
    public class HttpApiClient
    {
        private readonly RestClient _client;
        private readonly string? _token;
        public HttpApiClient(string? baseUrl = null)
        {
           
            // load token from env var or appsettings.json at runtime
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
             var baseUri = config["ApiSettings:BaseUrl"] ?? Environment.GetEnvironmentVariable("BaseUrl");
            _token = Environment.GetEnvironmentVariable("BEARER_TOKEN") ?? config["ApiSettings:BearerToken"];

            if (string.IsNullOrEmpty(baseUri))
                throw new ArgumentNullException(nameof(baseUri), "Base URL for RestClient cannot be null or empty.");

            _client = new RestClient(baseUri);
        }

        public async Task<RestResponse> GetAsync(string resource)
        {
            var req = new RestRequest(resource, Method.Get);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            var resp = await _client.ExecuteAsync(req);
            return resp;
        }

        public async Task<RestResponse> PostJsonAsync(string resource, object body)
        {
            var req = new RestRequest(resource, Method.Post);
            if (!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            req.AddJsonBody(body);
            var resp = await _client.ExecuteAsync(req);
            return resp;
        }

        public async Task<RestResponse> PatchJsonAsync(string resource, object body)
        {
            var req = new RestRequest(resource, Method.Patch);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            req.AddJsonBody(body);
            var resp = await _client.ExecuteAsync(req);
            return resp;
        }

        public async Task<RestResponse> PutJsonAsync(string resource, object body)
        {
            var req = new RestRequest(resource, Method.Put);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            req.AddJsonBody(body);
            var resp = await _client.ExecuteAsync(req);
            return resp;
        }

        public async Task<RestResponse> DeleteJsonAsync(string resource, object body)
        {
            var req = new RestRequest(resource, Method.Delete);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            req.AddJsonBody(body);
            var resp = await _client.ExecuteAsync(req);
            return resp;
        }
    }
}