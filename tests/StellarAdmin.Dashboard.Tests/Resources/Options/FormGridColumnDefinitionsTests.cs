using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests.Resources.Options;

public class FormGridColumnDefinitionsTests
{
    [Test]
    public async Task FromCount_WithCount_UsesOneColumnBelowMedium()
    {
        // Arrange
        var sut = FormGridColumnDefinitions.FromCount(3);

        // Act
        var tiers = sut.Resolve();

        // Assert
        await Assert.That(tiers).IsEqualTo(new FormGridTiers(1, 1, 3, 3));
    }

    [Test]
    [Arguments(0)]
    [Arguments(13)]
    public async Task FromCount_WithCountOutOfRange_ThrowsArgumentOutOfRangeException(int count)
    {
        // Arrange

        // Act
        Action act = () => FormGridColumnDefinitions.FromCount(count);

        // Assert
        await Assert.That(act).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Resolve_WithNoTiers_UsesOneColumn()
    {
        // Arrange
        var sut = new FormGridColumnDefinitions();

        // Act
        var tiers = sut.Resolve();

        // Assert
        await Assert.That(tiers).IsEqualTo(new FormGridTiers(1, 1, 1, 1));
    }

    [Test]
    public async Task Resolve_WithUnsetTiers_InheritsFromNextSmallerTier()
    {
        // Arrange
        var sut = new FormGridColumnDefinitions(Small: 2, Large: 4);

        // Act
        var tiers = sut.Resolve();

        // Assert
        await Assert.That(tiers).IsEqualTo(new FormGridTiers(1, 2, 2, 4));
    }
}
