using System.Security.Claims;
using DS.Identity.AppIdentity;

namespace DS.Identity.Extensions
{
    public static class AspNetIdentityExtensions
    {
        public static string Tenant(this ClaimsPrincipal principal) => principal.FindFirstValue(AppClaimTypes.Tenant);
    }
}