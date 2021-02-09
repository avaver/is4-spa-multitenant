using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace DS.Identity.AppIdentity
{
    public class KeyCredentialManager
    {
        private readonly KeyCredentialStore _store;
        private readonly ILogger<KeyCredentialManager> _logger;
        private readonly ILookupNormalizer _normalizer;

        public KeyCredentialManager(KeyCredentialStore store, ILookupNormalizer normalizer, ILogger<KeyCredentialManager> logger)
        {
            _store = store;
            _normalizer = normalizer;
            _logger = logger;
        }

        public async Task CreateAsync(KeyCredential credential)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            credential.NormalizedTenantName = Normalize(credential.TenantName);
            await _store.CreateAsync(credential);
        }

        public async Task DeleteAsync(KeyCredential credential)
        {
            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            await _store.DeleteAsync(credential);
        }

        public async Task<KeyCredential> FindByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await _store.FindByIdAsync(id);
        }

        public async Task<List<KeyCredential>> GetTenantKeysAsync(string tenantName)
        {
            if (tenantName == null)
            {
                throw new ArgumentNullException(nameof(tenantName));
            }

            return await _store.GetTenantKeysAsync(Normalize(tenantName));
        }

        public async Task<List<KeyCredential>> GetTenantAdminKeysAsync(string tenantName)
        {
            if (tenantName == null)
            {
                throw new ArgumentNullException(nameof(tenantName));
            }
            
            return await _store.GetTenantAdminKeysAsync(Normalize(tenantName));
        }
        
        public async Task<List<KeyCredential>> GetTenantDeviceKeysAsync(string tenantName)
        {
            if (tenantName == null)
            {
                throw new ArgumentNullException(nameof(tenantName));
            }
            
            return await _store.GetTenantDeviceKeysAsync(Normalize(tenantName));
        }

        private string Normalize(string s)
        {
            return _normalizer == null ? s : _normalizer.NormalizeName(s);
        }
    }
}