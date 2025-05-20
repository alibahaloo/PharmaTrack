using System.Text.Json.Serialization;

namespace PharmaTrack.Core.DTOs
{
    public class AuthDto
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = default!;
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = default!;
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = default!;
        [JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; } = false;
    }
}
