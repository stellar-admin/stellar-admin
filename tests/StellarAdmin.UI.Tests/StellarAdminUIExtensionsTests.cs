using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.Tests;

public partial class StellarAdminUIExtensionsTests
{
    [Test]
    public async Task AddStellarAdminUI_WhenIconsAreConfigured_PreservesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdminUI();
        var expected = new IconDefinition(new Dictionary<string, string>(), []);
        sut.AddIcon("custom", expected).AddIconPack<ReplacementPack>();

        // Act
        services.AddStellarAdminUI();
        using var provider = services.BuildServiceProvider();
        var icons = provider.GetRequiredService<IOptions<IconOptions>>().Value;

        // Assert
        await Assert.That(icons.TryGetIcon("custom", out var custom)).IsTrue();
        await Assert.That(custom).IsSameReferenceAs(expected);
        await Assert.That(icons.TryGetIcon("check", out var replacement)).IsTrue();
        await Assert.That(replacement!.Attributes["data-test"]).IsEqualTo("replacement");
    }

    private sealed class ReplacementPack : IIconPack
    {
        public IReadOnlyDictionary<string, IconDefinition> GetIcons() =>
            new Dictionary<string, IconDefinition>
            {
                ["check"] = new(
                    new Dictionary<string, string> { ["data-test"] = "replacement" },
                    []
                ),
            };

        public IReadOnlyDictionary<SemanticIconRole, string> GetSemanticIconMappings() =>
            new Dictionary<SemanticIconRole, string>();
    }
}
