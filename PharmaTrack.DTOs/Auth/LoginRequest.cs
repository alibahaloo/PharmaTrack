using System.ComponentModel.DataAnnotations;

namespace PharmaTrack.DTOs.Auth
{
    public class LoginRequest
    {
        [EmailAddress]
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
