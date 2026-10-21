using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceAddAnotherBuilderTests
{
    [Test]
    public async Task KeepForNext_WithComputedValue_KeepsNothing()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Tour>();
        resource.UseDataSource<TourDataSource>();
        var sut = resource.AllowCreate().EnableAddAnother();
        sut.KeepForNext(tour => tour.Name.ToUpperInvariant());
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Tour>>>().Value;

        // Assert
        await Assert.That(options.Create!.AddAnother!.KeptFields).IsEmpty();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.UnknownKeptField);
    }

    public sealed class Tour
    {
        public string Name { get; set; } = "";
    }

    public sealed class TourDataSource : IResourceDataSource<Tour>, IResourceCreateHandler<Tour>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Tour model,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceListResult<Tour>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Tour>([], 0));
    }
}
