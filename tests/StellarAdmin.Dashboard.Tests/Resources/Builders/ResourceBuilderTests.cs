using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests.Resources.Builders;

public class ResourceBuilderTests
{
    [Test]
    public async Task Index_WhenColumnsAreCleared_ReplacesEarlierColumns()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();
        sut.Index(index => index.Columns(columns => columns.Add(product => product.Id)));

        // Act
        sut.Index(index =>
            index.Columns(columns =>
            {
                columns.Clear();
                columns.Add(product => product.Name).Title = "Product name";
            })
        );
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.Columns.Count).IsEqualTo(1);
        await Assert.That(options.Index.Columns[0].FieldName).IsEqualTo("Name");
        await Assert.That(options.Index.Columns[0].Title).IsEqualTo("Product name");
    }

    [Test]
    public async Task Index_WithSeparateProviders_CreatesIndependentColumnOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();
        sut.Index(index => index.Columns(columns => columns.Add(product => product.Id)));
        using var firstProvider = services.BuildServiceProvider();
        using var secondProvider = services.BuildServiceProvider();

        // Act
        var first = firstProvider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        var second = secondProvider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(first.Index.Columns[0]).IsNotSameReferenceAs(second.Index.Columns[0]);
        await Assert.That(first.Index.Columns).IsNotSameReferenceAs(second.Index.Columns);
    }

    [Test]
    public async Task UseDataSource_WhenAlreadyRegistered_PreservesApplicationLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ProductDataSource>();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();

        // Act
        sut.UseDataSource<ProductDataSource>();
        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateScopes = true }
        );
        using var scope = provider.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<IResourceDataSource<Product>>();

        // Assert
        await Assert
            .That(source)
            .IsSameReferenceAs(provider.GetRequiredService<ProductDataSource>());
    }

    [Test]
    public async Task UseDataSource_WhenReconfigured_UsesLastSelection()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();

        // Act
        sut.UseDataSource<ReplacementDataSource>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<IResourceDataSource<Product>>();

        // Assert
        await Assert.That(source).IsTypeOf<ReplacementDataSource>();
    }

    public sealed record Product(int Id, string Name);

    public sealed class ProductDataSource : IResourceDataSource<Product>
    {
        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>([]);
    }

    public sealed class ReplacementDataSource : IResourceDataSource<Product>
    {
        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>([]);
    }
}
