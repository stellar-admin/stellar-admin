using System.Globalization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Slider;

public partial class SliderTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenValueFormatIsSet_FormatsThumbValueTextInCurrentCulture()
    {
        // Arrange
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Max = 2000,
            Value = "1500",
            ValueFormat = "{0} €",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var host = html.QuerySelector("sel-slider");
        await Assert.That(host?.GetAttribute("data-value-format")).IsEqualTo("{0} €");
        await Assert.That(host?.GetAttribute("data-value-locale")).IsEqualTo("de-DE");
        await Assert
            .That(html.QuerySelector("[role=slider]")?.GetAttribute("aria-valuetext"))
            .IsEqualTo("1.500 €");
    }

    [Test]
    public async Task ProcessAsync_WhenValueFormatIsNotSet_OmitsValueText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator) { ViewContext = context.ViewContext };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[role=slider]")?.HasAttribute("aria-valuetext"))
            .IsFalse();
        await Assert
            .That(html.QuerySelector("sel-slider")?.HasAttribute("data-value-format"))
            .IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenValueFormatIsEmpty_OmitsValueText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            ValueFormat = string.Empty,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[role=slider]")?.HasAttribute("aria-valuetext"))
            .IsFalse();
        await Assert
            .That(html.QuerySelector("sel-slider")?.HasAttribute("data-value-format"))
            .IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenValueFormatHasNoPlaceholder_ThrowsArgumentException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            ValueFormat = "km",
        };

        // Act
        var render = async () => await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(render).Throws<ArgumentException>();
    }

    [Test]
    public async Task ProcessAsync_WhenFieldLabelRenders_LabelsEachThumbByLabelId()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Label = "Price per night",
            Value = "20,80",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var label = html.QuerySelector("[data-slot=field-label]");
        await Assert.That(label?.Id).IsNotNull();
        await Assert.That(label?.HasAttribute("for")).IsFalse();
        foreach (var thumb in html.QuerySelectorAll("[role=slider]"))
        {
            await Assert.That(thumb.GetAttribute("aria-labelledby")).IsEqualTo(label?.Id);
        }
    }

    [Test]
    public async Task ProcessAsync_WhenThumbLabelsAreSet_NamesEachThumbInOrder()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Label = "Price per night",
            ThumbLabels = "Minimum price, Maximum price",
            Value = "20,80",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var thumbs = html.QuerySelectorAll("[role=slider]");
        await Assert.That(thumbs[0].GetAttribute("aria-label")).IsEqualTo("Minimum price");
        await Assert.That(thumbs[1].GetAttribute("aria-label")).IsEqualTo("Maximum price");
        await Assert.That(thumbs[0].HasAttribute("aria-labelledby")).IsFalse();
        await Assert.That(thumbs[1].HasAttribute("aria-labelledby")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WhenMarksAreChildren_RendersThemBetweenTrackAndThumbs()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator) { ViewContext = context.ViewContext };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new SliderMarksTagHelper { Interval = 50 },
                    parent
                )
        );

        // Assert
        var parts = html.QuerySelector("sel-slider")!
            .Children.Select(child => child.GetAttribute("data-slot"));
        await Assert
            .That(string.Join(" ", parts))
            .IsEqualTo("slider-track slider-marks slider-thumb");
    }

    [Test]
    public async Task ProcessAsync_WhenBoundWithId_KeepsIdOnHost()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Passengers",
                context.Metadata.GetModelExplorerForType(typeof(int), 4)
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            outputAttributes: [new TagHelperAttribute("id", "passengers")]
        );

        // Assert
        await Assert.That(html.QuerySelector("sel-slider")?.Id).IsEqualTo("passengers");
    }
}
