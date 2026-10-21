using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Tests.Resources.Builders;

public partial class ResourceBuilderTests
{
    [Test]
    public async Task EnableScopes_WithRepeatedIdentifier_LeavesOutLaterScope()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.Index(index =>
            index.EnableScopes(scopes =>
            {
                scopes.Add("all", "All");
                scopes.Add("ALL", "Duplicate");
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert
            .That(options.Index.Scopes!.Items.Select(scope => scope.Title))
            .IsEquivalentTo(["All"]);
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.RepeatedScope);
    }

    [Test]
    public async Task EnableScopes_WithUnknownDefaultScope_HasNoDefaultScope()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.Index(index =>
            index.EnableScopes(scopes =>
            {
                scopes.Add("all", "All");
                scopes.DefaultScope = "missing";
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.Scopes!.DefaultScope).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.UnknownDefaultScope);
    }

    [Test]
    public async Task AllowCreate_WithSeparateProviders_CreatesIndependentResourceConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
                fields.AddSection(
                    "Details",
                    section =>
                        section.AddGroup(outer =>
                            outer.AddGroup(group =>
                                group.Add(product => product.Name).UseEditor<TextInputEditor>()
                            )
                        )
                )
            )
        );
        sut.Index(index => index.Columns(columns => columns.Add(product => product.Name)));
        sut.Index(index => index.EnablePaging().PageSizes = [10, 25]);
        using var first = services.BuildServiceProvider();
        using var second = services.BuildServiceProvider();

        // Act
        var firstOptions = first.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        var secondOptions = second.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(firstOptions).IsNotSameReferenceAs(secondOptions);
        await Assert
            .That(firstOptions.Index.Paging!.PageSizes)
            .IsNotSameReferenceAs(secondOptions.Index.Paging!.PageSizes);
        await Assert
            .That(firstOptions.Index.Columns[0])
            .IsNotSameReferenceAs(secondOptions.Index.Columns[0]);
        await Assert
            .That(firstOptions.Create!.Items[0])
            .IsNotSameReferenceAs(secondOptions.Create!.Items[0]);
        var firstSection = (FormSectionOptions)firstOptions.Create!.Items[0];
        var secondSection = (FormSectionOptions)secondOptions.Create!.Items[0];
        await Assert.That(firstSection.Items[0]).IsNotSameReferenceAs(secondSection.Items[0]);
        await Assert
            .That(firstOptions.Create!.Fields[0])
            .IsNotSameReferenceAs(secondOptions.Create!.Fields[0]);
        await Assert
            .That(firstOptions.Create!.Fields[0].Editor)
            .IsNotSameReferenceAs(secondOptions.Create!.Fields[0].Editor);
    }

    [Test]
    public async Task DefaultSort_WithoutSortableColumn_IgnoresDefaultSort()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.Index(index =>
        {
            index.Columns(columns => columns.Add(product => product.Name));
            index.DefaultSortBy(product => product.Name);
        });
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.DefaultSort).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.UnsortableDefaultSort);
    }

    [Test]
    [Arguments(0, "2,3", 2, "2,3")]
    [Arguments(2, "", 2, "2,10,25,50,100")]
    [Arguments(2, "2,0", 2, "2")]
    [Arguments(2, "2,2", 2, "2")]
    [Arguments(2, "3,4", 2, "2,3,4")]
    public async Task EnablePaging_WithInvalidSizes_AdjustsPaging(
        int pageSize,
        string sizes,
        int expectedPageSize,
        string expectedSizes
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.Index(index =>
            index.EnablePaging(paging =>
            {
                paging.PageSize = pageSize;
                paging.PageSizes = sizes
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToArray();
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Index.Paging!.PageSize).IsEqualTo(expectedPageSize);
        await Assert
            .That(string.Join(",", options.Index.Paging.PageSizes))
            .IsEqualTo(expectedSizes);
        await Assert.That(options.Problems).IsNotEmpty();
    }

    [Test]
    public async Task UseDataSource_WhenAlreadyRegistered_PreservesApplicationLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ProductDataSource>();
        var sut = services.AddStellarAdmin().AddResource<Product>();

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
    [Arguments("create")]
    [Arguments("edit")]
    [Arguments("delete")]
    [Arguments("view")]
    public async Task UseDataSource_WhenConfiguredActionIsUnsupported_TurnsActionOff(string action)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ReadOnlyDataSource>();
        switch (action)
        {
            case "create":
                sut.AllowCreate();
                break;
            case "edit":
                sut.AllowEdit(_ => { });
                break;
            case "delete":
                sut.AllowDelete(_ => { });
                break;
            case "view":
                sut.AllowView(view => view.Fields(fields => fields.Add(product => product.Name)));
                break;
        }
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.Create).IsNull();
        await Assert.That(options.Edit).IsNull();
        await Assert.That(options.Delete).IsNull();
        await Assert.That(options.View).IsNull();
    }

    [Test]
    public async Task UseDataSource_WhenReconfigured_UsesLastSelection()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();

        // Act
        sut.UseDataSource<ReplacementDataSource>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<IResourceDataSource<Product>>();

        // Assert
        await Assert.That(source).IsTypeOf<ReplacementDataSource>();
    }

    [Test]
    public async Task UseKey_WithComputedValue_LeavesKeyUnset()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddResource<Product>();
        sut.UseKey(product => product.Name.ToUpperInvariant());
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.KeySelector).IsNull();
        await Assert
            .That(options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.InvalidSelector);
    }

    public sealed class Product
    {
        public string Name { get; set; } = "";
    }

    public sealed class ProductDataSource : IResourceCrudDataSource<Product>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Product resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceOperationResult> DeleteAsync(
            string id,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());

        public Task<Product?> FindAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult<Product?>(null);

        public Task<ResourceListResult<Product>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Product>([], 0));

        public Task<ResourceOperationResult> UpdateAsync(
            string id,
            Product resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());
    }

    public sealed class ReadOnlyDataSource : IResourceDataSource<Product>
    {
        public Task<ResourceListResult<Product>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Product>([], 0));
    }

    public sealed class ReplacementDataSource : IResourceCrudDataSource<Product>
    {
        public Task<ResourceOperationResult> CreateAsync(
            Product resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.Success());

        public Task<ResourceOperationResult> DeleteAsync(
            string id,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());

        public Task<Product?> FindAsync(string id, CancellationToken cancellationToken) =>
            Task.FromResult<Product?>(null);

        public Task<ResourceListResult<Product>> ListAsync(
            ResourceListRequest request,
            CancellationToken cancellationToken
        ) => Task.FromResult(new ResourceListResult<Product>([], 0));

        public Task<ResourceOperationResult> UpdateAsync(
            string id,
            Product resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());
    }
}
