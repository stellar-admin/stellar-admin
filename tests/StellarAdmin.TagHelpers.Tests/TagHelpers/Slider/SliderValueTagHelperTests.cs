using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Slider;

public class SliderValueTagHelperTests
{
    [Test]
    public async Task Process_WithoutAttributes_RendersEmptyQuietOutput()
    {
        // Arrange
        var sut = new SliderValueTagHelper();

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var output = html.QuerySelector("output");
        await Assert.That(output?.GetAttribute("data-slot")).IsEqualTo("slider-value");
        await Assert.That(output?.GetAttribute("aria-live")).IsEqualTo("off");
        await Assert.That(output?.HasAttribute("for")).IsFalse();
        await Assert.That(output?.HasAttribute("data-index")).IsFalse();
        await Assert.That(output?.ClassList).Contains("sa-slider-value");
        await Assert.That(output?.TextContent).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Process_WithForAndIndex_RendersTargetAndThumbIndex()
    {
        // Arrange
        var sut = new SliderValueTagHelper { For = "rating", Index = 1 };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var output = html.QuerySelector("output");
        await Assert.That(output?.GetAttribute("for")).IsEqualTo("rating");
        await Assert.That(output?.GetAttribute("data-index")).IsEqualTo("1");
    }
}
