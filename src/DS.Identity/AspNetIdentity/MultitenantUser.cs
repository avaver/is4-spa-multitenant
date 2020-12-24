using Microsoft.AspNetCore.Identity;

namespace DS.Identity.AspNetIdentity
{
    public class MultitenantUser : IdentityUser
    {
        public string TenantName { get; set; }

        public string NormalizedTenantName { get; set; }
    }
}
