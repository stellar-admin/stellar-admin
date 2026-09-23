using DashboardPlayground.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardPlayground.Data;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().OwnsOne(product => product.Details);

        // Store this demo's two-decimal prices as cents so SQLite can order them exactly.
        modelBuilder
            .Entity<Product>()
            .Property(product => product.Price)
            .HasConversion(price => (long)(price * 100m), cents => cents / 100m);
    }
}
