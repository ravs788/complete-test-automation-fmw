using System.Text.Json.Serialization;

namespace Core.Models.SauceDemo;

public sealed record CheckoutInfo
{
    [JsonPropertyName("firstName")] public string FirstName { get; init; } = "";
    [JsonPropertyName("lastName")] public string LastName { get; init; } = "";
    [JsonPropertyName("postalCode")] public string PostalCode { get; init; } = "";
}
