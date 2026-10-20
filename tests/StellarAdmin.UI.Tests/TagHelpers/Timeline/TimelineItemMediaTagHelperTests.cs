using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Timeline;

public class TimelineItemMediaTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenLabelIsSet_RendersHiddenTextAfterContent()
    {
        // Arrange
        var sut = new TimelineItemMediaTagHelper { Label = "Failed" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            _ => Task.FromResult("<svg id=\"icon\"></svg>")
        );

        // Assert
        await Assert
            .That(html.QuerySelector(".sa-timeline-item-media > #icon + .sr-only")?.TextContent)
            .IsEqualTo("Failed: ");
    }

    [Test]
    public async Task ProcessAsync_WhenLabelIsOmitted_RendersNoHiddenText()
    {
        // Arrange
        var sut = new TimelineItemMediaTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector(".sr-only")).IsNull();
    }
}
