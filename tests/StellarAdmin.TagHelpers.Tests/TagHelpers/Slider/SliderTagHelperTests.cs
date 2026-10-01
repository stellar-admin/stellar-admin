using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Slider;

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
}
