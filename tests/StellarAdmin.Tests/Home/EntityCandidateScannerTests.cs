using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Fakes.Identity;
using StellarAdmin.Home;

namespace StellarAdmin.Tests.Home;

public class EntityCandidateScannerTests
{
    [Test]
    public async Task Candidates_WithContextRegistration_ListsItsDbSetEntityTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptions<TravelContext>());
        var sut = new EntityCandidateScanner(services);

        // Act
        var candidates = sut.Candidates;

        // Assert
        await Assert.That(candidates.Contexts).Count().IsEqualTo(1);
        await Assert.That(candidates.Contexts[0].ContextType).IsEqualTo(typeof(TravelContext));
        await Assert
            .That(candidates.Contexts[0].EntityTypes)
            .IsEquivalentTo([typeof(Booking), typeof(Customer)]);
    }

    [Test]
    public async Task Candidates_WithFrameworkDeclaredDbSets_LeavesThemOut()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptions<TravelIdentityContext>());
        var sut = new EntityCandidateScanner(services);

        // Act
        var candidates = sut.Candidates;

        // Assert
        await Assert.That(candidates.Contexts[0].EntityTypes).IsEquivalentTo([typeof(Booking)]);
    }

    [Test]
    public async Task Candidates_WhenContextIsRegisteredAfterScannerIsCreated_FindsIt()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = new EntityCandidateScanner(services);
        services.AddSingleton(new DbContextOptions<TravelContext>());

        // Act
        var candidates = sut.Candidates;

        // Assert
        await Assert.That(candidates.Contexts).Count().IsEqualTo(1);
    }

    [Test]
    public async Task Candidates_WithoutContexts_ReturnsNone()
    {
        // Arrange
        var sut = new EntityCandidateScanner(new ServiceCollection());

        // Act
        var candidates = sut.Candidates;

        // Assert
        await Assert.That(candidates.Contexts).IsEmpty();
    }

    [Test]
    public async Task Candidates_WithoutEfCorePackage_ReportsItMissing()
    {
        // Arrange
        var sut = new EntityCandidateScanner(new ServiceCollection());

        // Act
        var candidates = sut.Candidates;

        // Assert
        await Assert.That(candidates.HasEfCorePackage).IsFalse();
    }

    public sealed class Booking;

    public sealed class Customer;

    public sealed class TravelContext
    {
        public DbSet<Booking>? Bookings { get; set; }

        public DbSet<Customer>? Customers { get; set; }

        public string? Name { get; set; }
    }

    public sealed class TravelIdentityContext : IdentityContext
    {
        public DbSet<Booking>? Bookings { get; set; }
    }
}
