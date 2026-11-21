using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Options;

public class ResourceOptionsFactoryTests
{
    [Test]
    public async Task Create_WhenIndexIsConfigured_HasOneTableLayout()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
                index.AddTable(
                    "table",
                    table =>
                    {
                        table.Columns(columns => columns.Add(product => product.Name).Sortable());
                        table.DefaultSortBy(product => product.Name);
                        table.EnablePaging();
                    }
                )
            );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var table = await Assert
            .That(options.Index.Layouts.Single())
            .IsTypeOf<ResourceTableDataLayoutOptions>();
        await Assert.That(table!.Key).IsEqualTo("table");
        await Assert.That(table.Columns.Count).IsEqualTo(1);
        await Assert.That(table.DefaultSort!.Field).IsEqualTo(nameof(Product.Name));
        await Assert.That(table.Paging).IsNotNull();
    }

    [Test]
    public async Task Create_WhenIndexIsNotConfigured_HasNoDataLayouts()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdmin().AddResource<Product>();
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.Layouts).IsEmpty();
    }

    public sealed class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
    }
}
