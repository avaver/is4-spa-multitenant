﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using DS.Identity.Extensions;
using DS.Identity.Multitenancy;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DS.Identity.Services;

namespace DS.Identity
{
    public class Startup
    {
        private IWebHostEnvironment Environment { get; }
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddCors(options => options
            //     .AddDefaultPolicy(policy => policy
            //         .AllowAnyOrigin()
            //         .AllowAnyMethod()
            //         .AllowAnyHeader()
            //     )
            // );
            services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.WriteIndented = true);
            
            services.AddMultitenantIdentity(_configuration);
            services.AddMultitenantIdentityServer(_configuration);
            services.Configure<IdentityOptions>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredUniqueChars = 1;
                o.Password.RequiredLength = 4;
            });
            services.ConfigureApplicationCookie(o =>
            {
                o.SlidingExpiration = true;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                o.Events.OnRedirectToLogin = context =>
                {
                    context.Response.Redirect("/");
                    return Task.CompletedTask;
                };
            });
                
            services.AddLocalApiAuth();

            services.AddSingleton<KeyMetadataService>();
            services.AddHostedService<MetadataInitService>();

            services.AddSpaStaticFiles(config => config.RootPath = "../identity-ui/build");
        }

        public void Configure(IApplicationBuilder app, UserManager<MultitenantUser> userManager, ILogger<Startup> logger)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Order is important here
            // app.UseCors();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "../identity-ui";
                if (Environment.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3001");
                }
            });

            UpdateIdentityDatabase(app, logger);
            CreateTestTenants(app, logger);
            CreateTestUsers(userManager, logger).GetAwaiter().GetResult();
        }

        private void UpdateIdentityDatabase(IApplicationBuilder app, ILogger logger)
        {
            using var scope = app.ApplicationServices.CreateScope();
            logger.LogInformation("Applying Multitenant AspNet Identity DB migrations");
            scope.ServiceProvider.GetService<MultitenantIdentityDbContext>()?.Database.Migrate();
            logger.LogInformation("Applying Configuration DB migrations");
            scope.ServiceProvider.GetService<ConfigurationDbContext>()?.Database.Migrate();
            logger.LogInformation("Applying Operational DB migrations");
            scope.ServiceProvider.GetService<PersistedGrantDbContext>()?.Database.Migrate();
                
            logger.LogInformation("Seeding IdentityServer data");
            var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            context?.IdentityResources.Recreate(Config.IdentityResources.Select(o => o.ToEntity()), (de, le) => de.Name == le.Name);
            context?.ApiScopes.Recreate(Config.ApiScopes.Select(o => o.ToEntity()), (de, le) => de.Name == le.Name);
            context?.Clients.Recreate(Config.Clients.Select(o => o.ToEntity()), (de, le) => de.ClientId == le.ClientId);
            context?.SaveChanges();
        }

        private void CreateTestTenants(IApplicationBuilder app, ILogger logger)
        {
            logger.LogInformation("Creating tenants");
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetService<MultitenantIdentityDbContext>();
            context?.Tenants.Recreate(Tenants, (de, le) => de.Id == le.Id);
            context?.SaveChanges();
        }

        private async Task CreateTestUsers(UserManager<MultitenantUser> userManager, ILogger<Startup> logger)
        {
            logger.LogInformation("Creating users");
            foreach (var user in Users)
            {
                var dbAdmin = await userManager.FindByIdAsync(user.Id);
                if (dbAdmin != null)
                {
                    logger.LogInformation($"User {user.UserName}@{user.TenantName} already exists, deleting it");
                    await userManager.DeleteAsync(dbAdmin);
                }

                if (!(await userManager.CreateAsync(user, "1111")).Succeeded)
                {
                    logger.LogError($"Failed to create user {user.UserName}@{user.TenantName}");
                }
                else
                {
                    logger.LogInformation($"User {user.UserName}@{user.TenantName} created");
                }
            }
        }

        private static IEnumerable<Tenant> Tenants =>
            new[]
            {
                new Tenant { Id = 1, Name = "happyteeth" },
                new Tenant { Id = 2, Name = "superdent" }
            };
        
        private static IEnumerable<MultitenantUser> Users =>
            new[]
            {
                new MultitenantUser
                {
                    Id = "5B3654F2-28E2-487B-BC6E-939AD2FFB842",
                    UserName = "sd",
                    Email = "sd@superdent.dk",
                    TenantName = "superdent",
                    PhoneNumber = "+380989997755",
                    IsClinicAdmin = false,
                    TenantId = 2
                },
                new MultitenantUser
                {
                    Id = "1071C712-70A1-47A4-A4BE-59B1A9F0D188",
                    UserName = "ht",
                    Email = "ht@happyteeth.dk",
                    TenantName = "happyteeth",
                    PhoneNumber = "+380989997755",
                    IsClinicAdmin = false,
                    TenantId = 1
                },
                new MultitenantUser
                {
                    Id = "772D9024-A03B-4607-8126-393AEFD88351",
                    UserName = "adm",
                    Email = "admin@superdent.com",
                    TenantName = "superdent",
                    PhoneNumber = "+380989997755",
                    IsClinicAdmin = true,
                    TenantId = 2
                },
                new MultitenantUser
                {
                    Id = "48295BA6-5E3C-469C-9E36-5FA4EDD7AE0F",
                    UserName = "adm",
                    Email = "admin@happyteeth.com",
                    TenantName = "happyteeth",
                    PhoneNumber = "+380989997755",
                    IsClinicAdmin = true,
                    TenantId = 1
                }
            };
    }
}
