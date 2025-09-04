using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public class CheckoutFlowTestData
    {
        [JsonPropertyName("user")]
        public User User { get; set; } = new User();

        [JsonPropertyName("products")]
        public List<Product> Products { get; set; } = new();

        [JsonPropertyName("checkoutInfo")]
        public CheckoutInfo CheckoutInfo { get; set; } = new CheckoutInfo();
    }
}
