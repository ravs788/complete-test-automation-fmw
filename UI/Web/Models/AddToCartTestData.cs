using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public class AddToCartTestData
    {
        [JsonPropertyName("user")]
        public User User { get; set; } = new User();

        [JsonPropertyName("product")]
        public Product Product { get; set; } = new Product();
    }
}
