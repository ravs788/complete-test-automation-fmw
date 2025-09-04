namespace API.Models
{
    public class AuthRequest
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";
    }

    public class AuthResponse
    {
        public string token { get; set; } = "";
    }
}
