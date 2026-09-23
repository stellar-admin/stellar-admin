using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests.Fixtures;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasKey(product => product.Number);
        modelBuilder.Entity<Product>().HasQueryFilter(product => !product.Hidden);
        modelBuilder.Entity<Product>().Property(product => product.Version).IsConcurrencyToken();
        modelBuilder
            .Entity<Product>()
            .Property(product => product.GeneratedCode)
            .HasDefaultValue(7);
    }
}
