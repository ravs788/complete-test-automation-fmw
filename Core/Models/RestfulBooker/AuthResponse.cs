using System.Text.Json.Serialization;

namespace Core.Models.RestfulBooker;

/// <summary>Response of POST /auth.</summary>
public sealed record AuthResponse
{
    [JsonPropertyName("token")] public string Token { get; init; } = "";
}
