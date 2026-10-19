using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.Tests.Icons;

public class LucideIconPackTests
{
    [Test]
    [Arguments("wifi", "wireless")]
    [Arguments("waves-ladder", "pool")]
    public async Task GetIcons_ReturnsUpstreamKeywords(string name, string keyword)
    {
        // Arrange
        var sut = new LucideIconPack();

        // Act
        var icons = sut.GetIcons();

        // Assert
        await Assert.That(icons[name].Keywords).Contains(keyword);
    }
}
