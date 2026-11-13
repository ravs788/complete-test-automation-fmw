using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    /// <summary>
    /// Test-data object for negative / invalid login scenarios.
    /// </summary>
    public record class NegativeLoginTestData
    {
        [JsonPropertyName("user")]
        public User User { get; init; } = new User();

        [JsonPropertyName("expectedError")]
        public string ExpectedError { get; init; } = string.Empty;
    }
}
