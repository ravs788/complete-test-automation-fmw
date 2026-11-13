using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public record class CheckoutFlowTestData
    {
        [JsonPropertyName("user")]
        public User User { get; init; } = new User();

        [JsonPropertyName("products")]
        public IReadOnlyList<Product> Products { get; init; } = new List<Product>();

        [JsonPropertyName("checkoutInfo")]
        public CheckoutInfo CheckoutInfo { get; init; } = new CheckoutInfo();
    }
}
