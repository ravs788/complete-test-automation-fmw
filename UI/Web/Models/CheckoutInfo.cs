using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public class CheckoutInfo
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = "";
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = "";
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; } = "";
    }
}
