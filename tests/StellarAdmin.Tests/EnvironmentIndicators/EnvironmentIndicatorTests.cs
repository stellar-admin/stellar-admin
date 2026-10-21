namespace StellarAdmin.Tests.EnvironmentIndicators;

public class EnvironmentIndicatorTests
{
    [Test]
    public async Task Color_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = EnvironmentIndicator.Banner("Live");

        // Act
        Action act = () => sut.Color = null!;

        // Assert
        await Assert.That(act).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Stripe_WithWhiteSpaceLabel_ThrowsArgumentException()
    {
        // Arrange

        // Act
        Action act = () => EnvironmentIndicator.Stripe(" ");

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }
}
