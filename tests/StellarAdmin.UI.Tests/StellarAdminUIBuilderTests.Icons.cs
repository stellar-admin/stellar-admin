using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests;

public partial class StellarAdminUIBuilderTests
{
    [Test]
    public async Task AddIcon_WhenAddedAfterResolution_DoesNotModifyExistingOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdminUI();
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
        var sut = services.AddStellarAdminUI();
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
    public async Task AddIcon_WhenNameAlreadyExists_ReplacesDefinition(string name)
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdminUI();
        var expected = new IconDefinition(new Dictionary<string, string>(), []);
        if (name == "duplicate-custom")
        {
            sut.AddIcon(name, new IconDefinition(new Dictionary<string, string>(), []));
        }

        // Act
        sut.AddIcon(name, expected);
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon(name, out var icon)).IsTrue();
        await Assert.That(icon).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task AddIconPack_WhenPackIsInvalid_FailsStartupValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdminUI().AddIconPack<InvalidMappingPack>();
        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IStartupValidator>();

        // Act
        Action act = () => validator.Validate();

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }

    [Test]
    [Arguments("add")]
    [Arguments("add-pack")]
    [Arguments("clear")]
    [Arguments("map")]
    [Arguments("remove")]
    public async Task IconOptions_WhenChangedAfterResolution_ThrowsInvalidOperationException(
        string change
    )
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddStellarAdminUI();
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Act
        Action act = change switch
        {
            "add" => () =>
                icons.AddIcon("later", new IconDefinition(new Dictionary<string, string>(), [])),
            "add-pack" => () => icons.AddIconPack<TestIconPack>(),
            "clear" => () => icons.ClearIcons(),
            "map" => () => icons.MapSemanticIcon(SemanticIconRole.Close, "check"),
            _ => () => icons.RemoveIcon("check"),
        };

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
        await Assert.That(icons.TryGetIcon("check", out _)).IsTrue();
    }

    [Test]
    public async Task AddIcon_WhenResolved_RegistersDefinition()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdminUI();
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
        var sut = services.AddStellarAdminUI();

        // Act
        sut.AddIconPack<TestIconPack>().AddIconPack<LucideIconPack>();
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon("check", out var icon)).IsTrue();
        await Assert.That(icon!.Shapes.Count).IsGreaterThan(0);
        await Assert.That(icon.Attributes.ContainsKey("data-test")).IsFalse();
    }

    private sealed class InvalidMappingPack : IIconPack
    {
        public IReadOnlyDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>();

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string> { [SemanticIconRole.Close] = "absent" };
    }
}
