using Core.Models.RestfulBooker;

namespace Core.Api.RestfulBooker;

public static class AuthClient
{
    /// <summary>POST /auth and return the token used by PUT, PATCH and DELETE.</summary>
    public static async Task<string> GetTokenAsync(ApiClient client, string username, string password)
    {
        var response = await client.PostAsync<AuthResponse>("auth", new AuthRequest { Username = username, Password = password });
        return response is { Token.Length: > 0 }
            ? response.Token
            : throw new ApiException("POST", "auth", 200, "no token returned (wrong credentials?)");
    }
}
