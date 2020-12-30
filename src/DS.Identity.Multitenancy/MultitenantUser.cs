using Microsoft.AspNetCore.Identity;

namespace DS.Identity.Multitenancy
{
    public class MultitenantUser : IdentityUser
    {
        public string TenantName { get; set; }

        public string NormalizedTenantName { get; set; }
    }
}
