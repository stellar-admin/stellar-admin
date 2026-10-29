using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Accordion;

public class AccordionTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersDefaultVariant()
    {
        // Arrange
        var sut = new AccordionTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var accordion = html.QuerySelector("[data-slot=accordion]");
        await Assert.That(accordion?.GetAttribute("data-variant")).IsEqualTo("default");
        await Assert.That(accordion?.ClassList).DoesNotContain("sa-accordion-variant-contained");
        await Assert.That(accordion?.ClassList).Contains("existing-class");
    }

    [Test]
    public async Task ProcessAsync_WhenContained_RendersContainedVariant()
    {
        // Arrange
        var sut = new AccordionTagHelper { Variant = AccordionVariant.Contained };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var accordion = html.QuerySelector("[data-slot=accordion]");
        await Assert.That(accordion?.GetAttribute("data-variant")).IsEqualTo("contained");
        await Assert.That(accordion?.ClassList).Contains("sa-accordion-variant-contained");
    }
}
