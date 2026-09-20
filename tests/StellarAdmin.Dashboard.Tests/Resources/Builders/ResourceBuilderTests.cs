using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests.Resources.Builders;

public class ResourceBuilderTests
{
    [Test]
    public async Task AllowCreate_WithSeparateProviders_CreatesIndependentResourceConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();
        sut.UseDataSource<ProductDataSource>();
        sut.AllowCreate(create =>
            create.Fields(fields =>
                fields.AddSection(
                    "Details",
                    section =>
                        section.AddRow(row =>
                            row.AddGroup(group => group.Add(product => product.Name))
                        )
                )
            )
        );
        sut.Index(index => index.Columns(columns => columns.Add(product => product.Name)));
        using var first = services.BuildServiceProvider();
        using var second = services.BuildServiceProvider();

        // Act
        var firstOptions = first.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        var secondOptions = second.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(firstOptions).IsNotSameReferenceAs(secondOptions);
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
    [Arguments("create")]
    [Arguments("edit")]
    [Arguments("delete")]
    public async Task UseDataSource_WhenConfiguredActionIsUnsupported_RejectsConfiguration(
        string action
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();
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
        }
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(act).Throws<OptionsValidationException>();
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

    [Test]
    public async Task UseKey_WithComputedValue_RejectsSelector()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard().AddResource<Product>();

        // Act
        Action act = () => sut.UseKey(product => product.Name.ToUpperInvariant());

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
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

        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>([]);

        public Task<ResourceOperationResult> UpdateAsync(
            string id,
            Product resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());
    }

    public sealed class ReadOnlyDataSource : IResourceDataSource<Product>
    {
        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>([]);
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

        public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>([]);

        public Task<ResourceOperationResult> UpdateAsync(
            string id,
            Product resource,
            CancellationToken cancellationToken
        ) => Task.FromResult(ResourceOperationResult.NotFound());
    }
}
