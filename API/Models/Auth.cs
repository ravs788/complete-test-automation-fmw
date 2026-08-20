namespace API.Models
{
    public record class AuthRequest
    {
        public string username { get; init; } = "";
        public string password { get; init; } = "";
    }

    public record class AuthResponse
    {
        public string token { get; init; } = "";
    }
}
