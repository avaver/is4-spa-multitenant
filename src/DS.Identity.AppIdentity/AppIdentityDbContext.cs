using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DS.Identity.AppIdentity
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        public virtual DbSet<KeyCredential> KeyCredentials { get; set; }
        
        public AppIdentityDbContext()
        {
        }

        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var user = builder.Entity<AppUser>();

            user.Metadata.RemoveIndex(user.HasIndex(u => u.NormalizedUserName).Metadata.Properties);

            user.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256);

            user.Property(u => u.TenantName)
                .IsRequired()
                .HasMaxLength(256);

            user.Property(u => u.IsClinicAdmin)
                .IsRequired()
                .HasDefaultValue(false);

            user.HasIndex(u => new { u.NormalizedUserName, u.NormalizedTenantName }).IsUnique();
            user.HasIndex(u => new { u.NormalizedEmail, u.NormalizedTenantName }).IsUnique();
        }
    }
}
