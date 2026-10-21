using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Tests.Resources.Displays;

public class HtmlDisplayTests
{
    [Test]
    public async Task LineClamp_WhenLessThanOne_KeepsValue()
    {
        // Arrange
        var sut = new HtmlDisplay();

        // Act
        sut.LineClamp = 0;

        // Assert
        await Assert.That(sut.LineClamp).IsEqualTo(2);
    }
}
