using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceSummaryBuilderTests
{
    [Test]
    public async Task SeparateItems_WithAddedSeparator_ReportsProblemAndDropsTheSeparator()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
        sut.Summary(summary =>
        {
            summary.SeparateItems = true;
            summary.Add(product => product.Name);
            summary.AddSeparator();
            summary.AddText("Text");
        });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.SeparatorWithSeparateItems);
        await Assert
            .That(options.View!.Summary!.Items.OfType<SummarySeparatorItem>().Any())
            .IsFalse();
        await Assert.That(options.View.Summary.GetRenderedItems().Count).IsEqualTo(3);
    }

    [Test]
    public async Task AddText_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        var act = () => resource.Summary(summary => summary.AddText(null!));

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    public sealed class Product
    {
        public string Name { get; set; } = "";
    }

    public sealed class ProductDataSource
        : IResourceDataSource<Product>,
            IResourceViewHandler<Product>
    {
        public Task<Product?> FindAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult<Product?>(null);

        public Task<ResourceListResult<Product>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Product>([], 0));
    }
}
