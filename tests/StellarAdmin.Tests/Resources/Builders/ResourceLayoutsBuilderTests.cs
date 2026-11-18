using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceLayoutsBuilderTests
{
    [Test]
    public async Task AddTable_AddsTablesInCallOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(
                _ => { },
                layouts =>
                {
                    layouts.AddTable("products");
                    layouts.AddTable("prices");
                }
            );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.Index.Layouts.Select(layout => layout.Key))
            .IsEquivalentTo(
                ["products", "prices"],
                TUnit.Assertions.Enums.CollectionOrdering.Matching
            );
        await Assert
            .That(options.Index.Layouts.All(layout => layout is ResourceTableLayoutOptions))
            .IsTrue();
    }

    [Test]
    public async Task AddTable_ConfiguresOnlyItsOwnTable()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(
                _ => { },
                layouts =>
                {
                    layouts.AddTable(
                        "products",
                        table =>
                        {
                            table.Title = "Products";
                            table.Icon = "table";
                            table.Columns(columns =>
                                columns.Add(product => product.Name).Sortable()
                            );
                            table.DefaultSortBy(product => product.Name);
                        }
                    );
                    layouts.AddTable(
                        "prices",
                        table =>
                        {
                            table.Columns(columns =>
                                columns.Add(product => product.Price).Sortable()
                            );
                            table.DefaultSortByDescending(product => product.Price);
                            table.EnablePaging(paging => paging.PageSize = 50);
                            table.RowActions(actions => actions.Delete = false);
                            table.IgnoreScopes();
                            table.IgnoreSearch();
                        }
                    );
                }
            );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var products = (ResourceTableLayoutOptions)options.Index.Layouts[0];
        var prices = (ResourceTableLayoutOptions)options.Index.Layouts[1];
        await Assert.That(products.Title).IsEqualTo("Products");
        await Assert.That(products.Icon).IsEqualTo("table");
        await Assert.That(products.Columns.Single().FieldName).IsEqualTo(nameof(Product.Name));
        await Assert.That(products.DefaultSort!.Field).IsEqualTo(nameof(Product.Name));
        await Assert.That(products.Paging).IsNull();
        await Assert.That(products.RowActions.Delete).IsTrue();
        await Assert.That(products.IgnoresScopes || products.IgnoresSearch).IsFalse();
        await Assert.That(prices.Title).IsNull();
        await Assert.That(prices.Columns.Single().FieldName).IsEqualTo(nameof(Product.Price));
        await Assert
            .That(prices.DefaultSort!.Direction)
            .IsEqualTo(ResourceSortDirection.Descending);
        await Assert.That(prices.Paging!.PageSize).IsEqualTo(50);
        await Assert.That(prices.RowActions.Delete).IsFalse();
        await Assert.That(prices.RowActions.Edit && prices.RowActions.View).IsTrue();
        await Assert.That(prices.IgnoresScopes && prices.IgnoresSearch).IsTrue();
    }

    [Test]
    [Arguments("prices")]
    [Arguments("PRICES")]
    public async Task AddTable_WithKeyAlreadyUsed_Throws(string key)
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        var act = () =>
            resource.Index(
                _ => { },
                layouts =>
                {
                    layouts.AddTable("prices");
                    layouts.AddTable(key);
                }
            );

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddTable_WithBlankKey_Throws(string key)
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        var act = () => resource.Index(_ => { }, layouts => layouts.AddTable(key));

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Index_WithNoLayouts_HasTheImplicitTable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdmin().AddResource<Product>().Index(_ => { }, _ => { });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.Index.Layouts.Single().Key)
            .IsEqualTo(ResourceIndexOptions.ImplicitTableKey);
    }

    public sealed class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public decimal Price { get; set; }
    }
}
