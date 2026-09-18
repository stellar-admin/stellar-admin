using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Core.Tests.Support;
using StellarAdmin.Icons;

namespace StellarAdmin.Core.Tests;

public partial class StellarAdminBuilderTests
{
    [Test]
    public async Task AddIcon_WhenAddedAfterResolution_DoesNotModifyExistingOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Act
        sut.AddIcon("later", new IconDefinition(new Dictionary<string, string>(), []));

        // Assert
        await Assert.That(icons.TryGetIcon("later", out _)).IsFalse();
    }

    [Test]
    public async Task AddIcon_WhenAddedAfterResolution_IsIncludedInNewProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        using var firstProvider = services.BuildServiceProvider();
        _ = firstProvider.GetRequiredService<IOptions<IconOptions>>().Value;
        var expected = new IconDefinition(new Dictionary<string, string>(), []);

        // Act
        sut.AddIcon("later", expected);
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon("later", out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(expected);
    }

    [Test]
    [Arguments("CHECK")]
    [Arguments("duplicate-custom")]
    public async Task AddIcon_WhenNameAlreadyExists_ThrowsOnOptionsResolution(string name)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        var icon = new IconDefinition(new Dictionary<string, string>(), []);
        if (name == "duplicate-custom")
        {
            sut.AddIcon(name, icon);
        }

        sut.AddIcon(name, icon);
        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
        {
            _ = provider.GetRequiredService<IOptions<IconOptions>>().Value;
        };

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    public async Task AddIcon_WhenResolved_RegistersDefinition()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        var expected = new IconDefinition(new Dictionary<string, string>(), []);

        // Act
        sut.AddIcon("custom", expected);
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon("custom", out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task AddIconPack_WhenMultiplePacksAreRegistered_AppliesRegistrationOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();

        // Act
        sut.AddIconPack<TestIconPack>().AddIconPack<LucideIconPack>();
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon("check", out var icon)).IsTrue();
        await Assert.That(icon!.Shapes.Count).IsGreaterThan(0);
        await Assert.That(icon.Attributes.ContainsKey("data-test")).IsFalse();
    }
}
