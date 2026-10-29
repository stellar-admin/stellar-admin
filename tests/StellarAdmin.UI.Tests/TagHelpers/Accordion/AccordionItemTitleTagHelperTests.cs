using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Accordion;

public class AccordionItemTitleTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithoutIcon_RendersTitleBeforeIndicator()
    {
        // Arrange
        var sut = new AccordionItemTitleTagHelper(Options.Create(new IconOptions()));

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, _ => Task.FromResult("Pricing"));

        // Assert
        var summary = html.QuerySelector("summary.sa-accordion-trigger");
        await Assert.That(summary?.QuerySelector(".sa-accordion-trigger-media")).IsNull();
        await Assert.That(summary?.TextContent).IsEqualTo("Pricing");
        await Assert.That(summary?.QuerySelector(".sa-accordion-trigger-icon > svg")).IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WithIcon_RendersIconTileBeforeTitle()
    {
        // Arrange
        var sut = new AccordionItemTitleTagHelper(Options.Create(new IconOptions()))
        {
            Icon = "tag",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut, _ => Task.FromResult("Pricing"));

        // Assert
        var label = html.QuerySelector("summary > .sa-accordion-trigger-label");
        await Assert.That(label?.QuerySelector(".sa-accordion-trigger-media > svg")).IsNotNull();
        await Assert.That(label?.TextContent).IsEqualTo("Pricing");
        await Assert
            .That(html.QuerySelector("summary > .sa-accordion-trigger-icon > svg"))
            .IsNotNull();
    }
}
