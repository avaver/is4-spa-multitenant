using System.Linq;
using System.Security.Claims;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DS.Identity.AppIdentity;
using DS.Identity.Extensions;
using DS.Identity.FIDO.WebAuthn.BrowserContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace DS.Identity.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string ReturnUrl { get; set; }
        }

        private readonly IIdentityServerInteractionService _identity;
        private readonly AppUserManager _userManager;
        private readonly KeyCredentialManager _keyManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IIdentityServerInteractionService identity, AppUserManager userManager, SignInManager<AppUser> signInManager, KeyCredentialManager keyManager, ILogger<AuthController> logger)
        {
            _identity = identity;
            _userManager = userManager;
            _signInManager = signInManager;
            _keyManager = keyManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var context = await _identity.GetAuthorizationContextAsync(request.ReturnUrl);
            if (context == null)
            {
                _logger.LogWarning("Cannot create authorization context. Return URL: {0}", request.ReturnUrl);
                return Unauthorized();
            }

            var tenant = context.Tenant ?? Constants.DefaultTenant;
            var user = await _userManager.FindByNameAndTenantAsync(request.Username, tenant);
            if (user == null)
            {
                _logger.LogWarning("User {0} not found in tenant {1}", request.Username, tenant);
                return Unauthorized();
            }

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!passwordCheck.Succeeded)
            {
                _logger.LogWarning("Incorrect password for user {0} in tenant {1}", request.Username, tenant);
                return Unauthorized();
            }

            var securityKeyVerification = await HttpContext.AuthenticateAsync(Constants.KeyAuthScheme);
            if (!securityKeyVerification.Succeeded)
            {
                _logger.LogInformation("No valid security key verification cookie found, requesting key sign in.");
                var keys = user.IsClinicAdmin
                    ? await _keyManager.GetTenantAdminKeysAsync(tenant)
                    : await _keyManager.GetTenantDeviceKeysAsync(tenant);
                if (keys.Count == 0)
                {
                    _logger.LogWarning("No {0} authentication keys found for tenant {1}", user.IsClinicAdmin ? "admin" : "device", tenant);
                    return Unauthorized();
                }
                
                var identity = new ClaimsIdentity(Constants.KeyAuthUserIdScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Id));
                identity.AddClaim(new Claim(ClaimTypes.UserData, request.ReturnUrl));
                await HttpContext.SignInAsync(Constants.KeyAuthUserIdScheme, new ClaimsPrincipal(identity));

                return Ok(new {Keys = keys.Select(k => k.Id)});
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to sign in user {0} to tenant {1}", request.Username, tenant);
                return Unauthorized();
            }

            return Ok(new {request.ReturnUrl});
        }

        [HttpPost]
        public async Task<IActionResult> KeyLogin([FromBody] PublicKeyCredentialAssertion request)
        {
            var userIdCookie = await HttpContext.AuthenticateAsync(Constants.KeyAuthUserIdScheme);
            if (!userIdCookie.Succeeded)
            {
                _logger.LogWarning("User id cookie must be present when signing in with security key");
                return Unauthorized();
            }

            var userId = userIdCookie.Principal.FindFirstValue(ClaimTypes.Name);
            var returnUrl = userIdCookie.Principal.FindFirstValue(ClaimTypes.UserData);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with id {0} not found", userId);
                return Unauthorized();
            }

            var assertion = request.ToCredentialAssertion();
            if (assertion.AuthenticatorData == null)
            {
                return Unauthorized();
            }

            await HttpContext.SignOutAsync(Constants.KeyAuthUserIdScheme);
            var keyIdentity = new ClaimsIdentity(Constants.KeyAuthScheme);
            keyIdentity.AddClaim(new Claim("credential_id", request.Id));
            await HttpContext.SignInAsync(Constants.KeyAuthScheme, new ClaimsPrincipal(keyIdentity));
            await _signInManager.SignInAsync(user, true, "hwk");
            return Ok(new {returnUrl});
        }

        [HttpGet]
        public async Task<IActionResult> Logout([FromQuery] string logoutId)
        {
            var context = await _identity.GetLogoutContextAsync(logoutId);

            if (User?.Identity?.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
            }

            return Ok(new
            {
                logoutId,
                client = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context.ClientName,
                prompt = context?.ShowSignoutPrompt,
                iframeUrl = context?.SignOutIFrameUrl,
                redirectUrl = context?.PostLogoutRedirectUri
            });
        }

        [HttpGet]
        public async Task<IActionResult> ErrorDetails([FromQuery(Name = "errorId")] string errorId)
        {
            var error = await _identity.GetErrorContextAsync(errorId);
            var message = error == null ? "unknown error" : error.ErrorDescription;
            return Ok(new { message });
        }
    }
}
