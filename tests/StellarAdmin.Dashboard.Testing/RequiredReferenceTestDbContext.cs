using System.Collections.Concurrent;
using System.Data.Common;
using IdentitySimplePlayground.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace StellarAdmin.Dashboard.Testing;

public sealed class RequiredReferenceTestDbContext(
    DbContextOptions<RequiredReferenceTestDbContext> options
) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToTable("Products");
        modelBuilder
            .Entity<Product>()
            .HasOne(product => product.Category)
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .IsRequired();
        modelBuilder.Entity<Category>().ToTable("Categories");
    }
}
