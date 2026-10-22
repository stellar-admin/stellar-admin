using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Options;

public class DependencyCheckTests
{
    [Test]
    public async Task Run_ParentOnTheForm_KeepsDependencyWithItsField()
    {
        // Arrange
        var sut = CreateOptions(fields =>
        {
            fields.Add(trip => trip.CountryId);
            fields.Add(trip => trip.CityId).DependsOn(trip => trip.CountryId);
        });

        // Act
        var city = sut.Create!.Fields.Single(field => field.FieldName == "CityId");

        // Assert
        await Assert.That(sut.Problems).IsEmpty();
        await Assert
            .That(city.Dependencies.Single().Parent)
            .IsSameReferenceAs(sut.Create.Fields.Single(field => field.FieldName == "CountryId"));
    }

    [Test]
    public async Task Run_ParentNotOnTheForm_ReportsAndDropsDependency()
    {
        // Arrange
        var sut = CreateOptions(fields =>
            fields.Add(trip => trip.CityId).DependsOn(trip => trip.CountryId)
        );

        // Act
        var city = sut.Create!.Fields.Single();

        // Assert
        await Assert.That(city.Dependencies).IsEmpty();
        await Assert
            .That(sut.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.UnknownParentField);
    }

    [Test]
    public async Task Run_RepeatedParent_KeepsOneDependency()
    {
        // Arrange
        var sut = CreateOptions(fields =>
        {
            fields.Add(trip => trip.CountryId);
            fields
                .Add(trip => trip.CityId)
                .DependsOn(trip => trip.CountryId)
                .DependsOn(trip => trip.CountryId, country => country.EmptyText = "Not used");
        });

        // Act
        var city = sut.Create!.Fields.Single(field => field.FieldName == "CityId");

        // Assert
        await Assert.That(sut.Problems).IsEmpty();
        await Assert.That(city.Dependencies.Single().EmptyText).IsNull();
    }

    [Test]
    public async Task Run_FieldsDependingOnEachOther_ReportsAndDropsTheDependencyThatClosesTheCycle()
    {
        // Arrange
        var sut = CreateOptions(fields =>
        {
            fields.Add(trip => trip.CountryId).DependsOn(trip => trip.CityId);
            fields.Add(trip => trip.CityId).DependsOn(trip => trip.CountryId);
        });

        // Act
        var dependencies = sut.Create!.Fields.SelectMany(field => field.Dependencies).ToArray();

        // Assert
        await Assert.That(dependencies).Count().IsEqualTo(1);
        await Assert
            .That(sut.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.DependencyCycle);
    }

    private static ResourceOptions<Trip> CreateOptions(
        Action<ResourceFieldsBuilder<Trip>> configureFields
    )
    {
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Trip>();
        resource.UseDataSource<TripDataSource>();
        resource.AllowCreate(create => create.Fields(configureFields));
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<ResourceOptions<Trip>>>().Value;
    }

    public sealed class Trip
    {
        public int? CityId { get; set; }

        public int? CountryId { get; set; }
    }

    public sealed class TripDataSource : IResourceDataSource<Trip>, IResourceCreateHandler<Trip>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Trip model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceListResult<Trip>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Trip>([], 0));
    }
}
