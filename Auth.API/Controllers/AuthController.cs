using Auth.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Auth.API.Data;
using PharmaTrack.Shared.APIModels;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthHelperService _authHelper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        //private readonly EmailService _emailService;

        public AuthController(
            AuthHelperService authHelper,
            UserManager<ApplicationUser> userManager,
            JwtService jwtService,
            ILogger<AuthController> logger,
            IUserStore<ApplicationUser> userStore)
        {
            _authHelper = authHelper;
            _userManager = userManager;
            _jwtService = jwtService;
            _logger = logger;
            _userStore = userStore;
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

                    var token = _jwtService.GenerateJwtToken(user.Id, user.UserName);
                    return Ok(new { Success = true, Content = new { token, user.UserName } });
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

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                if (!await _authHelper.ValidateUserCredentialsAsync(model.Username, model.Password))
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

                var token = _jwtService.GenerateJwtToken(user.Id, user.UserName);
                return Ok(new { Success = true, Content = new { token, user.UserName } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user '{Username}'.", model.Username);
                return Unauthorized();
            }
        }

        [HttpGet]
        [Route("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var (user, refreshedToken) = await _authHelper.GetUserIdAndRefreshToken();

                if (refreshedToken == null || user == null)
                {
                    return Unauthorized();
                }

                return Ok(new { Success = true, Content = new { refreshedToken, user.UserName } });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing the token.");
                return BadRequest(new { Success = false, Message = "An error occurred. Please try again." });
            }
        }
    }
}
