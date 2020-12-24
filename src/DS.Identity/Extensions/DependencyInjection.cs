using DS.Identity.AspNetIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

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
    }
}
