using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Tests.Resources.Options;

public class FormColumnSpanDefinitionsTests
{
    [Test]
    public async Task Resolve_WithNoTiers_SpansOneColumn()
    {
        // Arrange
        var sut = new FormColumnSpanDefinitions();
        var columns = new GridTiers(1, 2, 3, 4);

        // Act
        var spans = sut.Resolve(columns);

        // Assert
        await Assert.That(spans).IsEqualTo(new GridTiers(1, 1, 1, 1));
    }

    [Test]
    public async Task Resolve_WithFullSpan_SpansAllColumnsAtEachTier()
    {
        // Arrange
        var sut = new FormColumnSpanDefinitions(Base: ResponsiveGrid.FullSpan);
        var columns = new GridTiers(1, 2, 3, 4);

        // Act
        var spans = sut.Resolve(columns);

        // Assert
        await Assert.That(spans).IsEqualTo(new GridTiers(1, 2, 3, 4));
    }

    [Test]
    public async Task Resolve_WithSpanWiderThanParent_InheritsRequestedSpanAndClampsEachTier()
    {
        // Arrange
        var sut = new FormColumnSpanDefinitions(Base: 3, Large: 2);
        var columns = new GridTiers(1, 2, 4, 4);

        // Act
        var spans = sut.Resolve(columns);

        // Assert
        await Assert.That(spans).IsEqualTo(new GridTiers(1, 2, 3, 2));
    }
}
