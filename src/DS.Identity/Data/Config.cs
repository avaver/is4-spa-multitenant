// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace DS.Identity.Data
{
    public static class Config
    {
        const string LOCAL_CLIENT_URI = "http://localhost:3100";

        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("dentalsuite.api", "DentalSuite Nexta API")
                {
                    UserClaims = new List<string>
                    {
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Email,
                        JwtClaimTypes.EmailVerified,
                        "tenant"
                    }
                }
            };

        public static IEnumerable<Client> Clients =>
            new [] 
            {
                new Client
                {
                    ClientId = "dentalsuite.web",
                    ClientName = "DentalSuite Nexta Web",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,

                    AllowedCorsOrigins = { LOCAL_CLIENT_URI, "https://localhost:3000" },
                    PostLogoutRedirectUris = { $"{LOCAL_CLIENT_URI}/", "https://localhost:3000/login" },
                    RedirectUris = 
                    { 
                        $"{LOCAL_CLIENT_URI}/authentication/callback",
                        $"{LOCAL_CLIENT_URI}/authentication/silentcallback",
                        "https://localhost:3000/authentication/callback",
                        "https://localhost:3000/authentication/silentcallback"
                    },

                    AllowedScopes = 
                    { 
                        IdentityServerConstants.StandardScopes.OpenId, 
                        IdentityServerConstants.StandardScopes.Profile,
                        "dentalsuite.api"
                    },
                }
            };
    }
}