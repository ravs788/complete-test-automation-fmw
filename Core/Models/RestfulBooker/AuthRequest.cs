using System.Text.Json.Serialization;

namespace Core.Models.RestfulBooker;

/// <summary>Body of POST /auth.</summary>
public sealed record AuthRequest
{
    [JsonPropertyName("username")] public string Username { get; init; } = "";
    [JsonPropertyName("password")] public string Password { get; init; } = "";
}
