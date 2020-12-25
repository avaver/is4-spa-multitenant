using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using DS.Identity.AspNetIdentity;

namespace DS.Identity.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController : ControllerBase
    {
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string ReturnUrl { get; set; }
        }

        private readonly IIdentityServerInteractionService _identity;
        private readonly MultitenantUserManager _userManager;
        private readonly SignInManager<MultitenantUser> _signInManager;

        public AuthController(IIdentityServerInteractionService identity, MultitenantUserManager userManager, SignInManager<MultitenantUser> signInManager)
        {
            _identity = identity;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var context = await _identity.GetAuthorizationContextAsync(request.ReturnUrl);
            if (context != null)
            {
                var user = await _userManager.FindByNameAndTenantAsync(request.Username, context.Tenant);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, request.Password, true, false);
                    if (result.Succeeded)
                    {
                        return new JsonResult(new { RedirectUrl = request.ReturnUrl, ok = true });
                    }
                }
            }

            return Unauthorized();
        }

        [HttpGet]
        public async Task<IActionResult> Logout([FromQuery] string logoutId)
        {
            var context = await _identity.GetLogoutContextAsync(logoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync();
            }

            return Ok(new
            {
                logoutId,
                client = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context?.ClientName,
                prompt = context.ShowSignoutPrompt,
                iframeUrl = context?.SignOutIFrameUrl,
                redirectUrl = context?.PostLogoutRedirectUri
            });
        }

        [HttpGet]
        public async Task<IActionResult> ErrorRedirect([FromQuery(Name = "errorId")] string errorId)
        {
            var error = await _identity.GetErrorContextAsync(errorId);
            var message = error == null ? "unknown error" : error.ErrorDescription;
            return Ok(new { message });
        }
    }
}
