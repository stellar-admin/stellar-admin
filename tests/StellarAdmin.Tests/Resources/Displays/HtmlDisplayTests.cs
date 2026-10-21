using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Tests.Resources.Displays;

public class HtmlDisplayTests
{
    [Test]
    public async Task LineClamp_WhenLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var sut = new HtmlDisplay();

        // Act
        var act = () => sut.LineClamp = 0;

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }
}
