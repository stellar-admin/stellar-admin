using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;

// A second context over the catalog's tables, which also maps warehouses. Its filters keep products priced 30 or more
// and leave out the Beverage category, so tests can tell which context loaded a page.
public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasKey(product => product.Number);
        modelBuilder.Entity<Product>().OwnsOne(product => product.Details);
        modelBuilder
            .Entity<Product>()
            .HasQueryFilter(product => !product.Hidden && product.Price >= 30);
        modelBuilder.Entity<Product>().Property(product => product.Version).IsConcurrencyToken();
        modelBuilder
            .Entity<Product>()
            .Property(product => product.GeneratedCode)
            .HasDefaultValue(7);
        modelBuilder.Entity<Category>().HasQueryFilter(category => category.Id != 2);
        modelBuilder.Entity<Warehouse>();
    }
}
