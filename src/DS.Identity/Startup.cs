// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using DS.Identity.AspNetIdentity;
using DS.Identity.Data;
using DS.Identity.Extensions;
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
using System.Reflection;
using System.Threading.Tasks;

namespace DS.Identity
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        private readonly string connectionString;

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            connectionString = configuration.GetConnectionString("DSIdentity");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options
                .AddDefaultPolicy(policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                )
            );
            services.AddControllers();
            services.AddSpaStaticFiles(config => config.RootPath = "../identity-ui/build");

            services.AddMultitenantIdentity(connectionString);

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var builder = services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ErrorUrl = "/api/errordetails";
            })
                .AddAspNetIdentity<MultitenantUser>()
                .AddConfigurationStore(o => o.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddOperationalStore(o => o.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddDeveloperSigningCredential();

        }

        public void Configure(IApplicationBuilder app, UserManager<MultitenantUser> userManager, ILogger<Startup> logger)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();
            app.UseRouting();
            app.UseIdentityServer();
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

            UpdateIdentityDatabase(app);
            CreateUsers(userManager, logger).GetAwaiter().GetResult();
        }

        private void UpdateIdentityDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                scope.ServiceProvider.GetService<MultitenantIdentityDbContext>().Database.Migrate();
                scope.ServiceProvider.GetService<ConfigurationDbContext>().Database.Migrate();
                scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
                
                var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                context.IdentityResources.Recreate(Config.IdentityResources.Select(o => o.ToEntity()), (de, le) => de.Name == le.Name);
                context.ApiScopes.Recreate(Config.ApiScopes.Select(o => o.ToEntity()), (de, le) => de.Name == le.Name);
                context.Clients.Recreate(Config.Clients.Select(o => o.ToEntity()), (de, le) => de.ClientId == le.ClientId);
                context.SaveChanges();
            }
        }

        private async Task CreateUsers(UserManager<MultitenantUser> userManager, ILogger<Startup> logger)
        {
            foreach (var user in Users)
            {
                var dbAdmin = await userManager.FindByIdAsync(user.Id);
                if (dbAdmin != null)
                {
                    logger.LogInformation($"user {user.UserName}:{user.TenantName} already exists, deleting it");
                    await userManager.DeleteAsync(dbAdmin);
                }

                if (!(await userManager.CreateAsync(user, "Aa!234")).Succeeded)
                {
                    logger.LogError($"failed to create user {user.UserName}:{user.TenantName}");
                }
                else
                {
                    logger.LogInformation($"user {user.UserName}:{user.TenantName} created");
                }
            }
        }

        private static IEnumerable<MultitenantUser> Users =>
            new MultitenantUser[]
            {
                new MultitenantUser
                {
                    Id = "5B3654F2-28E2-487B-BC6E-939AD2FFB842",
                    UserName = "admin",
                    Email = "avaver@gmail.com",
                    TenantName = "superdent",
                },
                new MultitenantUser
                {
                    Id = "1071C712-70A1-47A4-A4BE-59B1A9F0D188",
                    UserName = "admin",
                    Email = "avaver@gmail.com",
                    TenantName = "happyteeth",
                }
            };
    }
}
