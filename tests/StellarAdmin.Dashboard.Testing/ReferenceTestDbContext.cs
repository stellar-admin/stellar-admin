using System.Collections.Concurrent;
using System.Data.Common;
using IdentitySimplePlayground.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace StellarAdmin.Dashboard.Testing;

public sealed class ReferenceTestDbContext(DbContextOptions<ReferenceTestDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder.Entity<Category>().ToTable("Categories");
    }
}
