using DS.Identity.AspNetIdentity;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DS.Identity.Services
{
    public class MultitenantProfileService : DefaultProfileService
    {
        private readonly MultitenantUserManager _manager;

        public MultitenantProfileService(ILogger<DefaultProfileService> logger, MultitenantUserManager manager) : base(logger)
        {
            _manager = manager;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);

            var user = await _manager.GetUserAsync(context.Subject);
            if (user == null)
            {
                Logger.LogWarning("No user found matching subject Id: {subjectId}", context.Subject.GetSubjectId());
            }
            else
            {
                context.IssuedClaims.Add(new Claim("tenant", user.TenantName));
                Logger.LogInformation("Added tenant claim {0} for user {1}", user.TenantName, user.UserName);
            }
        }
    }
}
