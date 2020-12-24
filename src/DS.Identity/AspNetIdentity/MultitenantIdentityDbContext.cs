using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DS.Identity.AspNetIdentity
{
    public class MultitenantIdentityDbContext : IdentityDbContext<MultitenantUser>
    {
        public MultitenantIdentityDbContext(DbContextOptions<MultitenantIdentityDbContext> options) : base(options)
        { 
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var user = builder.Entity<MultitenantUser>();

            user.Metadata.RemoveIndex(user.HasIndex(u => u.NormalizedUserName).Metadata.Properties);

            user.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256);

            user.Property(u => u.TenantName)
                .IsRequired()
                .HasMaxLength(256);

            user.HasIndex(u => new { u.NormalizedUserName, u.NormalizedTenantName }).IsUnique();
            user.HasIndex(u => new { u.NormalizedEmail, u.NormalizedTenantName }).IsUnique();
        }
    }
}
