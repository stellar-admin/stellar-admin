using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.Tests.Icons;

public class TablerOutlineIconPackTests
{
    [Test]
    [Arguments("wifi", "hotspot")]
    [Arguments("ambulance", "911")]
    public async Task GetIcons_ReturnsUpstreamKeywords(string name, string keyword)
    {
        // Arrange
        var sut = new TablerOutlineIconPack();

        // Act
        var icons = sut.GetIcons();

        // Assert
        await Assert.That(icons[name].Keywords).Contains(keyword);
    }
}
