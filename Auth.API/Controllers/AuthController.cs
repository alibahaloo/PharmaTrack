using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Auth.API.Data;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly AuthDBContext _dbContext;
        //private readonly EmailService _emailService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService,
            ILogger<AuthController> logger,
            IUserStore<ApplicationUser> userStore,
            AuthDBContext dBContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _logger = logger;
            _userStore = userStore;
            _dbContext = dBContext;
            _emailStore = GetEmailStore();
        }
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest model)
        {
            try
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, model.Username, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, model.Username, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);

                    //Send confirmation email
                    //_emailService.SendConfirmation(Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

                    if (user == null || string.IsNullOrEmpty(user.UserName))
                    {
                        _logger.LogWarning("Login failed: User '{Username}' not found.", model.Username);
                        return Unauthorized(new { Success = false, Message = "User not found." });
                    }

                    var accessToken = _jwtService.GenerateJwtToken(user.Id, user.UserName);
                    var refreshToken = _jwtService.GenerateSecureRefreshToken();
                    var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                    // Store the Refresh Token in the database
                    await AddRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

                    return Ok(new { Success = true, Content = new { accessToken, refreshToken, user.UserName } });
                }
                else
                {
                    var response = new { Success = false, Content = result.Errors };
                    _logger.LogError("An error occurred during user registration '{Username}'.", model.Username);
                    return Unauthorized(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user registration '{Username}'.", model.Username);
                return Unauthorized();
            }
        }
        public async Task AddRefreshTokenAsync(string userId, string token, DateTime expiryDate)
        {
            // Remove existing refresh tokens for the user
            var existingTokens = _dbContext.RefreshTokens.Where(rt => rt.UserId == userId);
            _dbContext.RefreshTokens.RemoveRange(existingTokens);

            // Add the new refresh token
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = userId,
                ExpiryDate = expiryDate,
                CreatedDate = DateTime.UtcNow
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                var signInResult = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, lockoutOnFailure: false);
                if (!signInResult.Succeeded)
                {
                    _logger.LogWarning("Login failed: Invalid credentials for username '{Username}'.", model.Username);
                    return Unauthorized(new { Success = false, Content = "Invalid credentials." });
                }

                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null || string.IsNullOrEmpty(user.UserName))
                {
                    _logger.LogWarning("Login failed: User '{Username}' not found.", model.Username);
                    return Unauthorized(new { Success = false, Content = "User not found." });
                }

                var accessToken = _jwtService.GenerateJwtToken(user.Id, user.UserName);
                var refreshToken = _jwtService.GenerateSecureRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                // Store the Refresh Token in the database
                await AddRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

                return Ok(new { Success = true, Content = new { accessToken, refreshToken, user.UserName } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user '{Username}'.", model.Username);
                return Unauthorized();
            }
        }
        public async Task<RefreshToken?> ValidateRefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (tokenEntity == null || tokenEntity.ExpiryDate < DateTime.UtcNow)
            {
                return null; // Token is invalid or expired
            }

            return tokenEntity;
        }
        public async Task UpdateRefreshTokenAsync(RefreshToken tokenEntity)
        {
            _dbContext.RefreshTokens.Update(tokenEntity);
            await _dbContext.SaveChangesAsync();
        }
        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            try
            {
                // Step 1: Validate the incoming refresh token
                var tokenEntity = await ValidateRefreshTokenAsync(model.RefreshToken);
                if (tokenEntity == null)
                {
                    _logger.LogWarning("Invalid refresh token.");
                    return Unauthorized(new { Success = false, Message = "Invalid or expired refresh token." });
                }

                //Find user based on UserID
                var user = await _userManager.FindByIdAsync(tokenEntity.UserId);

                if (user == null || string.IsNullOrEmpty(user.UserName)) {
                    _logger.LogWarning("User not found.");
                    return Unauthorized(new { Success = false, Message = "User not found." });
                }

                // Step 2: Generate a new Access Token
                var newAccessToken = _jwtService.GenerateJwtToken(tokenEntity.UserId, user.UserName); // Replace with actual username if needed

                // Step 3: (Optional) Generate a new Refresh Token and replace the old one
                var newRefreshToken = _jwtService.GenerateSecureRefreshToken();
                tokenEntity.Token = newRefreshToken;
                tokenEntity.ExpiryDate = DateTime.UtcNow.AddDays(7); // Update expiry
                await UpdateRefreshTokenAsync(tokenEntity);

                // Step 4: Return the new tokens
                return Ok(new
                {
                    Success = true,
                    Content = new
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        user.UserName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing the token.");
                return BadRequest(new { Success = false, Message = "An error occurred. Please try again." });
            }
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest model)
        {
            try
            {
                // Validate the incoming refresh token
                var tokenEntity = await ValidateRefreshTokenAsync(model.RefreshToken);
                if (tokenEntity == null)
                {
                    _logger.LogWarning("Invalid refresh token provided for logout.");
                    return Unauthorized(new { Success = false, Message = "Invalid or expired refresh token." });
                }

                // Remove the refresh token from the database
                _dbContext.RefreshTokens.Remove(tokenEntity);
                await _dbContext.SaveChangesAsync();

                // Respond with success
                return Ok(new { Success = true, Message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout.");
                return BadRequest(new { Success = false, Message = "An error occurred while logging out. Please try again." });
            }
        }

    }
}
