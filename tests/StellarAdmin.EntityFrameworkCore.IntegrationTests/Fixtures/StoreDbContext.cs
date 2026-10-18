using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Fixtures;

// A context with DbSet properties, for the home page's entity suggestions
public sealed class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();
}
