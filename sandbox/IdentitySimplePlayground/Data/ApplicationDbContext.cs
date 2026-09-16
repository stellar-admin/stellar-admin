using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentitySimplePlayground.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(category =>
        {
            category.Property(entity => entity.Name).IsRequired().HasMaxLength(100);
            category.Property(entity => entity.Description).HasMaxLength(500);
        });

        builder.Entity<Product>(product =>
        {
            product.Property(entity => entity.Name).IsRequired().HasMaxLength(200);
            product.Property(entity => entity.Sku).IsRequired().HasMaxLength(64);
            product.Property(entity => entity.Description).HasMaxLength(4000);

            product.Property(entity => entity.Price).HasPrecision(18, 2);
            product.Property(entity => entity.CompareAtPrice).HasPrecision(18, 2);
            product.Property(entity => entity.CostPrice).HasPrecision(18, 2);
        });

        builder.Entity<ApplicationUser>(ub =>
        {
            ub.HasMany(u => u.Roles).WithMany(r => r.Users).UsingEntity<IdentityUserRole<string>>();
        });
    }
}
