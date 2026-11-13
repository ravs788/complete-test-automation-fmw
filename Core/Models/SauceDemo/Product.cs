using System.Text.Json.Serialization;

namespace Core.Models.SauceDemo;

public sealed record Product
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
}
