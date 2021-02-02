using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DS.Identity.Multitenancy
{
    public class MultitenantPrincipalFactory : UserClaimsPrincipalFactory<MultitenantUser, IdentityRole>
    {
        public MultitenantPrincipalFactory(UserManager<MultitenantUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(MultitenantUser user)
        {
            var principal = await base.CreateAsync(user);
            if (principal.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(MultitenantClaimTypes.Tenant, user.TenantName, ClaimValueTypes.String));
                identity.AddClaim(new Claim(MultitenantClaimTypes.TenantAdmin, user.IsClinicAdmin.ToString().ToLowerInvariant(), ClaimValueTypes.Boolean));
            }
            return principal;
        }
    }
}