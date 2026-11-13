using Core.Models.RestfulBooker;

namespace Core.Api.RestfulBooker;

/// <summary>Restful Booker authentication: a token from POST /auth, sent back as a <c>token</c> cookie.</summary>
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

    /// <summary>The request headers that authenticate a write operation with <paramref name="token"/>.</summary>
    public static IReadOnlyDictionary<string, string> Headers(string token) =>
        new Dictionary<string, string> { ["Cookie"] = $"token={token}" };

    /// <summary>Gets a token and returns it as request headers for <see cref="ApiClient"/>.</summary>
    public static async Task<IReadOnlyDictionary<string, string>> GetAuthHeadersAsync(ApiClient client, string username, string password) =>
        Headers(await GetTokenAsync(client, username, password));
}
