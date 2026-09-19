using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests;

public class StellarAdminDashboardBuilderExtensionsTests
{
    [Test]
    public async Task AddResource_WhenRepeated_ComposesOverridesInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>(resource =>
        {
            resource.SingularLabel = "Item";
            resource.PluralLabel = "Inventory";
        });

        // Act
        sut.AddResource<Product>(resource => resource.SingularLabel = "Stock item");
        sut.AddResource<Product>();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Stock item");
        await Assert.That(options.PluralLabel).IsEqualTo("Inventory");
    }

    [Test]
    public async Task AddResource_WithAcronym_ResolvesReadableLabels()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<APIKey>();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<APIKey>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("API key");
        await Assert.That(options.PluralLabel).IsEqualTo("API keys");
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddResource_WithBlankPluralLabel_RejectsConfiguration(string value)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>(resource => resource.PluralLabel = value);
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddResource_WithBlankSingularLabel_RejectsConfiguration(string value)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>(resource => resource.SingularLabel = value);
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task AddResource_WithExplicitPlural_IsIndependentOfAssignmentOrder(
        bool pluralFirst
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<Product>(resource =>
        {
            if (pluralFirst)
            {
                resource.PluralLabel = "Inventory";
                resource.SingularLabel = "Stock item";
            }
            else
            {
                resource.SingularLabel = "Stock item";
                resource.PluralLabel = "Inventory";
            }
        });
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Stock item");
        await Assert.That(options.PluralLabel).IsEqualTo("Inventory");
    }

    [Test]
    public async Task AddResource_WithGenericType_ResolvesReadableLabels()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<Product<int>>();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product<int>>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Product");
        await Assert.That(options.PluralLabel).IsEqualTo("Products");
    }

    [Test]
    public async Task AddResource_WithMixedOverloads_SharesConfigurationAndReturnsParent()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        var resource = sut.AddResource<Product>();
        resource.SingularLabel = "Item";

        // Act
        var result = sut.AddResource<Product>(configured =>
        {
            configured.PluralLabel = "Inventory";
        });
        resource.SingularLabel = "Stock item";
        sut.AddResource<Product>();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(result).IsSameReferenceAs(sut);
        await Assert.That(options.SingularLabel).IsEqualTo("Stock item");
        await Assert.That(options.PluralLabel).IsEqualTo("Inventory");
    }

    [Test]
    public async Task AddResource_WithMultipleProviders_CreatesSeparateOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>().SingularLabel = "Item";
        using var firstProvider = services.BuildServiceProvider();
        using var secondProvider = services.BuildServiceProvider();

        // Act
        var first = firstProvider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        var second = secondProvider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(first).IsNotSameReferenceAs(second);
        await Assert.That(first.SingularLabel).IsEqualTo("Item");
        await Assert.That(second.SingularLabel).IsEqualTo("Item");
    }

    [Test]
    public async Task AddResource_WithMultipleResources_KeepsConfigurationSeparate()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<Product>(resource => resource.SingularLabel = "Item");
        sut.AddResource<ProductCategory>();
        using var provider = services.BuildServiceProvider();
        var product = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        var category = provider
            .GetRequiredService<IOptions<ResourceOptions<ProductCategory>>>()
            .Value;

        // Assert
        await Assert.That(product.PluralLabel).IsEqualTo("Items");
        await Assert.That(category.SingularLabel).IsEqualTo("Product category");
        await Assert.That(category.PluralLabel).IsEqualTo("Product categories");
    }

    [Test]
    public async Task AddResource_WithNullCallback_Throws()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        Action act = () => sut.AddResource<Product>(null!);

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddResource_WithOptionsPipeline_AppliesConfigurationAndPostConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>().SingularLabel = "Item";
        services.Configure<ResourceOptions<Product>>(options => options.SingularLabel = "Category");
        services.PostConfigure<ResourceOptions<Product>>(options =>
            options.PluralLabel = "Catalog"
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Category");
        await Assert.That(options.PluralLabel).IsEqualTo("Catalog");
    }

    [Test]
    public async Task AddResource_WithOptionsValidation_RejectsInvalidConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>().SingularLabel = "Item";
        services
            .AddOptions<ResourceOptions<Product>>()
            .Validate(options => options.SingularLabel == "Product", "Expected Product.");
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            _ = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(act).Throws<OptionsValidationException>();
    }

    [Test]
    public async Task AddResource_WithPluralOverride_PreservesInferredSingular()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<Product>(resource => resource.PluralLabel = "Inventory");
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Product");
        await Assert.That(options.PluralLabel).IsEqualTo("Inventory");
    }

    [Test]
    public async Task AddResource_WithReturnedBuilder_ConfiguresRegisteredResource()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        var resource = sut.AddResource<Product>();
        resource.SingularLabel = "Item";
        resource.PluralLabel = "Inventory";
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Item");
        await Assert.That(options.PluralLabel).IsEqualTo("Inventory");
    }

    [Test]
    public async Task AddResource_WithSeparateServiceCollections_KeepsConfigurationSeparate()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        var otherServices = new ServiceCollection();
        otherServices.AddStellarAdmin().AddDashboard().AddResource<Product>();

        // Act
        sut.AddResource<Product>().SingularLabel = "Item";
        using var firstProvider = services.BuildServiceProvider();
        using var secondProvider = otherServices.BuildServiceProvider();
        var first = firstProvider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;
        var second = secondProvider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(first).IsNotSameReferenceAs(second);
        await Assert.That(first.SingularLabel).IsEqualTo("Item");
        await Assert.That(second.SingularLabel).IsEqualTo("Product");
        await Assert.That(second.PluralLabel).IsEqualTo("Products");
    }

    [Test]
    public async Task AddResource_WithSingularOverride_DerivesPlural()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<Product>(resource => resource.SingularLabel = "Person");
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Person");
        await Assert.That(options.PluralLabel).IsEqualTo("People");
    }

    [Test]
    public async Task AddResource_WithoutConfiguration_ResolvesDefaultLabels()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<Product>();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Product");
        await Assert.That(options.PluralLabel).IsEqualTo("Products");
    }

    private sealed class APIKey;

    private sealed class Product;

    private sealed class Product<T>;

    private sealed class ProductCategory;
}
