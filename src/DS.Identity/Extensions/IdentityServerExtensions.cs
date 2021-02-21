using System.Linq;
using IdentityServer4.Validation;

namespace DS.Identity.Extensions
{
    public static class IdentityServerExtensions
    {
        public static string GetTenantAcrValue(this ValidatedRequest request)
        {
            var acr = request.Raw["acr_values"]?.Split(" ").FirstOrDefault(a => a.ToLowerInvariant().StartsWith("tenant:"));
            return acr?.ToLowerInvariant().Replace("tenant:", "");
        }
    }
}