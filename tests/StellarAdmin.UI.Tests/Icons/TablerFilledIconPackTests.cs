using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.Tests.Icons;

public class TablerFilledIconPackTests
{
    [Test]
    [Arguments("bong", "420")]
    public async Task GetIcons_ReturnsUpstreamKeywords(string name, string keyword)
    {
        // Arrange
        var sut = new TablerFilledIconPack();

        // Act
        var icons = sut.GetIcons();

        // Assert
        await Assert.That(icons[name].Keywords).Contains(keyword);
    }
}
