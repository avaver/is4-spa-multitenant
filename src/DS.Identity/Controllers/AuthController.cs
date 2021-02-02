using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DS.Identity.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
            public string Tenant { get; set; }
            public string ReturnUrl { get; set; }
        }

        public class TenantRequest
        {
            public string ReturnUrl { get; set; }
        }

        private readonly IIdentityServerInteractionService _identity;
        private readonly MultitenantUserManager _userManager;
        private readonly SignInManager<MultitenantUser> _signInManager;
        private readonly MultitenantIdentityDbContext _dbcontext;

        public AuthController(IIdentityServerInteractionService identity, MultitenantUserManager userManager, SignInManager<MultitenantUser> signInManager, MultitenantIdentityDbContext dbcontext)
        {
            _identity = identity;
            _userManager = userManager;
            _signInManager = signInManager;
            _dbcontext = dbcontext;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var context = await _identity.GetAuthorizationContextAsync(request.ReturnUrl);
            var tenant = context != null ? context.Tenant : request.Tenant;
            if (!string.IsNullOrEmpty(tenant))
            {
                var user = await _userManager.FindByNameAndTenantAsync(request.Username, tenant);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
                    if (result.Succeeded)
                    {
                        var tenantInfo = _dbcontext.Tenants.Include(t => t.Keys).Single(t => t.Name == tenant);
                        if (!user.IsClinicAdmin && (tenantInfo.Keys == null || tenantInfo.Keys.Count <= 1))
                        {
                            return Unauthorized();
                        }

                        return new JsonResult(new
                        {
                            RedirectUrl = request.ReturnUrl, 
                            Keys = user.IsClinicAdmin && tenantInfo.AdminKeyId != null 
                                ? new[] { tenantInfo.AdminKeyId } 
                                : tenantInfo.Keys?.Where(k => k.Id != tenantInfo.AdminKeyId).Select(k => k.Id).ToArray()
                        });
                    }
                }
            }

            return Unauthorized();
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
        public async Task<IActionResult> ErrorRedirect([FromQuery(Name = "errorId")] string errorId)
        {
            var error = await _identity.GetErrorContextAsync(errorId);
            var message = error == null ? "unknown error" : error.ErrorDescription;
            return Ok(new { message });
        }
    }
}
