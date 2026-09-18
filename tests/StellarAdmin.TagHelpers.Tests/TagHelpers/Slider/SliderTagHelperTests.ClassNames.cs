using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Slider;

public partial class SliderTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenClassNamesAreConfigured_AppliesClassesToExpectedElements()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            ClassNames = new SliderClassNames
            {
                Control = "slider-control",
                Track = "slider-track",
                Range = "slider-range",
                Thumb = "slider-thumb",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".slider-control").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("sel-slider.sa-slider.slider-control").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".slider-track").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("span.sa-slider-track.slider-track").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".slider-range").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("span.sa-slider-range.slider-range").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".slider-thumb").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("span.sa-slider-thumb.slider-thumb").Length)
            .IsEqualTo(1);
    }
}
