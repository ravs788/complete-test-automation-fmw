using System.Threading.Tasks;
using RestSharp;
using API.Models;
using Core.Utilities;

namespace API.Utilities
{
    public static class AuthHelper
    {
        public static async Task<string> GetAuthTokenAsync()
        {
            var auth = new AuthRequest
            {
                username = ConfigLoader.Load<API.Utilities.ConfigSettings>().DefaultUsername,
                password = ConfigLoader.Load<API.Utilities.ConfigSettings>().DefaultPassword
            };
            var client = new RestClient(ConfigLoader.Load<API.Utilities.ConfigSettings>().BaseUrl);
            var request = new RestRequest("auth", Method.Post)
                .AddJsonBody(auth);

            var response = await client.ExecuteAsync<AuthResponse>(request);
            if (response.IsSuccessful && response.Data != null && !string.IsNullOrEmpty(response.Data.token))
                return response.Data.token;
            throw new RestClientException("Failed to obtain token for authenticated API operations.");
        }
    }
}
