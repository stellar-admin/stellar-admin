using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VoyagerTravel.Models;

namespace VoyagerTravel.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Accommodation> Accommodations => Set<Accommodation>();

    public DbSet<Excursion> Excursions => Set<Excursion>();

    public DbSet<Amenity> Amenities => Set<Amenity>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingAddOn> BookingAddOns => Set<BookingAddOn>();

    public DbSet<BookingPassenger> BookingPassengers => Set<BookingPassenger>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<Departure> Departures => Set<Departure>();

    public DbSet<Destination> Destinations => Set<Destination>();

    public DbSet<Guide> Guides => Set<Guide>();

    public DbSet<ItineraryDay> ItineraryDays => Set<ItineraryDay>();

    public DbSet<Language> Languages => Set<Language>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<RoomAllocation> RoomAllocations => Set<RoomAllocation>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();

    public DbSet<Traveller> Travellers => Set<Traveller>();

    public DbSet<Trip> Trips => Set<Trip>();

    public DbSet<TripTag> TripTags => Set<TripTag>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RefreshBookingVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        RefreshBookingVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Trip>().HasOne(trip => trip.LeadGuide).WithMany(guide => guide.LedTrips);
        builder.Entity<Trip>().HasMany(trip => trip.Guides).WithMany(guide => guide.Trips);
        builder.Entity<Trip>().Property(trip => trip.BaggageAllowance).HasConversion(DictionaryConverter, DictionaryComparer);
        builder.Entity<Trip>().Property(trip => trip.GuidesByLanguage).HasConversion(DictionaryConverter, DictionaryComparer);

        // Deleting a record that others still point to fails, unless the others belong to it.
        var foreignKeys = builder.Model.GetEntityTypes()
            .Where(entityType => entityType.ClrType.Namespace == typeof(Trip).Namespace)
            .SelectMany(entityType => entityType.GetForeignKeys());

        foreach (var foreignKey in foreignKeys)
        {
            foreignKey.DeleteBehavior = foreignKey.IsRequired ? DeleteBehavior.Restrict : DeleteBehavior.SetNull;
        }

        builder.Entity<BookingPassenger>().HasOne(passenger => passenger.Booking).WithMany(booking => booking.Passengers).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Payment>().HasOne(payment => payment.Booking).WithMany(booking => booking.Payments).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<BookingAddOn>().HasOne(addOn => addOn.Booking).WithMany(booking => booking.AddOns).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ItineraryDay>().HasOne(day => day.Trip).WithMany(trip => trip.ItineraryDays).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<RoomAllocation>().HasOne(allocation => allocation.Departure).WithMany(departure => departure.RoomAllocations).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Departure>().HasOne(departure => departure.LeadGuide).WithMany(guide => guide.LedDepartures);
    }

    private static readonly ValueConverter<Dictionary<string, int>, string> DictionaryConverter = new(
        dictionary => JsonSerializer.Serialize(dictionary, (JsonSerializerOptions?)null),
        json => JsonSerializer.Deserialize<Dictionary<string, int>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, int>());

    private static readonly ValueComparer<Dictionary<string, int>> DictionaryComparer = new(
        (left, right) => left!.Count == right!.Count && !left.Except(right).Any(),
        dictionary => dictionary.Aggregate(0, (hash, entry) => HashCode.Combine(hash, entry.Key, entry.Value)),
        dictionary => new Dictionary<string, int>(dictionary));

    private void RefreshBookingVersions()
    {
        foreach (var entry in ChangeTracker.Entries<Booking>().Where(entry => entry.State == EntityState.Modified))
        {
            entry.Entity.Version = Guid.NewGuid();
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
