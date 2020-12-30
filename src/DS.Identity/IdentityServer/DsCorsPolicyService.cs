using System;
using System.Threading.Tasks;
using IdentityServer4.Services;

namespace DS.Identity.IdentityServer
{
    public class DsCorsPolicyService : ICorsPolicyService
    {
        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            var result = origin.ToLowerInvariant().EndsWith("localhost:3100") ||
                   origin.ToLowerInvariant().EndsWith("dentalsuite.local:3100");

            return Task.FromResult(result);
        }
    }
}