using System.Text.Json.Serialization;

namespace UI.Web.Models
{
    public class User
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = "";
        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }
}
