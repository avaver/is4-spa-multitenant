using Microsoft.AspNetCore.Identity;

namespace DS.Identity.Multitenancy
{
    public class MultitenantUser : IdentityUser
    {
        public string TenantName { get; set; }

        public string NormalizedTenantName { get; set; }
        
        public bool IsClinicAdmin { get; set; }
        
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
