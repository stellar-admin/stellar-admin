using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers.Tests;

public partial class StellarAdminTagHelpersExtensionsTests
{
    [Test]
    public async Task AddTagHelpers_WhenIconsAreConfigured_PreservesConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var sut = services.AddStellarAdmin();
        var expected = new IconDefinition(new Dictionary<string, string>(), []);
        sut.AddIcon("custom", expected).AddIconPack<ReplacementPack>();

        // Act
        services.AddStellarAdmin().AddTagHelpers();
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
        public IDictionary<string, IconDefinition> GetIcons() =>
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
