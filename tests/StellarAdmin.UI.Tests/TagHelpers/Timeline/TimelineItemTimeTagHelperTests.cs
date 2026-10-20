using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Timeline;

public class TimelineItemTimeTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenDateTimeIsSet_RendersMachineReadableDateTime()
    {
        // Arrange
        var sut = new TimelineItemTimeTagHelper
        {
            DateTime = new DateTimeOffset(2026, 10, 20, 9, 40, 0, TimeSpan.FromHours(2)),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            _ => Task.FromResult("5 min ago")
        );

        // Assert
        var time = html.QuerySelector("time");
        await Assert.That(time?.GetAttribute("datetime")).IsEqualTo("2026-10-20T09:40:00+02:00");
        await Assert.That(time?.TextContent).IsEqualTo("5 min ago");
    }

    [Test]
    public async Task ProcessAsync_WhenDateTimeIsOmitted_RendersNoDateTimeAttribute()
    {
        // Arrange
        var sut = new TimelineItemTimeTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("time")?.HasAttribute("datetime")).IsFalse();
    }
}
