using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public class Product
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
    }
}
