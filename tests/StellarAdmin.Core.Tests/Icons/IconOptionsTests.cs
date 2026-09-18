using StellarAdmin.Icons;

namespace StellarAdmin.Core.Tests.Icons;

public partial class IconOptionsTests
{
    [Test]
    public async Task ClearIcons_WhenDefaultsAreRegistered_RemovesAllIcons()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        sut.ClearIcons();

        // Assert
        await Assert.That(sut.GetIconNames()).IsEmpty();
    }

    [Test]
    public async Task ClearIcons_WhenNamesWereRetrieved_DoesNotMutateSnapshot()
    {
        // Arrange
        var sut = new IconOptions();
        var names = sut.GetIconNames();

        // Act
        sut.ClearIcons();

        // Assert
        await Assert.That(names).Contains("check");
        await Assert.That(sut.GetIconNames()).IsEmpty();
    }

    [Test]
    public async Task RemoveIcon_WhenIconWasAlreadyRemoved_ReturnsFalse()
    {
        // Arrange
        var sut = new IconOptions();
        sut.RemoveIcon("check");

        // Act
        var removed = sut.RemoveIcon("check");

        // Assert
        await Assert.That(removed).IsFalse();
    }

    [Test]
    public async Task RemoveIcon_WhenNameCasingDiffers_RemovesIcon()
    {
        // Arrange
        var sut = new IconOptions();

        // Act
        var removed = sut.RemoveIcon("CHECK");

        // Assert
        await Assert.That(removed).IsTrue();
        await Assert.That(sut.TryGetIcon("check", out var icon)).IsFalse();
        await Assert.That(icon).IsNull();
    }
}
