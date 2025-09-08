using RestSharp;
using System.Threading.Tasks;
using API.Utilities;
using Core.Utilities;

namespace API.Utilities
{
    public class ApiClient
    {
        protected readonly RestClient _client;

        public ApiClient()
        {
            _client = new RestClient(ConfigManager.Instance.Settings.BaseUrl);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var request = new RestRequest(endpoint, Method.Get);
            request.AddHeader("Accept", "application/json");
            // Log the fully composed GET URL for diagnostics
            var response = await _client.ExecuteAsync<T>(request);
            if (!response.IsSuccessful)
                throw new RestClientException($"GET {endpoint} failed: {(int)response.StatusCode} {response.StatusDescription}");

            return response.Data;
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data) where TRequest : class
        {
            var request = new RestRequest(endpoint, Method.Post)
                .AddHeader("Accept", "application/json")
                .AddJsonBody(data);
            var response = await _client.ExecuteAsync<TResponse>(request);
            if (!response.IsSuccessful)
                throw new RestClientException($"POST {endpoint} failed: {(int)response.StatusCode} {response.StatusDescription}");

            return response.Data;
        }

        public async Task<RestResponse> DeleteAsync(string endpoint, string? token = null)
        {
            var request = new RestRequest(endpoint, Method.Delete);
            request.AddHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(token))
            {
                request.AddCookie("token", token, "/", GetDomainFromBaseUrl());
            }
            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new RestClientException($"DELETE {endpoint} failed: {(int)response.StatusCode} {response.StatusDescription}");
            return response;
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, string? token = null) where TRequest : class
        {
            var request = new RestRequest(endpoint, Method.Put)
                .AddHeader("Accept", "application/json")
                .AddJsonBody(data);
            if (!string.IsNullOrEmpty(token))
            {
                request.AddCookie("token", token, "/", GetDomainFromBaseUrl());
            }
            var response = await _client.ExecuteAsync<TResponse>(request);
            if (!response.IsSuccessful)
                throw new RestClientException($"PUT {endpoint} failed: {(int)response.StatusCode} {response.StatusDescription}");
            return response.Data;
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest data, string? token = null) where TRequest : class
        {
            var request = new RestRequest(endpoint, Method.Patch)
                .AddHeader("Accept", "application/json")
                .AddJsonBody(data);
            if (!string.IsNullOrEmpty(token))
            {
                request.AddCookie("token", token, "/", GetDomainFromBaseUrl());
            }
            var response = await _client.ExecuteAsync<TResponse>(request);
            if (!response.IsSuccessful)
                throw new RestClientException($"PATCH {endpoint} failed: {(int)response.StatusCode} {response.StatusDescription}");
            return response.Data;
        }

        private string GetDomainFromBaseUrl()
        {
            var uri = new System.Uri(ConfigManager.Instance.Settings.BaseUrl);
            return uri.Host;
        }

        public void Dispose()
        {
            // RestClient does not implement IDisposable in recent versions, leaving empty for symmetry.
        }
    }

    public class RestClientException : System.Exception
    {
        public RestClientException(string message) : base(message) { }
    }
}
