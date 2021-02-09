using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DS.Identity.AppIdentity
{
    public class AppPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public AppPrincipalFactory(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(AppUser user)
        {
            var principal = await base.CreateAsync(user);
            if (principal.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(AppClaimTypes.Tenant, user.TenantName, ClaimValueTypes.String));
                identity.AddClaim(new Claim(AppClaimTypes.TenantAdmin, user.IsClinicAdmin.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean));
            }
            return principal;
        }
    }
}