namespace PharmaTrack.Shared.DTOs
{
    public class AuthDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public bool IsAdmin { get; set; } = false;
    }
}
