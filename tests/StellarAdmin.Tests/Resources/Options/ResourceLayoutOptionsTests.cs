using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Options;

public class ResourceLayoutOptionsTests
{
    [Test]
    public async Task Fields_OfPartial_IsEmpty()
    {
        // Arrange
        var options = CreateOptions(
            (ResourceLayoutsBuilder<Product> layouts) =>
            {
                layouts.AddTable("products");
                layouts.AddPartial("cards", "_ProductCards");
            }
        );

        // Act
        var fields = options.Index.FindLayout("cards")!.Fields;

        // Assert
        await Assert.That(fields).IsEmpty();
    }

    [Test]
    public async Task Fields_OfTable_ListsItsColumnsFieldExpressions()
    {
        // Arrange
        var options = CreateOptions(index =>
            index.Columns(columns =>
            {
                columns.Add(product => product.Name);
                columns.Add(product => product.Category.Name);
            })
        );
        var table = options.Index.RecordTables.Single();

        // Act
        var fields = table.Fields;

        // Assert
        await Assert
            .That(fields.Select(field => field.ToString()))
            .IsEquivalentTo(["product => product.Name", "product => product.Category.Name"]);
    }

    [Test]
    public async Task FindDefaultSort_HasTheColumnsSelector()
    {
        // Arrange
        var options = CreateOptions(index =>
        {
            index.Columns(columns => columns.Add(product => product.Name).Sortable());
            index.DefaultSortByDescending(product => product.Name);
        });
        var table = options.Index.RecordTables.Single();

        // Act
        var sort = table.FindDefaultSort();

        // Assert
        await Assert.That(sort!.Field).IsEqualTo(nameof(Product.Name));
        await Assert.That(sort.Direction).IsEqualTo(ResourceSortDirection.Descending);
        await Assert.That(sort.Selector?.ToString()).IsEqualTo("product => product.Name");
    }

    [Test]
    public async Task FindSort_ForColumnWithSortExpression_SelectsTheSortExpression()
    {
        // Arrange
        var options = CreateOptions(index =>
            index.Columns(columns =>
                columns.Add(
                    product => product.Id * 2,
                    column =>
                    {
                        column.Title = "Double id";
                        column.Sortable(product => product.Id);
                    }
                )
            )
        );
        var table = options.Index.RecordTables.Single();

        // Act
        var sort = table.FindSort("doubleid", ResourceSortDirection.Ascending);

        // Assert
        await Assert.That(sort!.Field).IsEqualTo("DoubleId");
        await Assert.That(sort.Selector?.ToString()).IsEqualTo("product => product.Id");
    }

    [Test]
    [Arguments(null)]
    [Arguments("Name")]
    [Arguments("Unknown")]
    public async Task FindSort_WithoutSortableColumn_ReturnsNull(string? field)
    {
        // Arrange
        var options = CreateOptions(index =>
            index.Columns(columns => columns.Add(product => product.Name))
        );
        var table = options.Index.RecordTables.Single();

        // Act
        var sort = table.FindSort(field, ResourceSortDirection.Ascending);

        // Assert
        await Assert.That(sort).IsNull();
    }

    private static ResourceOptions<Product> BuildOptions(Action<ResourceBuilder<Product>> configure)
    {
        var services = new ServiceCollection();
        configure(services.AddStellarAdmin().AddResource<Product>());
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
    }

    private static ResourceOptions<Product> CreateOptions(
        Action<ResourceIndexBuilder<Product>> configure
    ) => BuildOptions(resource => resource.Index(configure));

    private static ResourceOptions<Product> CreateOptions(
        Action<ResourceLayoutsBuilder<Product>> layouts
    ) => BuildOptions(resource => resource.Index(_ => { }, layouts));

    public sealed class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
    }

    public sealed class Product
    {
        public Category Category { get; set; } = new();

        public int Id { get; set; }

        public string Name { get; set; } = "";
    }
}
