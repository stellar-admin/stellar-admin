using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests.Resources.Options;

public class FormColumnSpanDefinitionsTests
{
    [Test]
    public async Task Resolve_WithNoTiers_SpansOneColumn()
    {
        // Arrange
        var sut = new FormColumnSpanDefinitions();
        var columns = new FormGridTiers(1, 2, 3, 4);

        // Act
        var spans = sut.Resolve(columns);

        // Assert
        await Assert.That(spans).IsEqualTo(new FormGridTiers(1, 1, 1, 1));
    }

    [Test]
    public async Task Resolve_WithFullSpan_SpansAllColumnsAtEachTier()
    {
        // Arrange
        var sut = new FormColumnSpanDefinitions(Default: FormColumnSpanDefinitions.Full);
        var columns = new FormGridTiers(1, 2, 3, 4);

        // Act
        var spans = sut.Resolve(columns);

        // Assert
        await Assert.That(spans).IsEqualTo(new FormGridTiers(1, 2, 3, 4));
    }

    [Test]
    public async Task Resolve_WithSpanWiderThanParent_InheritsRequestedSpanAndClampsEachTier()
    {
        // Arrange
        var sut = new FormColumnSpanDefinitions(Default: 3, Large: 2);
        var columns = new FormGridTiers(1, 2, 4, 4);

        // Act
        var spans = sut.Resolve(columns);

        // Assert
        await Assert.That(spans).IsEqualTo(new FormGridTiers(1, 2, 3, 2));
    }
}
