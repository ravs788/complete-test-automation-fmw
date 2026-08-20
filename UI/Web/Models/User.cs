using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public record class User
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = "";
        [JsonPropertyName("password")]
        public string Password { get; init; } = "";
        [JsonPropertyName("role")]
        public string? Role { get; init; }
    }
}
