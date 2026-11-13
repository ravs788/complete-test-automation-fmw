namespace Core.TestData.SauceDemo;

using System.Collections.Generic;
using System.Text.Json.Serialization;

// Test-data shapes for the SauceDemo suite. The JSON files live in tests/Tests/test-data/.

public sealed record User
{
    [JsonPropertyName("username")] public string Username { get; init; } = "";
    [JsonPropertyName("password")] public string Password { get; init; } = "";
    [JsonPropertyName("role")] public string? Role { get; init; }
}

public sealed record Product
{
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("id")] public int? Id { get; init; }
    [JsonPropertyName("price")] public decimal? Price { get; init; }
}

public sealed record CheckoutInfo
{
    [JsonPropertyName("firstName")] public string FirstName { get; init; } = "";
    [JsonPropertyName("lastName")] public string LastName { get; init; } = "";
    [JsonPropertyName("postalCode")] public string PostalCode { get; init; } = "";
}

public sealed record AddToCartTestData
{
    [JsonPropertyName("user")] public User User { get; init; } = new();
    [JsonPropertyName("product")] public Product Product { get; init; } = new();
}

public sealed record CheckoutFlowTestData
{
    [JsonPropertyName("user")] public User User { get; init; } = new();
    [JsonPropertyName("products")] public IReadOnlyList<Product> Products { get; init; } = new List<Product>();
    [JsonPropertyName("checkoutInfo")] public CheckoutInfo CheckoutInfo { get; init; } = new();
}

public sealed record NegativeLoginTestData
{
    [JsonPropertyName("user")] public User User { get; init; } = new();
    [JsonPropertyName("expectedError")] public string ExpectedError { get; init; } = string.Empty;
}
