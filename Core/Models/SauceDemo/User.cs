using System.Text.Json.Serialization;

namespace Core.Models.SauceDemo;

public sealed record User
{
    [JsonPropertyName("username")] public string Username { get; init; } = "";
    [JsonPropertyName("password")] public string Password { get; init; } = "";
}
