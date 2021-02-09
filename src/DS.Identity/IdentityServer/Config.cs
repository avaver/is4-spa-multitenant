// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using DS.Identity.AppIdentity;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace DS.Identity.IdentityServer
{
    public static class Config
    {
        const string LocalClientUri = "http://dentalsuite.local";

        // Claims available to client via id_token
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new(Constants.DsProfileScope, "DentalSuite User Profile",
           new[]
                    {
                        JwtClaimTypes.Name,
                        AppClaimTypes.Tenant
                    })
            };

        // Claims available to api via access_token
        public static IEnumerable<ApiScope> ApiScopes =>
            new[]
            {
                new ApiScope(Constants.DsApiScope, "DentalSuite Nexta API")
                {
                    UserClaims = new []
                    {
                        JwtClaimTypes.Name,
                        AppClaimTypes.Tenant,
                        AppClaimTypes.TenantAdmin
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
                        Constants.DsProfileScope,
                        Constants.DsApiScope
                    },
                }
            };
    }
}