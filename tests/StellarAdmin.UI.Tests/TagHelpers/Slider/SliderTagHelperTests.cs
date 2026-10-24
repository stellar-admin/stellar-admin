using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Slider;

public partial class SliderTagHelperTests
{
    [Test]
    public async Task ProcessAsync_TemplateModelExpression_NamesInputFromPrefix()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "Booking.Passengers";
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression("", context.Metadata.GetModelExplorerForType(typeof(int), 4)),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var input = html.QuerySelector("input[type=hidden]");
        await Assert.That(input?.GetAttribute("name")).IsEqualTo("Booking.Passengers");
        await Assert.That(input?.GetAttribute("value")).IsEqualTo("4");
    }

    [Test]
    [Arguments(0, "1")]
    [Arguments(12, "9")]
    public async Task ProcessAsync_BoundValueOutsideBounds_ClampsThumbAndInput(
        int model,
        string expected
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Passengers",
                context.Metadata.GetModelExplorerForType(typeof(int), model)
            ),
            Max = 9,
            Min = 1,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.GetAttribute("value"))
            .IsEqualTo(expected);
        await Assert
            .That(html.QuerySelector("[role=slider]")?.GetAttribute("aria-valuenow"))
            .IsEqualTo(expected);
    }

    [Test]
    public async Task ProcessAsync_ReadOnly_MarksHostAndThumbAndPostsValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "budget",
            Value = "60",
            ReadOnly = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("sel-slider")?.GetAttribute("data-readonly"))
            .IsEqualTo("true");
        var thumb = html.QuerySelector("[role=slider]");
        await Assert.That(thumb?.GetAttribute("aria-readonly")).IsEqualTo("true");
        await Assert.That(thumb?.GetAttribute("tabindex")).IsEqualTo("0");
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.HasAttribute("disabled"))
            .IsFalse();
    }

    [Test]
    public async Task ProcessAsync_Disabled_DisablesHiddenInput()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SliderTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "budget",
            Value = "20,80",
            Disabled = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(
                html.QuerySelectorAll("input[type=hidden]")
                    .All(input => input.HasAttribute("disabled"))
            )
            .IsTrue();
        await Assert.That(html.QuerySelectorAll("input[type=hidden]").Length).IsEqualTo(2);
    }
}
