using System.Security.Claims;
using DS.Identity.AppIdentity;

namespace DS.Identity.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string Tenant(this ClaimsPrincipal principal) => principal.FindFirstValue(AppClaimTypes.Tenant);
    }
}