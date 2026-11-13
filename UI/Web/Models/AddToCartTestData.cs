using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public record class AddToCartTestData
    {
        [JsonPropertyName("user")]
        public User User { get; init; } = new User();

        [JsonPropertyName("product")]
        public Product Product { get; init; } = new Product();
    }
}
