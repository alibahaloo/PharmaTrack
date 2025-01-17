using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PharmaTrack.Shared.Services
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
    }

    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateSecureRefreshToken(int size = 64)
        {
            var randomNumber = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }


        public string GenerateJwtToken(string userId, string username, bool isAdmin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("userId", userId), // Add userId as a claim
                    new Claim(ClaimTypes.Role, isAdmin ? "admin" : "user")
                }),
                Expires = DateTime.UtcNow.AddMinutes(15), // Short-lived token (15 minutes)
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public (bool IsValid, string? Username, bool IsAdmin) ValidateAccessToken(string token)
        {
            try
            {
                // Decode and validate the JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer, // Replace with your actual issuer
                    ValidAudience = _jwtSettings.Audience, // Replace with your actual audience
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)) // Replace with your secret key
                };

                // Validate the token and get the principal
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                // Extract the "username" claim
                var usernameClaim = principal.FindFirst(ClaimTypes.Name); // ClaimTypes.Name is used for the username
                var username = usernameClaim?.Value;

                var userRoleClaim = principal.FindFirst(ClaimTypes.Role);
                var isAdmin = (userRoleClaim?.Value) switch
                {
                    "admin" => true,
                    _ => false,
                };
                return (true, username, isAdmin);
            }
            catch (Exception)
            {
                // Return false with null username if validation fails
                return (false, null, false);
            }
        }
        public (IActionResult? ValidationResult, string? Username, bool IsAdmin) ValidateAuthorizationHeader(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return (new ObjectResult(new { Success = false, Message = "Authorization header is missing." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                }, null, false);
            }

            var token = authHeader.ToString().Replace("Bearer ", string.Empty);
            if (string.IsNullOrEmpty(token))
            {
                return (new ObjectResult(new { Success = false, Message = "Token is missing." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                }, null, false);
            }

            var (isValid, username, isAdmin) = ValidateAccessToken(token);
            if (!isValid || string.IsNullOrEmpty(username))
            {
                return (new ObjectResult(new { Success = false, Message = "Invalid or expired token." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                }, null, false);
            }

            return (null, username, isAdmin);
        }

    }
}
