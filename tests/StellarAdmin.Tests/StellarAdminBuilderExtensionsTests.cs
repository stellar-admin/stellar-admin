using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Options;
using StellarAdmin.Sidebar;

namespace StellarAdmin.Tests;

public class StellarAdminBuilderExtensionsTests
{
    [Test]
    public async Task AddResource_WhenOptionsThrow_TurnsResourceOff()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        sut.AddResource<Product>();
        services.Configure<ResourceOptions<Product>>(_ =>
            throw new InvalidOperationException("Broken")
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.IsOff).IsTrue();
        await Assert
            .That(options.Problems.Single().Message)
            .Contains("InvalidOperationException: Broken");
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddResource_WithBlankPluralLabel_UsesDefaultLabel(string value)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        sut.AddResource<Product>(resource => resource.PluralLabel = value);
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.PluralLabel).IsEqualTo("Products");
        await Assert
            .That(options.Problems.Select(problem => problem.Kind))
            .IsEquivalentTo([ConfigurationProblemKind.BlankLabel]);
    }

    [Test]
    [Arguments("", "Label")]
    [Arguments(" ", "Label")]
    [Arguments("", "Group")]
    [Arguments(" ", "Group")]
    [Arguments("", "Icon")]
    [Arguments(" ", "Icon")]
    public async Task AddResource_WithBlankSidebarText_UsesDefault(string value, string property)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        sut.AddResource<Product>(resource =>
            resource.SidebarItem(item =>
            {
                switch (property)
                {
                    case "Label":
                        item.Label = value;
                        break;
                    case "Group":
                        item.Group = value;
                        break;
                    default:
                        item.Icon = value;
                        break;
                }
            })
        );
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SidebarItem.Label).IsNull();
        await Assert.That(options.SidebarItem.Group).IsEqualTo("Resources");
        await Assert.That(options.SidebarItem.Icon).IsNull();
        await Assert
            .That(options.Problems.Select(problem => problem.Kind))
            .IsEquivalentTo([ConfigurationProblemKind.BlankLabel]);
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    public async Task AddResource_WithBlankSingularLabel_UsesDefaultLabel(string value)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        sut.AddResource<Product>(resource => resource.SingularLabel = value);
        using var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<ResourceOptions<Product>>>().Value;

        // Assert
        await Assert.That(options.SingularLabel).IsEqualTo("Product");
        await Assert
            .That(options.Problems.Select(problem => problem.Kind))
            .IsEquivalentTo([ConfigurationProblemKind.BlankLabel]);
    }

    [Test]
    public async Task AddResource_WithCustomSidebarProvider_PreservesCustomItems()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISidebarItemsProvider>(new CustomSidebarProvider());
        var sut = services.AddStellarAdmin();

        // Act
        sut.AddResource<Product>();
        using var provider = services.BuildServiceProvider();
        var items = new List<SidebarItem>();
        foreach (var sidebarProvider in provider.GetServices<ISidebarItemsProvider>())
        {
            items.AddRange(
                await sidebarProvider.GetItemsAsync(
                    new DefaultHttpContext(),
                    CancellationToken.None
                )
            );
        }

        // Assert
        await Assert.That(items.Count).IsEqualTo(2);
        await Assert.That(items[0]).IsTypeOf<SidebarLinkItem>();
        await Assert.That(items[1]).IsTypeOf<SidebarGroupItem>();
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments("Products")]
    [Arguments("order_items")]
    [Arguments("order--items")]
    [Arguments("-products")]
    [Arguments("catalog/products")]
    public async Task AddResource_WithInvalidSlug_TurnsResourceOff(string slug)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();

        // Act
        sut.AddResource<Product>(slug);

        // Assert
        var problem = sut.Options.Problems.Single();
        await Assert.That(services.Any(IsRegistration)).IsFalse();
        await Assert.That(problem.Kind).IsEqualTo(ConfigurationProblemKind.InvalidSlug);
        await Assert.That(problem.Effect).IsEqualTo(ConfigurationProblemEffect.ResourceOff);
    }

    [Test]
    public async Task AddResource_WithRepeatedAndDistinctResources_RegistersOneSidebarProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();

        // Act
        sut.AddResource<Product>();
        sut.AddResource<InventoryItem>(resource => resource.PluralLabel = "Inventory");
        sut.AddResource<Product>();
        using var provider = services.BuildServiceProvider();
        var sidebarProviders = provider.GetServices<ISidebarItemsProvider>().ToArray();
        var links = (
            await sidebarProviders
                .Single()
                .GetItemsAsync(new DefaultHttpContext(), CancellationToken.None)
        )
            .Cast<SidebarGroupItem>()
            .SelectMany(group => group.Items)
            .ToArray();

        // Assert
        await Assert.That(sidebarProviders.Length).IsEqualTo(1);
        await Assert.That(links.Length).IsEqualTo(2);
        await Assert.That(links[0].Label).IsEqualTo("Products");
        await Assert.That(links[1].Label).IsEqualTo("Inventory");
    }

    [Test]
    public async Task AddResource_WithSecondSlugForSameResource_KeepsFirstSlug()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        sut.AddResource<Product>();

        // Act
        sut.AddResource<Product>("catalog");

        // Assert
        var registration = (ResourceRegistration)
            services.Single(IsRegistration).ImplementationInstance!;
        await Assert.That(registration.ControllerName).IsEqualTo("products");
        await Assert
            .That(sut.Options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.ResourceSlugRepeated);
    }

    [Test]
    public async Task AddResource_WithSlugOfAnotherResource_TurnsLaterResourceOff()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        sut.AddResource<Product>();

        // Act
        sut.AddResource<InventoryItem>("products");

        // Assert
        await Assert.That(services.Count(IsRegistration)).IsEqualTo(1);
        await Assert
            .That(sut.Options.Problems.Single().Kind)
            .IsEqualTo(ConfigurationProblemKind.SlugInUse);
    }

    private static bool IsRegistration(ServiceDescriptor descriptor) =>
        descriptor.ServiceType == typeof(ResourceRegistration);

    private sealed class CustomSidebarProvider : ISidebarItemsProvider
    {
        public Task<IReadOnlyList<SidebarItem>> GetItemsAsync(
            HttpContext httpContext,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<SidebarItem>>([new SidebarLinkItem("Help", "/help")]);
    }

    private sealed class InventoryItem;

    private sealed class Product;
}
