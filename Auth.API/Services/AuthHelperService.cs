using Microsoft.AspNetCore.Identity;
using Auth.API.Data;
using System.IdentityModel.Tokens.Jwt;
using PharmaTrack.Shared.Services;

namespace Auth.API.Services
{
    public class AuthHelperService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthHelperService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JwtService _jwtService;

        public AuthHelperService(SignInManager<ApplicationUser> signInManager, ILogger<AuthHelperService> logger, IHttpContextAccessor httpContextAccessor, JwtService jwtService, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _userManager = userManager;
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: false);
            return result.Succeeded;
        }

        public async Task<(ApplicationUser? user, string? refreshedToken)> GetUserIdAndRefreshToken()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("Token not found.");
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User ID not found in token.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user?.UserName == null)
                {
                    throw new UnauthorizedAccessException($"User not found for ID '{userId}");
                }

                // Refresh the token
                string refreshedToken = _jwtService.RefreshToken(token);

                return (user, refreshedToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user ID from token.");
                return (null, null);
            }
        }
    }
}
