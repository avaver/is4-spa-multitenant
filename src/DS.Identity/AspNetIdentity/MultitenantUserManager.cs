using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DS.Identity.AspNetIdentity
{
    public class MultitenantUserManager : UserManager<MultitenantUser>
    {
        private readonly MultitenantUserStore _store;
        private readonly IServiceProvider _services;

        public MultitenantUserManager(
            MultitenantUserStore store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<MultitenantUser> passwordHasher,
            IEnumerable<IMultitenantUserValidator<MultitenantUser>> userValidators,
            IEnumerable<IPasswordValidator<MultitenantUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<MultitenantUserManager> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _store = store;
            _services = services;
        }

        public override Task<IdentityResult> CreateAsync(MultitenantUser user)
        {
            user.NormalizedTenantName = NormalizeName(user.TenantName);
            return base.CreateAsync(user);
        }

        protected override Task<IdentityResult> UpdateUserAsync(MultitenantUser user)
        {
            user.NormalizedTenantName = NormalizeName(user.TenantName);
            return base.UpdateUserAsync(user);
        }

        public override Task<MultitenantUser> FindByNameAsync(string userName)
        {
            throw new InvalidOperationException("Cannot search only by username in multitenant user store. Use FindByNameAndTenantAsync method instead.");
        }

        public override Task<MultitenantUser> FindByEmailAsync(string email)
        {
            throw new InvalidOperationException("Cannot search only by email in multitenant user store. Use FindByEmailAndTenantAsync method instead.");
        }

        public async Task<MultitenantUser> FindByNameAndTenantAsync(string userName, string tenantName)
        {
            ThrowIfDisposed();
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (tenantName == null)
            {
                throw new ArgumentNullException(nameof(tenantName));
            }

            userName = NormalizeName(userName);
            tenantName = NormalizeName(tenantName);

            var user = await _store.FindByNameAndTenantAsync(userName, tenantName);
            // Need to potentially check all keys
            if (user == null && Options.Stores.ProtectPersonalData)
            {
                var keyRing = _services.GetService<ILookupProtectorKeyRing>();
                var protector = _services.GetService<ILookupProtector>();
                if (keyRing != null && protector != null)
                {
                    foreach (var key in keyRing.GetAllKeyIds())
                    {
                        var oldKey = protector.Protect(key, userName);
                        user = await _store.FindByNameAndTenantAsync(oldKey, tenantName, CancellationToken);
                        if (user != null)
                        {
                            return user;
                        }
                    }
                }
            }

            return user;
        }

        public async Task<MultitenantUser> FindByEmailAndTenantAsync(string email, string tenantName)
        {
            ThrowIfDisposed();
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            if (tenantName == null)
            {
                throw new ArgumentNullException(nameof(tenantName));
            }

            email = NormalizeEmail(email);
            tenantName = NormalizeName(tenantName);

            var user = await _store.FindByEmailAndTenantAsync(email, tenantName, CancellationToken);

            // Need to potentially check all keys
            if (user == null && Options.Stores.ProtectPersonalData)
            {
                var keyRing = _services.GetService<ILookupProtectorKeyRing>();
                var protector = _services.GetService<ILookupProtector>();
                if (keyRing != null && protector != null)
                {
                    foreach (var key in keyRing.GetAllKeyIds())
                    {
                        var oldKey = protector.Protect(key, email);
                        user = await _store.FindByEmailAndTenantAsync(oldKey, tenantName, CancellationToken);
                        if (user != null)
                        {
                            return user;
                        }
                    }
                }
            }

            return user;
        }
    }
}
