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
        const string LOCAL_CLIENT_URI = "http://dentalsuite.local:3100";

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
                    UpdateAccessTokenClaimsOnRefresh = true,

                    AllowedCorsOrigins = { LOCAL_CLIENT_URI },
                    PostLogoutRedirectUris = 
                    { 
                        $"{LOCAL_CLIENT_URI}/",
                        "http://happyteeth.dentalsuite.local:3100/",
                        "http://superdent.dentalsuite.local:3100/",
                    },
                    RedirectUris = 
                    { 
                        //$"{LOCAL_CLIENT_URI}/authentication/callback",
                        //$"{LOCAL_CLIENT_URI}/authentication/silentcallback",
                        "http://happyteeth.dentalsuite.local:3100/authentication/callback",
                        "http://happyteeth.dentalsuite.local:3100/authentication/silentcallback",
                        "http://superdent.dentalsuite.local:3100/authentication/callback",
                        "http://superdent.dentalsuite.local:3100/authentication/silentcallback",
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