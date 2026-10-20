using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Timeline;

public class TimelineItemTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenOptionsAreOmitted_RendersSolidRailAndMarker()
    {
        // Arrange
        var sut = new TimelineItemTagHelper { Title = "Booked" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("li");
        await Assert.That(item?.GetAttribute("data-slot")).IsEqualTo("timeline-item");
        await Assert.That(item?.GetAttribute("data-rail")).IsEqualTo("solid");
        await Assert.That(item?.HasAttribute("data-current")).IsFalse();
        await Assert.That(item?.HasAttribute("aria-current")).IsFalse();
        await Assert.That(item?.ClassName).IsEqualTo("sa-timeline-item existing-class");
        await Assert
            .That(html.QuerySelector(".sa-timeline-item-marker")?.GetAttribute("aria-hidden"))
            .IsEqualTo("true");
    }

    [Test]
    [Arguments(TimelineRail.Dashed, "dashed")]
    [Arguments(TimelineRail.None, "none")]
    public async Task ProcessAsync_WhenRailIsSet_RendersIt(TimelineRail rail, string expected)
    {
        // Arrange
        var sut = new TimelineItemTagHelper { Rail = rail };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("li")?.GetAttribute("data-rail")).IsEqualTo(expected);
    }

    [Test]
    public async Task ProcessAsync_WhenCurrent_RendersCurrentStep()
    {
        // Arrange
        var sut = new TimelineItemTagHelper { Current = true };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var item = html.QuerySelector("li");
        await Assert.That(item?.GetAttribute("data-current")).IsEqualTo("true");
        await Assert.That(item?.GetAttribute("aria-current")).IsEqualTo("step");
    }

    [Test]
    public async Task ProcessAsync_WhenTitleTimeAndDescriptionAreSet_RendersThemBeforeChildren()
    {
        // Arrange
        var sut = new TimelineItemTagHelper
        {
            Title = "Payment received",
            Time = "2 Mar, 09:15",
            Description = "R 48,200 by card ending 4417.",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            _ => Task.FromResult("<div id=\"child\"></div>")
        );

        // Assert
        var slots = string.Join(
            " ",
            html.QuerySelectorAll("li > *")
                .Select(element => element.GetAttribute("data-slot") ?? element.Id)
        );
        await Assert
            .That(slots)
            .IsEqualTo(
                "timeline-item-marker timeline-item-title timeline-item-time timeline-item-description child"
            );
        await Assert
            .That(html.QuerySelector("time.sa-timeline-item-time")?.TextContent)
            .IsEqualTo("2 Mar, 09:15");
    }

    [Test]
    public async Task ProcessAsync_WhenMediaIsAChildPart_RendersNoMarker()
    {
        // Arrange
        var sut = new TimelineItemTagHelper { Title = "Seat changed" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new TimelineItemMediaTagHelper(),
                    parent,
                    _ => Task.FromResult("<svg id=\"icon\"></svg>")
                )
        );

        // Assert
        await Assert.That(html.QuerySelector(".sa-timeline-item-marker")).IsNull();
        await Assert.That(html.QuerySelector(".sa-timeline-item-media > #icon")).IsNotNull();
    }

    [Test]
    [Arguments("title")]
    [Arguments("time")]
    [Arguments("description")]
    public async Task ProcessAsync_WhenAttributeAndItsChildPartAreBothSet_ThrowsInvalidOperationException(
        string part
    )
    {
        // Arrange
        var sut = new TimelineItemTagHelper();
        TagHelper child;
        switch (part)
        {
            case "title":
                sut.Title = "Booked";
                child = new TimelineItemTitleTagHelper();
                break;
            case "time":
                sut.Time = "Today";
                child = new TimelineItemTimeTagHelper();
                break;
            default:
                sut.Description = "Online, by Amara Okafor.";
                child = new TimelineItemDescriptionTagHelper();
                break;
        }

        // Act
        Func<Task> act = () =>
            TagHelperRenderer.RenderAsync(
                sut,
                parent => TagHelperRenderer.RenderChildAsync(child, parent)
            );

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
