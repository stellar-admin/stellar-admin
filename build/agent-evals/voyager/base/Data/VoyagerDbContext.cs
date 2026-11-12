using Microsoft.EntityFrameworkCore;
using Voyager.Models;

namespace Voyager.Data;

public class VoyagerDbContext(DbContextOptions<VoyagerDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite cannot order by decimal, so store totals as double
        modelBuilder.Entity<Booking>().Property(b => b.Total).HasConversion<double>();
    }
}

public static class SeedData
{
    public static void Initialize(VoyagerDbContext db)
    {
        if (!db.Database.EnsureCreated())
        {
            return;
        }

        var customers = new[]
        {
            new Customer { Name = "Ana Ribeiro", Email = "ana@example.com", Country = "Portugal" },
            new Customer { Name = "Lena Fischer", Email = "lena@example.com", Country = "Germany" },
            new Customer { Name = "Tomás García", Email = "tomas@example.com", Country = "Spain" },
            new Customer { Name = "Priya Nair", Email = "priya@example.com", Country = "India" },
        };
        db.Customers.AddRange(customers);

        string[] destinations = ["Lisbon", "Kyoto", "Reykjavík", "Cape Town", "Lima", "Vancouver"];
        for (var i = 0; i < 24; i++)
        {
            db.Bookings.Add(new Booking
            {
                Reference = $"VT-{1000 + i}",
                Customer = customers[i % customers.Length],
                Destination = destinations[i % destinations.Length],
                DepartureDate = DateOnly.FromDateTime(DateTime.Today).AddDays(7 * i),
                Status = (BookingStatus)(i % 3),
                Cabin = (CabinClass)(i % 4),
                Travellers = 1 + i % 4,
                Total = 450m + 125m * i,
            });
        }

        db.SaveChanges();
    }
}
