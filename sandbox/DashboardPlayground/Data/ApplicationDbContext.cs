using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DashboardPlayground.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Department> Departments => Set<Department>();

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
    }
}
