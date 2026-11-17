using System.Text.Json.Serialization;

namespace Domain.Models.SauceDemo;

public sealed record Product
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
}
