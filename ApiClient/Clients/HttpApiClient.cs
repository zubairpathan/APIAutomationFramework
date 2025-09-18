using RestSharp;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;

namespace ApiClient.Clients
{
    public class HttpApiClient
    {
        private readonly RestClient _client;
        private readonly string? _token;
        public HttpApiClient(string? baseUrl = null)
        {
            var baseUri = baseUrl ?? Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https://api.restful-api.dev/";
            // load token from env var or appsettings.json at runtime
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _token = Environment.GetEnvironmentVariable("BEARER_TOKEN") ?? config["ApiSettings:BearerToken"];

            _client = new RestClient(baseUri);
        }

        public async Task<GenericResponse> GetAsync(string resource)
        {
            var req = new RestRequest(resource, Method.Get);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            var resp = await _client.ExecuteAsync(req);
            var content = resp?.Content;
            return new GenericResponse { Raw = content, StatusCode = resp != null ? (int)resp.StatusCode : -1 };
        }

        public async Task<GenericResponse> PostJsonAsync(string resource, object body)
        {
            var req = new RestRequest(resource, Method.Post);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            req.AddJsonBody(body);
            var resp = await _client.ExecuteAsync(req);
            var content = resp?.Content;
            return new GenericResponse { Raw = content, StatusCode = resp != null ? (int)resp.StatusCode : -1 };
        }

        public async Task<GenericResponse> PatchJsonAsync(string resource, object body)
        {
            var req = new RestRequest(resource, Method.Patch);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            req.AddJsonBody(body);
            var resp = await _client.ExecuteAsync(req);
            var content = resp?.Content;
            return new GenericResponse { Raw = content, StatusCode = resp != null ? (int)resp.StatusCode : -1 };
        }

        public async Task<GenericResponse> PutJsonAsync(string resource, object body)
        {
            var req = new RestRequest(resource, Method.Put);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            req.AddJsonBody(body);
            var resp = await _client.ExecuteAsync(req);
            var content = resp?.Content;
            return new GenericResponse { Raw = content, StatusCode = resp != null ? (int)resp.StatusCode : -1 };
        }

        public async Task<GenericResponse> DeleteJsonAsync(string resource, object body)
        {
            var req = new RestRequest(resource, Method.Delete);
            if(!string.IsNullOrEmpty(_token)) req.AddHeader("Authorization", $"Bearer {_token}");
            req.AddJsonBody(body);
            var resp = await _client.ExecuteAsync(req);
            var content = resp?.Content;
            return new GenericResponse { Raw = content, StatusCode = resp != null ? (int)resp.StatusCode : -1 };
        }

    }
}

namespace ApiClient.Clients
{
    // Generic response moved here so ApiClient does not require a separate Models folder.
    public class GenericResponse
    {
        public int StatusCode { get; set; }
        public string? Raw { get; set; }
    }
}
