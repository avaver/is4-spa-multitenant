using DS.Identity.AspNetIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DS.Identity.Services;

namespace DS.Identity.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMultitenantIdentity(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<MultitenantUserStore>();
            services.AddTransient<IMultitenantUserValidator<MultitenantUser>, MultitenantUserValidator>();
            services.AddDbContext<MultitenantIdentityDbContext>(o => o.UseSqlServer(connectionString));
            services.AddIdentity<MultitenantUser, IdentityRole>()
                .AddEntityFrameworkStores<MultitenantIdentityDbContext>()
                .AddUserManager<MultitenantUserManager>()
                .AddUserStore<MultitenantUserStore>()
                .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddMultitenantIdentityServer(this IServiceCollection services, string connectionString)
        {
            var migrationsAssembly = typeof(DependencyInjection).Assembly.GetName().Name;
            var builder = services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ErrorUrl = "/error";
            })
                .AddAspNetIdentity<MultitenantUser>()
                .AddConfigurationStore(o => o.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddOperationalStore(o => o.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddProfileService<MultitenantProfileService>()
                .AddDeveloperSigningCredential();

            return services;
        }
    }
}
