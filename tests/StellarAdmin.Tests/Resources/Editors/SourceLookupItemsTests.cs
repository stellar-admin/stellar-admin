using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Tests.Resources.Editors;

public class SourceLookupItemsTests
{
    [Test]
    public async Task Create_WithBoxedArgument_ReadsArgumentType()
    {
        // Arrange
        Expression<Func<Trip, object?>> argument = trip => trip.CountryId;

        // Act
        var sut = SourceLookupItems.Create(typeof(CountryAirportSource), argument, "LookupSheetEditor");

        // Assert
        await Assert.That(sut.ArgumentType).IsEqualTo(typeof(int?));
        await Assert.That(sut.Shape.ArgumentType).IsEqualTo(typeof(int));
    }

    [Test]
    public async Task Create_WithOverriddenMethods_ReadsDescriptionAndMedia()
    {
        // Arrange
        var sourceType = typeof(CodedAirportSource);

        // Act
        var sut = SourceLookupItems.Create<Trip>(sourceType, null, "LookupSheetEditor");

        // Assert
        await Assert.That(sut.HasDescription).IsTrue();
        await Assert.That(sut.ItemType).IsEqualTo(typeof(Airport));
        await Assert.That(sut.MediaType).IsEqualTo(typeof(ItemMedia.Code));
        await Assert.That(sut.ValueType).IsEqualTo(typeof(string));
        await Assert.That(sut.ArgumentType).IsNull();
    }

    [Test]
    public async Task SearchAsync_WithoutArgumentValue_Throws()
    {
        // Arrange
        await using var services = new ServiceCollection().BuildServiceProvider();
        var sut = SourceLookupItems.Create<Trip>(
            typeof(CountryAirportSource),
            trip => trip.CountryId,
            "LookupSheetEditor"
        );
        var query = new LookupQuery(null, 0, 10) { Model = new Trip() };

        // Act
        Func<Task> act = () => sut.SearchAsync(services, query, CancellationToken.None);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    public sealed record Airport(string Code, string Name, string Country, int CountryId);

    public sealed class Trip
    {
        public int? CountryId { get; set; }
    }

    public sealed class CodedAirportSource : LookupSource<Airport, string>
    {
        public static readonly Airport[] Airports =
        [
            new("LIS", "Lisbon", "Portugal", 1),
            new("CPT", "Cape Town", "South Africa", 2),
        ];

        public override Task<IReadOnlyCollection<Airport>> FindAsync(
            IReadOnlyCollection<string> values,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult<IReadOnlyCollection<Airport>>(
                Airports.Where(airport => values.Contains(airport.Code)).ToArray()
            );

        public override string? GetCode(Airport item) => item.Code;

        public override string? GetDescription(Airport item) => item.Country;

        public override string GetTitle(Airport item) => item.Name;

        public override string GetValue(Airport item) => item.Code;

        public override Task<LookupPage<Airport>> SearchAsync(
            LookupQuery query,
            CancellationToken cancellationToken
        ) => Task.FromResult(new LookupPage<Airport>(Airports, false));
    }

    public sealed class CountryAirportSource : LookupSource<Airport, string, int>
    {
        public override Task<IReadOnlyCollection<Airport>> FindAsync(
            IReadOnlyCollection<string> values,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyCollection<Airport>>([]);

        public override string GetTitle(Airport item) => item.Name;

        public override string GetValue(Airport item) => item.Code;

        public override Task<LookupPage<Airport>> SearchAsync(
            int argument,
            LookupQuery query,
            CancellationToken cancellationToken
        ) =>
            Task.FromResult(
                new LookupPage<Airport>(
                    CodedAirportSource
                        .Airports.Where(airport => airport.CountryId == argument)
                        .ToArray(),
                    false
                )
            );
    }
}
