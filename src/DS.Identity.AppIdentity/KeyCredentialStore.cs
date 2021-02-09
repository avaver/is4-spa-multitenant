using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DS.Identity.AppIdentity
{
    public class KeyCredentialStore
    {
        private readonly AppIdentityDbContext _context;
        private readonly ILogger<KeyCredentialStore> _logger;

        public KeyCredentialStore(AppIdentityDbContext context, ILogger<KeyCredentialStore> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAsync(KeyCredential credential, CancellationToken cancellationToken = default)
        {
            await _context.KeyCredentials.AddAsync(credential, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(KeyCredential credential, CancellationToken cancellationToken = default)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            _context.KeyCredentials.Remove(credential);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<KeyCredential> FindByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.KeyCredentials.SingleOrDefaultAsync(key => key.Id == id, cancellationToken);
        }
        
        public async Task<List<KeyCredential>> GetTenantKeysAsync(string normalizedTenantName, CancellationToken cancellationToken = default)
        {
            return await _context.KeyCredentials.Where(key => key.NormalizedTenantName == normalizedTenantName).ToListAsync(cancellationToken);
        }
        
        public async Task<List<KeyCredential>> GetTenantAdminKeysAsync(string normalizedTenantName, CancellationToken cancellationToken = default)
        {
            return await _context.KeyCredentials.Where(key => key.NormalizedTenantName == normalizedTenantName && key.IsAdminKey).ToListAsync(cancellationToken);
        }
        
        public async Task<List<KeyCredential>> GetTenantDeviceKeysAsync(string normalizedTenantName, CancellationToken cancellationToken = default)
        {
            return await _context.KeyCredentials.Where(key => key.NormalizedTenantName == normalizedTenantName && !key.IsAdminKey).ToListAsync(cancellationToken);
        }
    }
}