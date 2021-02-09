
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace DS.Identity.AppIdentity
{
    public class AppUserStore : UserStore<AppUser>
    {
        public AppUserStore(AppIdentityDbContext context) : base(context)
        {
        }

        public async Task<AppUser> FindByNameAndTenantAsync(string normalizedUserName, string normalizedTenantName, CancellationToken cancellationToken = default)
        {
            return await Context.Set<AppUser>()
                .SingleOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName && u.NormalizedTenantName == normalizedTenantName, cancellationToken);
        }

        public async Task<AppUser> FindByEmailAndTenantAsync(string normalizedEmail, string normalizedTenantName, CancellationToken cancellationToken = default)
        {
            return await Context.Set<AppUser>()
                .SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.NormalizedTenantName == normalizedTenantName, cancellationToken);
        }

        public async Task<IList<AppUser>> FindByTenantAsync(string normalizedTenantName, CancellationToken cancellationToken = default)
        {
            return await Context.Set<AppUser>().Where(u => u.NormalizedTenantName == normalizedTenantName)
                .ToListAsync(cancellationToken);
        }
    }
}
