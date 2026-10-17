using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;
using TUnit.Assertions.Enums;

namespace StellarAdmin.Tests.Resources.Builders;

public class ResourceViewFieldsBuilderTests
{
    [Test]
    public async Task AllowView_WithSectionsAndGroups_BuildsNestedLayout()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowView(view =>
        {
            view.Title = "Product";
            view.SectionLayout = FormSectionLayout.Split;
            view.Fields(fields =>
            {
                fields.Columns(2);
                fields.Add(product => product.Name, field => field.Title = "Title");
                fields.AddSection(
                    "Audit",
                    section =>
                    {
                        section.Description = "Changes to this product.";
                        section.Layout = FormSectionLayout.Stacked;
                        section.Orientation = FieldOrientation.Horizontal;
                        section.Columns(2);
                        section.ColumnSpanFull();
                        section.Add(product => product.ReadOnly);
                        section.AddGroup(group =>
                            group.Add(product => product.Name).EmptyText = "None"
                        );
                    }
                );
            });
        });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        var view = options.View!;
        await Assert.That(view.ModelType).IsEqualTo(typeof(Product));
        await Assert.That(view.Title).IsEqualTo("Product");
        await Assert.That(view.SectionLayout).IsEqualTo(FormSectionLayout.Split);
        await Assert.That(view.Columns.Medium).IsEqualTo(2);
        await Assert.That(((FormDisplayFieldOptions)view.Items[0]).Title).IsEqualTo("Title");
        var section = (FormSectionOptions)view.Items[1];
        await Assert.That(section.Title).IsEqualTo("Audit");
        await Assert.That(section.Description).IsEqualTo("Changes to this product.");
        await Assert.That(section.Layout).IsEqualTo(FormSectionLayout.Stacked);
        await Assert.That(section.Orientation).IsEqualTo(FieldOrientation.Horizontal);
        await Assert.That(section.Columns.Medium).IsEqualTo(2);
        var group = (FormGroupOptions)section.Items[1];
        await Assert.That(((FormDisplayFieldOptions)group.Items[0]).EmptyText).IsEqualTo("None");
        await Assert
            .That(view.Fields.Select(field => field.FieldName))
            .IsEquivalentTo(["Name", "ReadOnly", "Name"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task AllowView_WithFieldOrientation_SetsFieldOrientation()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowView(view =>
            view.Fields(fields =>
                fields.Add(product => product.Name).Orientation = DisplayFieldOrientation.Horizontal
            )
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.View!.Fields[0].Orientation)
            .IsEqualTo(DisplayFieldOrientation.Horizontal);
    }

    [Test]
    public async Task AllowView_WithCustomModel_UsesModelType()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowView<ProductSummary, ProductSummaryHandler>(view =>
            view.Fields(fields => fields.Add(summary => summary.Label))
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.View!.ModelType).IsEqualTo(typeof(ProductSummary));
        await Assert.That(options.View.Fields[0].FieldName).IsEqualTo("Label");
    }

    [Test]
    public async Task AllowView_WithoutFields_RejectsConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowView(view => view.Fields(fields => fields.AddSection("Empty")));
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(act).Throws<OptionsValidationException>();
    }

    [Test]
    public async Task AllowView_WithDataSourceWithoutViewHandler_RejectsConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ListOnlyDataSource>();
        sut.AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(act).Throws<OptionsValidationException>();
    }

    [Test]
    public async Task AllowView_WithHandlerAndListOnlyDataSource_AcceptsConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ListOnlyDataSource>();
        sut.AllowView<ProductSummary, ProductSummaryHandler>(view =>
            view.Fields(fields => fields.Add(summary => summary.Label))
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.View).IsNotNull();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddSection_WithBlankTitle_ThrowsArgumentException(string title)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();

        // Act
        Action act = () => sut.AllowView(view => view.Fields(fields => fields.AddSection(title)));

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    public sealed class Product
    {
        public string Name { get; set; } = "";
        public string ReadOnly => Name;
    }

    public sealed class ProductSummary
    {
        public string Label => "Summary";
    }

    public sealed class ProductSummaryHandler : IResourceViewHandler<ProductSummary>
    {
        public Task<ProductSummary?> FindAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult<ProductSummary?>(new());
    }

    public sealed class ListOnlyDataSource : IResourceDataSource<Product>
    {
        public Task<ResourceListResult<Product>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Product>([], 0));
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
