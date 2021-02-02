using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using DS.Identity.IdentityServer;
using DS.Identity.Multitenancy;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DS.Identity.Extensions
{
    public static class DependencyInjection
    {
        private const string SqliteConnection = "DSIdentitySqlite";
        private const string SqliteMigrations = "DS.Identity.Migrations.Sqlite";
        private const string SqlServerConnection = "DSIdentitySqlServer";
        private const string SqlServerMigrations = "DS.Identity.Migrations.SqlServer";

        public static IServiceCollection AddMultitenantIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var platform = configuration.GetValue<string>("platform");
            if (platform == "mac" || string.IsNullOrEmpty(platform) && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                services.AddDbContext<MultitenantIdentityDbContext>(o =>
                    o.UseSqlite(configuration.GetConnectionString(SqliteConnection),
                        sql => sql.MigrationsAssembly(SqliteMigrations)));
            }
            else if (platform == "win" || string.IsNullOrEmpty(platform) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddDbContext<MultitenantIdentityDbContext>(o =>
                    o.UseSqlServer(configuration.GetConnectionString(SqlServerConnection),
                        sql => sql.MigrationsAssembly(SqlServerMigrations)));
            }

            services.AddScoped<MultitenantUserStore>();
            services.AddScoped<IUserClaimsPrincipalFactory<MultitenantUser>, MultitenantPrincipalFactory>();
            services.AddTransient<IMultitenantUserValidator<MultitenantUser>, MultitenantUserValidator>();
            services.AddIdentity<MultitenantUser, IdentityRole>()
                .AddEntityFrameworkStores<MultitenantIdentityDbContext>()
                .AddUserManager<MultitenantUserManager>()
                .AddUserStore<MultitenantUserStore>()
                .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddMultitenantIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            var platform = configuration.GetValue<string>("platform");
            var connectionSqlServer = configuration.GetConnectionString(SqlServerConnection);
            var connectionSqlite = configuration.GetConnectionString(SqliteConnection);
            var builder = services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/login";
                    options.UserInteraction.LogoutUrl = "/logout";
                    options.UserInteraction.ErrorUrl = "/error";
                })
                .AddAspNetIdentity<MultitenantUser>()
                .AddDeveloperSigningCredential();

            if (platform == "mac" || string.IsNullOrEmpty(platform) && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                builder
                    .AddConfigurationStore(o => o.ConfigureDbContext = b => b
                        .UseSqlite(connectionSqlite, sql => sql
                            .MigrationsAssembly(SqliteMigrations)
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)))
                    .AddOperationalStore(o => o.ConfigureDbContext = b => b
                        .UseSqlite(connectionSqlite, sql => sql
                            .MigrationsAssembly(SqliteMigrations)
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            }
            else if (platform == "win" || string.IsNullOrEmpty(platform) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder
                    .AddConfigurationStore(o => o.ConfigureDbContext = b => b
                        .UseSqlServer(connectionSqlServer, sql => sql
                            .MigrationsAssembly(SqlServerMigrations)
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)))
                    .AddOperationalStore(o => o.ConfigureDbContext = b => b
                        .UseSqlServer(connectionSqlServer, sql => sql
                            .MigrationsAssembly(SqlServerMigrations)
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            }

            services.AddScoped<IAuthorizeInteractionResponseGenerator, DsInteractionResponseGenerator>();
            services.AddSingleton<IRedirectUriValidator, DsRedirectUriValidator>();
            services.AddSingleton<ICorsPolicyService, DsCorsPolicyService>();

            return services;
        }

        public static IServiceCollection AddLocalApiAuth(this IServiceCollection services)
        {
            // For authenticating to account management controllers
            services.AddAuthorization(o =>
            {
                o.DefaultPolicy =
                    new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                o.AddPolicy(Constants.DsClinicAdminPolicy, b =>
                {
                    b.RequireAuthenticatedUser();
                    b.RequireClaim(MultitenantClaimTypes.Tenant);
                    b.RequireClaim(MultitenantClaimTypes.TenantAdmin, true.ToString().ToLowerInvariant());
                });
            });

            return services;
        }
    }
}
