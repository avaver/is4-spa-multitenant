using System;
using DS.Identity.IdentityServer;
using DS.Identity.AppIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authorization;
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
            var dbType = configuration.GetValue<string>("DBTYPE");
            if (dbType == "SQLSERVER")
            {
                services.AddDbContext<AppIdentityDbContext>(o =>
                    o.UseSqlServer(configuration.GetConnectionString(SqlServerConnection),
                        sql => sql.MigrationsAssembly(SqlServerMigrations)));
            }
            else
            {
                services.AddDbContext<AppIdentityDbContext>(o =>
                    o.UseSqlite(configuration.GetConnectionString(SqliteConnection),
                        sql => sql.MigrationsAssembly(SqliteMigrations)));
            }

            services.AddScoped<AppUserStore>();
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppPrincipalFactory>();
            services.AddTransient<IAppUserValidator<AppUser>, AppUserValidator>();
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddUserManager<AppUserManager>()
                .AddUserStore<AppUserStore>()
                .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddMultitenantIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            var dbType = configuration.GetValue<string>("DBTYPE");
            var connectionSqlServer = configuration.GetConnectionString(SqlServerConnection);
            var connectionSqlite = configuration.GetConnectionString(SqliteConnection);
            var builder = services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/login";
                    options.UserInteraction.LogoutUrl = "/logout";
                    options.UserInteraction.ErrorUrl = "/error";
                })
                .AddAspNetIdentity<AppUser>()
                .AddDeveloperSigningCredential();

            if (dbType == "SQLSERVER")
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
            else
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

            services.AddScoped<IAuthorizeInteractionResponseGenerator, DsInteractionResponseGenerator>();
            services.AddSingleton<IRedirectUriValidator, DsRedirectUriValidator>();
            services.AddSingleton<ICorsPolicyService, DsCorsPolicyService>();

            return services;
        }

        public static IServiceCollection AddKeyAuthCookies(this IServiceCollection services)
        {
            services.AddAuthentication()
                .AddCookie(Constants.KeyAuthScheme, o =>
                {
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                    o.Cookie.Name = Constants.KeyAuthScheme;
                })
                .AddCookie(Constants.KeyAuthUserIdScheme, o =>
                {
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(3);
                    o.Cookie.Name = Constants.KeyAuthUserIdScheme;
                });

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
                o.AddPolicy(Constants.ClinicAdminPolicy, b =>
                {
                    b.RequireAuthenticatedUser();
                    b.RequireClaim(AppClaimTypes.Tenant);
                    b.RequireClaim(AppClaimTypes.TenantAdmin, true.ToString().ToLowerInvariant());
                });
            });

            return services;
        }
    }
}
