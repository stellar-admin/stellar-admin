using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.Dashboard.Sidebar;

namespace StellarAdmin.Dashboard.Tests;

public class StellarAdminDashboardBuilderExtensionsTests
{
    [Test]
    public async Task ConfigureTheme_WithUnknownPreset_RejectsConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.ConfigureTheme(theme => theme.Preset = (DashboardThemePreset)int.MaxValue);
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () => _ = provider.GetRequiredService<IOptions<DashboardThemeOptions>>().Value;

        // Assert
        await Assert.That(act).Throws<OptionsValidationException>();
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
    [Arguments("Products")]
    [Arguments("order_items")]
    [Arguments("order--items")]
    [Arguments("-products")]
    [Arguments("catalog/products")]
    public async Task AddResource_WithInvalidSlug_ThrowsArgumentException(string slug)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        Action act = () => sut.AddResource<Product>(slug);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task AddResource_WithSlugOfAnotherResource_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>();

        // Act
        Action act = () => sut.AddResource<InventoryItem>("products");

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("", true)]
    [Arguments(" ", true)]
    [Arguments("", false)]
    [Arguments(" ", false)]
    public async Task AddResource_WithBlankSidebarText_RejectsConfiguration(
        string value,
        bool label
    )
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();
        sut.AddResource<Product>(resource =>
            resource.SidebarItem(item =>
            {
                if (label)
                {
                    item.Label = value;
                }
                else
                {
                    item.Group = value;
                }
            })
        );
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
    public async Task AddResource_WithCustomSidebarProvider_PreservesCustomItems()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ISidebarItemsProvider>(new CustomSidebarProvider());
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<Product>();
        using var provider = services.BuildServiceProvider();
        var items = new List<SidebarItem>();
        foreach (var sidebarProvider in provider.GetServices<ISidebarItemsProvider>())
        {
            items.AddRange(await sidebarProvider.GetItemsAsync(new DefaultHttpContext()));
        }

        // Assert
        await Assert.That(items.Count).IsEqualTo(2);
        await Assert.That(items[0]).IsTypeOf<SidebarLinkItem>();
        await Assert.That(items[1]).IsTypeOf<SidebarGroupItem>();
    }

    [Test]
    public async Task AddResource_WithRepeatedAndDistinctResources_RegistersOneSidebarProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin().AddDashboard();

        // Act
        sut.AddResource<Product>();
        sut.AddResource<InventoryItem>(resource => resource.PluralLabel = "Inventory");
        sut.AddResource<Product>();
        using var provider = services.BuildServiceProvider();
        var sidebarProviders = provider.GetServices<ISidebarItemsProvider>().ToArray();
        var group = (SidebarGroupItem)
            (await sidebarProviders.Single().GetItemsAsync(new DefaultHttpContext())).Single();

        // Assert
        await Assert.That(sidebarProviders.Length).IsEqualTo(1);
        await Assert.That(group.Items.Length).IsEqualTo(2);
        await Assert.That(group.Items[0].Label).IsEqualTo("Products");
        await Assert.That(group.Items[1].Label).IsEqualTo("Inventory");
    }

    private sealed class CustomSidebarProvider : ISidebarItemsProvider
    {
        public Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext) =>
            Task.FromResult<SidebarItem[]>([new SidebarLinkItem("Help", "/help")]);
    }

    private sealed class InventoryItem;

    private sealed class Product;
}
