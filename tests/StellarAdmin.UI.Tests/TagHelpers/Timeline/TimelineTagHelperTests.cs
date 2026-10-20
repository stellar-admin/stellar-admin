using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Timeline;

public class TimelineTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenOptionsAreOmitted_RendersDefaults()
    {
        // Arrange
        var sut = new TimelineTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var root = html.QuerySelector("ol");
        await Assert.That(root?.GetAttribute("role")).IsEqualTo("list");
        await Assert.That(root?.GetAttribute("data-slot")).IsEqualTo("timeline");
        await Assert.That(root?.GetAttribute("data-size")).IsEqualTo("default");
        await Assert.That(root?.GetAttribute("data-marker")).IsEqualTo("dot");
        await Assert.That(root?.GetAttribute("data-media-style")).IsEqualTo("disc");
        await Assert.That(root?.GetAttribute("data-time-placement")).IsEqualTo("end");
        await Assert.That(root?.ClassName).IsEqualTo("sa-timeline existing-class");
    }

    [Test]
    public async Task ProcessAsync_WhenOptionsAreSet_RendersThem()
    {
        // Arrange
        var sut = new TimelineTagHelper
        {
            Marker = TimelineMarker.Number,
            MediaStyle = TimelineMediaStyle.Ring,
            Size = TimelineSize.Small,
            TimePlacement = TimelineTimePlacement.Start,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var root = html.QuerySelector("ol");
        await Assert.That(root?.GetAttribute("data-size")).IsEqualTo("sm");
        await Assert.That(root?.GetAttribute("data-marker")).IsEqualTo("number");
        await Assert.That(root?.GetAttribute("data-media-style")).IsEqualTo("ring");
        await Assert.That(root?.GetAttribute("data-time-placement")).IsEqualTo("start");
    }
}
