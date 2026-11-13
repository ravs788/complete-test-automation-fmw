using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public record class CheckoutInfo
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; init; } = "";
        [JsonPropertyName("lastName")]
        public string LastName { get; init; } = "";
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; init; } = "";
    }
}
