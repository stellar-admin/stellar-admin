namespace StellarAdmin.Tests.EnvironmentIndicators;

public class StripeEnvironmentIndicatorTests
{
    [Test]
    public async Task TabSize_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var sut = EnvironmentIndicator.Stripe("Testing");

        // Act
        Action act = () => sut.TabSize = (EnvironmentIndicatorTabSize)99;

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }
}
