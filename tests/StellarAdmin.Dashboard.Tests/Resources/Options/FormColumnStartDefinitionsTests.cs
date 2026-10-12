using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Tests.Resources.Options;

public class FormColumnStartDefinitionsTests
{
    [Test]
    public async Task Resolve_WithNoTiers_StartsAutomatically()
    {
        // Arrange
        var sut = new FormColumnStartDefinitions();
        var columns = new GridTiers(1, 2, 3, 4);
        var span = new GridTiers(1, 1, 1, 1);

        // Act
        var starts = sut.Resolve(columns, span);

        // Assert
        await Assert.That(starts).IsEqualTo(new GridTiers(0, 0, 0, 0));
    }

    [Test]
    public async Task Resolve_WithBaseStart_InheritsStartAndDropsItWhereItDoesNotFit()
    {
        // Arrange
        var sut = new FormColumnStartDefinitions(Base: 2);
        var columns = new GridTiers(1, 2, 3, 4);
        var span = new GridTiers(1, 2, 2, 2);

        // Act
        var starts = sut.Resolve(columns, span);

        // Assert
        await Assert.That(starts).IsEqualTo(new GridTiers(0, 0, 2, 2));
    }

    [Test]
    public async Task Resolve_WithAutoTier_ResetsStartFromThatTier()
    {
        // Arrange
        var sut = new FormColumnStartDefinitions(Small: 2, Large: ResponsiveGrid.AutoStart);
        var columns = new GridTiers(1, 2, 3, 4);
        var span = new GridTiers(1, 1, 1, 1);

        // Act
        var starts = sut.Resolve(columns, span);

        // Assert
        await Assert.That(starts).IsEqualTo(new GridTiers(0, 2, 2, 0));
    }
}
