using System.Linq;
using System.Threading.Tasks;
using DS.Identity.Multitenancy;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace DS.Identity.IdentityServer
{
    public class DsInteractionResponseGenerator: AuthorizeInteractionResponseGenerator
    {
        public DsInteractionResponseGenerator(ISystemClock clock, ILogger<AuthorizeInteractionResponseGenerator> logger, IConsentService consent, IProfileService profile) : base(clock, logger, consent, profile)
        {
        }

        public override async Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null)
        {
            var result = await base.ProcessInteractionAsync(request, consent);
            if (!result.IsLogin && !result.IsConsent && !result.IsRedirect && !result.IsError)
            {
                var acr = request.Raw["acr_values"]?.Split(" ").FirstOrDefault(a => a.ToLowerInvariant().StartsWith("tenant:"));
                var tenantAcr = acr?.ToLowerInvariant().Replace("tenant:", "");
                var claim = request.Subject?.Claims.SingleOrDefault(c => c.Type.ToLowerInvariant() == MultitenantClaimTypes.Tenant);
                var tenantClaim = claim?.Value.ToLowerInvariant();
                if (!string.IsNullOrEmpty(tenantAcr) && !string.IsNullOrEmpty(tenantClaim))
                {
                    Logger.LogInformation("Validating tenant. Cookie tenant: {0}, acr tenant: {1}", tenantClaim, tenantAcr);
                    return new InteractionResponse {IsLogin = tenantClaim != tenantAcr};
                }
            }
            
            return result;
        }
    }
}