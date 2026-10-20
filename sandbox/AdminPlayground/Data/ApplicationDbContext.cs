using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdminPlayground.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Department>(department =>
        {
            department.Property(value => value.Name).IsRequired().HasMaxLength(200);
        });

        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(value => value.FirstName).IsRequired().HasMaxLength(100);
            user.Property(value => value.LastName).IsRequired().HasMaxLength(100);
            user.Property(value => value.PreferredLanguage).HasMaxLength(35);
            user.Property(value => value.TimeZoneId).HasMaxLength(35);

            user.HasOne(value => value.Department)
                .WithMany(department => department.Users)
                .HasForeignKey(value => value.DepartmentId);
        });

        builder.Entity<ApplicationRole>(role =>
        {
            role.Property(value => value.Description).HasMaxLength(200);
        });

        builder.Entity<Tag>(tag =>
        {
            tag.Property(value => value.Name).IsRequired().HasMaxLength(50);
            tag.HasIndex(value => value.Name).IsUnique();
            tag.HasData(
                new Tag { Id = 1, Name = "New" },
                new Tag { Id = 2, Name = "Bestseller" },
                new Tag { Id = 3, Name = "Clearance" },
                new Tag { Id = 4, Name = "Eco-friendly" }
            );
        });

        builder.Entity<Product>().OwnsOne(product => product.Details);

        // Store this demo's two-decimal prices as cents so SQLite can order them exactly.
        builder
            .Entity<Product>()
            .Property(product => product.Price)
            .HasConversion(price => (long)(price * 100m), cents => cents / 100m);
    }
}
