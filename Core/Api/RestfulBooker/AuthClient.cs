namespace Core.Api.RestfulBooker;

using System.Threading.Tasks;

public static class AuthClient
{
    /// <summary>POST /auth and return the token used by PUT, PATCH and DELETE.</summary>
    public static async Task<string> GetTokenAsync(ApiClient client, string username, string password)
    {
        var response = await client.PostAsync<AuthResponse>("auth", new AuthRequest { username = username, password = password });
        return response is { token.Length: > 0 }
            ? response.token
            : throw new ApiException("POST", "auth", 200, "no token returned (wrong credentials?)");
    }
}
