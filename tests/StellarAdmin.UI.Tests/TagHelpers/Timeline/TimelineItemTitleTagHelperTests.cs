using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Timeline;

public class TimelineItemTitleTagHelperTests
{
    [Test]
    [Arguments(null, false)]
    [Arguments(false, false)]
    [Arguments(true, true)]
    public async Task ProcessAsync_WhenMutedIsSet_RendersMutedAttributeOnlyWhenTrue(
        bool? muted,
        bool expected
    )
    {
        // Arrange
        var sut = new TimelineItemTitleTagHelper { Muted = muted };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=timeline-item-title]")?.HasAttribute("data-muted"))
            .IsEqualTo(expected);
    }
}
