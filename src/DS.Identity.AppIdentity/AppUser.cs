using Microsoft.AspNetCore.Identity;

namespace DS.Identity.AppIdentity
{
    public class AppUser : IdentityUser
    {
        public string TenantName { get; set; }

        public string NormalizedTenantName { get; set; }

        public bool IsClinicAdmin { get; set; }
    }
}
