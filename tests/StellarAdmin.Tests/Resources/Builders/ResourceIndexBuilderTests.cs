using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceIndexBuilderTests
{
    [Test]
    public async Task AddPartial_AddsThePartialInCallOrderWithItsSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
            {
                index.AddTable("products");
                index.AddPartial(
                    "cards",
                    "_ProductCards",
                    cards =>
                    {
                        cards.Title = "Cards";
                        cards.Icon = "layout-grid";
                        cards.IgnoreSearch();
                    }
                );
                index.AddTable("prices");
            });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.Index.Layouts.Select(layout => layout.Key))
            .IsEquivalentTo(
                ["products", "cards", "prices"],
                TUnit.Assertions.Enums.CollectionOrdering.Matching
            );
        var cards = await Assert
            .That(options.Index.Layouts[1])
            .IsTypeOf<ResourcePartialDataLayoutOptions>();
        await Assert.That(cards!.PartialName).IsEqualTo("_ProductCards");
        await Assert.That(cards.Title).IsEqualTo("Cards");
        await Assert.That(cards.Icon).IsEqualTo("layout-grid");
        await Assert.That(cards.IgnoresSearch).IsTrue();
        await Assert.That(cards.IgnoresScopes).IsFalse();
    }

    [Test]
    public async Task AddPartialWithHandler_AddsThePartialAndRegistersTheHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
            {
                index.AddTable("products");
                index.AddPartial<ProductCount, ProductCountHandler>(
                    "counts",
                    "_ProductCounts",
                    counts => counts.IgnoreScopes()
                );
            });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var counts = await Assert
            .That(options.Index.Layouts[1])
            .IsTypeOf<ResourcePartialDataLayoutOptions>();
        await Assert.That(counts!.Key).IsEqualTo("counts");
        await Assert.That(counts.PartialName).IsEqualTo("_ProductCounts");
        await Assert.That(counts.IgnoresScopes).IsTrue();
        await Assert
            .That(
                services
                    .Single(service => service.ServiceType == typeof(ProductCountHandler))
                    .Lifetime
            )
            .IsEqualTo(ServiceLifetime.Scoped);
    }

    [Test]
    public async Task AddPartialWithHandler_WithRecordKey_KeysItsItems()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
                index.AddPartial<ProductCount, ProductCountHandler>(
                    "counts",
                    "_ProductCounts",
                    counts => counts.HasRecordKey(count => count.Count)
                )
            );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var counts = options.Index.Layouts.Single();
        await Assert.That(counts.RecordKey!(new ProductCount(7))).IsEqualTo("7");
        await Assert.That(counts.Link).IsNull();
    }

    [Test]
    public async Task AddPartialWithHandler_WithLinkAndRecordKey_Throws()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        var act = () =>
            resource.Index(index =>
                index.AddPartial<ProductCount, ProductCountHandler>(
                    "counts",
                    "_ProductCounts",
                    counts =>
                        counts
                            .LinkTo<Product>(count => count.Count)
                            .HasRecordKey(count => count.Count)
                )
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task AddTableWithHandler_AddsATableWithItsColumnsAndRegistersTheHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
            {
                index.AddTable("products");
                index.AddTable<ProductCount, ProductCountHandler>(
                    "counts",
                    counts =>
                    {
                        counts.HasRecordKey(count => count.Count);
                        counts.Columns(columns =>
                            columns.Add(count => count.Count, column => column.Sortable())
                        );
                        counts.DefaultSortByDescending(count => count.Count);
                        counts.EnablePaging(paging => paging.PageSize = 5);
                    }
                );
            });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var counts = await Assert
            .That(options.Index.Layouts[1])
            .IsTypeOf<ResourceTableDataLayoutOptions>();
        await Assert.That(counts!.Key).IsEqualTo("counts");
        await Assert.That(counts.Loader).IsNotNull();
        await Assert.That(counts.RecordKey!(new ProductCount(7))).IsEqualTo("7");
        await Assert.That(counts.Columns.Single().SortField).IsEqualTo(nameof(ProductCount.Count));
        await Assert
            .That(counts.DefaultSort)
            .IsEqualTo(
                new ResourceSort(nameof(ProductCount.Count), ResourceSortDirection.Descending)
            );
        await Assert.That(counts.Paging!.PageSize).IsEqualTo(5);
        await Assert
            .That(options.Index.RecordTables.Select(table => table.Key))
            .IsEquivalentTo(["products"]);
        await Assert
            .That(
                services
                    .Single(service => service.ServiceType == typeof(ProductCountHandler))
                    .Lifetime
            )
            .IsEqualTo(ServiceLifetime.Scoped);
    }

    [Test]
    public async Task AddTableWithHandler_WithRecordKeyAndLink_Throws()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        var act = () =>
            resource.Index(index =>
                index.AddTable<ProductCount, ProductCountHandler>(
                    "counts",
                    counts =>
                        counts
                            .HasRecordKey(count => count.Count)
                            .LinkTo<Product>(count => count.Count)
                )
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task AddPartial_WithKeyATableUses_Throws()
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        var act = () =>
            resource.Index(index =>
            {
                index.AddTable("products");
                index.AddPartial("Products", "_ProductCards");
            });

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddPartial_WithBlankPartialName_Throws(string partialName)
    {
        // Arrange
        var services = new ServiceCollection();
        var resource = services.AddStellarAdmin().AddResource<Product>();

        // Act
        var act = () => resource.Index(index => index.AddPartial("cards", partialName));

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task AddTable_AddsTablesInCallOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddStellarAdmin()
            .AddResource<Product>()
            .Index(index =>
            {
                index.AddTable("products");
                index.AddTable("prices");
            });
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
            .That(options.Index.Layouts.All(layout => layout is ResourceTableDataLayoutOptions))
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
            .Index(index =>
            {
                index.AddTable(
                    "products",
                    table =>
                    {
                        table.Title = "Products";
                        table.Icon = "table";
                        table.Columns(columns => columns.Add(product => product.Name).Sortable());
                        table.DefaultSortBy(product => product.Name);
                    }
                );
                index.AddTable(
                    "prices",
                    table =>
                    {
                        table.Columns(columns => columns.Add(product => product.Price).Sortable());
                        table.DefaultSortByDescending(product => product.Price);
                        table.EnablePaging(paging => paging.PageSize = 50);
                        table.RowActions(actions => actions.Delete = false);
                        table.IgnoreScopes();
                        table.IgnoreSearch();
                    }
                );
            });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var products = (ResourceTableDataLayoutOptions)options.Index.Layouts[0];
        var prices = (ResourceTableDataLayoutOptions)options.Index.Layouts[1];
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
            resource.Index(index =>
            {
                index.AddTable("prices");
                index.AddTable(key);
            });

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
        var act = () => resource.Index(index => index.AddTable(key));

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task Index_WithoutDataLayouts_HasNoDataLayouts()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdmin().AddResource<Product>().Index(index => index.Title = "Products");
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

        public decimal Price { get; set; }
    }

    private sealed record ProductCount(int Count);

    private sealed class ProductCountHandler : IResourceDataLayoutHandler<ProductCount>
    {
        public Task<ResourceListResult<ProductCount>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<ProductCount>([new(0)], 1));
    }
}
