using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace DS.Identity.IdentityServer
{
    public class DsRedirectUriValidator : IRedirectUriValidator
    {
        private readonly IEnumerable<string> _allowedOrigins = new[]
        {
            "localhost:3100",
            "dentalsuite.local:3100"
        };
        
        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            var result = client.ClientId == Constants.DsWebClientId
                ? IsRedirectOriginValid(requestedUri) && IsRedirectPathValid(requestedUri)
                : client.RedirectUris.Contains(requestedUri);

            return Task.FromResult(result);
        }

        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            var result = client.ClientId == Constants.DsWebClientId
                ? IsRedirectOriginValid(requestedUri)
                : client.PostLogoutRedirectUris.Contains(requestedUri);

            return Task.FromResult(result);
        }

        private bool IsRedirectPathValid(string requestedUri)
        {
            return requestedUri.ToLowerInvariant().EndsWith("authentication/callback") ||
                   requestedUri.ToLowerInvariant().EndsWith("authentication/silentcallback");
        }

        private bool IsRedirectOriginValid(string requestedUri)
        {
            var uri = new Uri(requestedUri);
            var origin = uri.GetLeftPart(UriPartial.Authority);
            return _allowedOrigins.Any(o => origin.ToLowerInvariant().EndsWith(o));
        }
    }
}