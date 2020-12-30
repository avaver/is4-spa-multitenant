// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using DS.Identity.Multitenancy;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace DS.Identity
{
    public static class Config
    {
        const string LocalClientUri = "http://dentalsuite.local";

        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope(Constants.DsApiScope, "DentalSuite Nexta API")
                {
                    UserClaims = new List<string>
                    {
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Email,
                        MultitenantClaimTypes.Tenant
                    }
                }
            };

        public static IEnumerable<Client> Clients =>
            new [] 
            {
                new Client
                {
                    ClientId = Constants.DsWebClientId,
                    ClientName = "DentalSuite Nexta Web",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true,

                    AllowedCorsOrigins = {LocalClientUri},
                    PostLogoutRedirectUris = {LocalClientUri},
                    RedirectUris = {LocalClientUri},

                    AllowedScopes = 
                    { 
                        IdentityServerConstants.StandardScopes.OpenId, 
                        IdentityServerConstants.StandardScopes.Profile,
                        Constants.DsApiScope
                    },
                }
            };
    }
}