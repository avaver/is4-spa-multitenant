using System;
using DS.Identity.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using DS.Identity.AppIdentity;
using DS.Identity.Services;
using DS.Identity.Services.Hosted;

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
            services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.WriteIndented = true);
            
            services.AddMultitenantIdentity(_configuration);
            services.AddMultitenantIdentityServer(_configuration);
            services.AddKeyAuthCookies();
            services.Configure<IdentityOptions>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredUniqueChars = 1;
                o.Password.RequiredLength = 4;
                o.User.RequireUniqueEmail = false;
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
            services.AddSpaStaticFiles(config => config.RootPath = "../identity-ui/build");

            services.AddScoped<KeyCredentialStore>();
            services.AddScoped<KeyCredentialManager>();
            services.AddSingleton<FidoMetadataService>();
            services.AddHostedService<DatabaseInitService>();
            services.AddHostedService<DataSeedingService>();
            services.AddHostedService<MetadataInitService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
                    spa.UseProxyToSpaDevelopmentServer("http://ds-identity-ui:3001");
                }
            });
        }
    }
}
