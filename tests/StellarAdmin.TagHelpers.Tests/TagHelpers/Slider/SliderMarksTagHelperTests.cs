using System.Globalization;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Slider;

public class SliderMarksTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenEmpty_GeneratesMarkEveryIntervalAndAtMax()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper { Interval = 30 };

        // Act
        using var html = await RenderInSliderAsync(context, sut);

        // Assert
        var marks = html.QuerySelector("[data-slot=slider-marks]");
        await Assert.That(marks?.GetAttribute("aria-hidden")).IsEqualTo("true");
        await Assert.That(MarkValues(html)).IsEqualTo("0 30 60 90 100");
        await Assert
            .That(html.QuerySelectorAll("[data-slot=slider-mark-tick]").Length)
            .IsEqualTo(5);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=slider-mark-label]").Length)
            .IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_WhenIntervalIsNotSet_UsesSliderStep()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper();

        // Act
        using var html = await RenderInSliderAsync(context, sut, slider => slider.Step = 25);

        // Assert
        await Assert.That(MarkValues(html)).IsEqualTo("0 25 50 75 100");
    }

    [Test]
    public async Task ProcessAsync_WhenLabelsAreEnds_LabelsMinimumAndMaximumWithValueFormat()
    {
        // Arrange
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper { Interval = 250, Labels = SliderMarkLabels.Ends };

        // Act
        using var html = await RenderInSliderAsync(
            context,
            sut,
            slider =>
            {
                slider.Max = 1000;
                slider.ValueFormat = "${0}";
            }
        );

        // Assert
        await Assert.That(MarkValues(html)).IsEqualTo("0 250 500 750 1000");
        var labels = html.QuerySelectorAll("[data-slot=slider-mark-label]")
            .Select(label => label.TextContent);
        await Assert.That(string.Join(" ", labels)).IsEqualTo("$0 $1,000");
    }

    [Test]
    public async Task ProcessAsync_WhenTicksAreHidden_RendersOnlyLabelledMarks()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper { Labels = SliderMarkLabels.Ends, ShowTicks = false };

        // Act
        using var html = await RenderInSliderAsync(context, sut);

        // Assert
        await Assert.That(MarkValues(html)).IsEqualTo("0 100");
        await Assert
            .That(html.QuerySelectorAll("[data-slot=slider-mark-tick]").Length)
            .IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_WhenIntervalGeneratesTooManyMarks_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper { Interval = 1 };

        // Act
        var render = async () =>
            await RenderInSliderAsync(context, sut, slider => slider.Max = 1000);

        // Assert
        await Assert.That(render).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ProcessAsync_WithRangeSlider_MarksValuesBetweenThumbsInRange()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper { Interval = 20 };

        // Act
        using var html = await RenderInSliderAsync(context, sut, slider => slider.Value = "20,60");

        // Assert
        var states = html.QuerySelectorAll("[data-slot=slider-mark]")
            .Select(mark => $"{mark.GetAttribute("data-value")}:{mark.GetAttribute("data-state")}");
        await Assert
            .That(string.Join(" ", states))
            .IsEqualTo(
                "0:out-of-range 20:in-range 40:in-range 60:in-range 80:out-of-range 100:out-of-range"
            );
    }

    [Test]
    public async Task ProcessAsync_MarksTheMinimumAndMaximumAsBounds()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper { Interval = 50 };

        // Act
        using var html = await RenderInSliderAsync(context, sut);

        // Assert
        var bounds = html.QuerySelectorAll("[data-slot=slider-mark]")
            .Select(mark => $"{mark.GetAttribute("data-value")}:{mark.GetAttribute("data-bound")}");
        await Assert.That(string.Join(" ", bounds)).IsEqualTo("0:min 50: 100:max");
    }

    [Test]
    [Arguments(SliderThumbAlignment.Center, "left: 25%;")]
    [Arguments(
        SliderThumbAlignment.Edge,
        "left: calc(0.25 * (100% - var(--sa-slider-thumb-size, 1rem)) + var(--sa-slider-thumb-size, 1rem) / 2);"
    )]
    public async Task ProcessAsync_PositionsMarksAtThumbCentres(
        SliderThumbAlignment alignment,
        string expected
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper { Interval = 25 };

        // Act
        using var html = await RenderInSliderAsync(
            context,
            sut,
            slider => slider.ThumbAlignment = alignment
        );

        // Assert
        await Assert
            .That(
                html.QuerySelector("[data-slot=slider-mark][data-value='25']")
                    ?.GetAttribute("style")
            )
            .IsEqualTo(expected);
    }

    [Test]
    public async Task ProcessAsync_WhenMarksAreAuthored_RendersThemInsteadOfGenerating()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderMarksTagHelper { Interval = 10, ShowTicks = false };

        // Act
        using var html = await RenderInSliderAsync(
            context,
            sut,
            children: parent =>
                TagHelperRenderer.RenderChildAsync(
                    new SliderMarkTagHelper { Value = 50 },
                    parent,
                    _ => Task.FromResult("Halfway")
                )
        );

        // Assert
        var mark = html.QuerySelector("[data-slot=slider-mark]");
        await Assert.That(MarkValues(html)).IsEqualTo("50");
        await Assert.That(mark?.QuerySelector("[data-slot=slider-mark-tick]")).IsNull();
        await Assert
            .That(mark?.QuerySelector("[data-slot=slider-mark-label]")?.TextContent)
            .IsEqualTo("Halfway");
    }

    private static string MarkValues(IHtmlDocument html) =>
        string.Join(
            " ",
            html.QuerySelectorAll("[data-slot=slider-mark]")
                .Select(mark => mark.GetAttribute("data-value"))
        );

    private static Task<IHtmlDocument> RenderInSliderAsync(
        RenderingContext context,
        SliderMarksTagHelper sut,
        Action<SliderTagHelper>? configure = null,
        Func<TagHelperContext, Task<string>>? children = null
    )
    {
        var slider = new SliderTagHelper(context.Generator) { ViewContext = context.ViewContext };
        configure?.Invoke(slider);

        return TagHelperRenderer.RenderAsync(
            slider,
            parent => TagHelperRenderer.RenderChildAsync(sut, parent, children)
        );
    }
}
