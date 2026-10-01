using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Slider;

public class SliderMarkTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithContent_RendersTickAndLabel()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarkTagHelper { Value = 5 };
        var slider = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Min = 1,
            Max = 5,
            Value = "4",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            slider,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new SliderMarksTagHelper(),
                    parent,
                    marks =>
                        TagHelperRenderer.RenderChildAsync(
                            sut,
                            marks,
                            _ => Task.FromResult("Excellent")
                        )
                )
        );

        // Assert
        var mark = html.QuerySelector("[data-slot=slider-mark]");
        await Assert.That(mark?.GetAttribute("data-value")).IsEqualTo("5");
        await Assert.That(mark?.GetAttribute("data-state")).IsEqualTo("out-of-range");
        await Assert.That(mark?.QuerySelector("[data-slot=slider-mark-tick]")).IsNotNull();
        await Assert
            .That(mark?.QuerySelector("[data-slot=slider-mark-label]")?.TextContent)
            .IsEqualTo("Excellent");
    }

    [Test]
    public async Task ProcessAsync_WithoutContent_RendersTickOnly()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarkTagHelper { Value = 30 };
        var slider = new SliderTagHelper(context.Generator) { ViewContext = context.ViewContext };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            slider,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new SliderMarksTagHelper(),
                    parent,
                    marks => TagHelperRenderer.RenderChildAsync(sut, marks)
                )
        );

        // Assert
        var mark = html.QuerySelector("[data-slot=slider-mark]");
        await Assert.That(mark?.QuerySelector("[data-slot=slider-mark-tick]")).IsNotNull();
        await Assert.That(mark?.QuerySelector("[data-slot=slider-mark-label]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WhenValueIsOutsideBounds_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarkTagHelper { Value = 120 };
        var slider = new SliderTagHelper(context.Generator) { ViewContext = context.ViewContext };

        // Act
        var render = async () =>
            await TagHelperRenderer.RenderAsync(
                slider,
                parent =>
                    TagHelperRenderer.RenderChildAsync(
                        new SliderMarksTagHelper(),
                        parent,
                        marks => TagHelperRenderer.RenderChildAsync(sut, marks)
                    )
            );

        // Assert
        await Assert.That(render).Throws<ArgumentOutOfRangeException>();
    }
}
