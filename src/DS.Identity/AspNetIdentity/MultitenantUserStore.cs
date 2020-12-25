
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DS.Identity.AspNetIdentity
{
    public class MultitenantUserStore : UserStore<MultitenantUser>
    {
        public MultitenantUserStore(MultitenantIdentityDbContext context) : base(context)
        {
        }

        public async Task<MultitenantUser> FindByNameAndTenantAsync(string normalizedUserName, string normalizedTenantName, CancellationToken cancellationToken = default)
        {
            return await Context.Set<MultitenantUser>()
                .SingleOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName && u.NormalizedTenantName == normalizedTenantName, cancellationToken);
        }

        public async Task<MultitenantUser> FindByEmailAndTenantAsync(string normalizedEmail, string normalizedTenantName, CancellationToken cancellationToken = default)
        {
            return await Context.Set<MultitenantUser>()
                .SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.NormalizedTenantName == normalizedTenantName, cancellationToken);
        }
    }
}
