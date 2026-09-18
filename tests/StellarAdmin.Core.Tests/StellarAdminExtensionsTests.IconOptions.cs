using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Core.Tests.Support;
using StellarAdmin.Icons;

namespace StellarAdmin.Core.Tests;

public partial class StellarAdminExtensionsTests
{
    [Test]
    public async Task AddStellarAdmin_WhenCalledAgain_PreservesIconOverrides()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdmin().AddIconPack<TestIconPack>();

        // Act
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon("check", out var icon)).IsTrue();
        await Assert.That(icon!.Attributes["data-test"]).IsEqualTo("replacement");
    }

    [Test]
    public async Task AddStellarAdmin_WhenConfiguredDirectly_AppliesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdmin();
        services.Configure<IconOptions>(options => options.RemoveIcon("activity"));

        // Act
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon("activity", out _)).IsFalse();
    }

    [Test]
    public async Task AddStellarAdmin_WhenProvidersShareServiceCollection_CreatesSeparateOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdmin();
        using var firstProvider = services.BuildServiceProvider();
        using var secondProvider = services.BuildServiceProvider();

        // Act
        var first = firstProvider.GetRequiredService<IOptions<IconOptions>>().Value;
        var second = secondProvider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(first).IsNotSameReferenceAs(second);
    }

    [Test]
    public async Task AddStellarAdmin_WhenResolvedRepeatedly_ReusesOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();

        // Act
        var first = provider.GetRequiredService<IOptions<IconOptions>>().Value;
        var second = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(first).IsSameReferenceAs(second);
    }

    [Test]
    public async Task AddStellarAdmin_WhenServiceCollectionsDiffer_IsolatesCustomizations()
    {
        // Arrange
        var customizedServices = new ServiceCollection();
        customizedServices
            .AddStellarAdmin()
            .AddIcon("custom", new IconDefinition(new Dictionary<string, string>(), []))
            .AddIconPack<TestIconPack>();
        using var customizedProvider = customizedServices.BuildServiceProvider();
        _ = customizedProvider.GetRequiredService<IOptions<IconOptions>>().Value;
        var services = new ServiceCollection();

        // Act
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon("custom", out _)).IsFalse();
        await Assert.That(icons.TryGetIcon("check", out var icon)).IsTrue();
        await Assert.That(icon!.Shapes.Count).IsGreaterThan(0);
    }
}
