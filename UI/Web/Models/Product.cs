using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public record class Product
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = "";
        [JsonPropertyName("id")]
        public int? Id { get; init; }
        [JsonPropertyName("price")]
        public decimal? Price { get; init; }
    }
}
