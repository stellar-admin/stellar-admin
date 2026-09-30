using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Toaster;

public class ToasterTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenAttributesAreNotSet_RendersDefaults()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var toaster = html.QuerySelector("sel-toaster[data-slot=toaster]");
        var viewport = toaster?.QuerySelector("[data-slot=toast-viewport]");
        await Assert.That(toaster?.GetAttribute("duration")).IsEqualTo("5000");
        await Assert.That(toaster?.GetAttribute("limit")).IsEqualTo("3");
        await Assert.That(viewport?.GetAttribute("data-position")).IsEqualTo("bottom-right");
        await Assert.That(viewport?.GetAttribute("popover")).IsEqualTo("manual");
        await Assert.That(viewport?.GetAttribute("role")).IsEqualTo("region");
        await Assert.That(viewport?.GetAttribute("aria-live")).IsEqualTo("polite");
        await Assert.That(toaster?.QuerySelector("script")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WhenAttributesAreSet_RendersThem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Duration = TimeSpan.FromSeconds(8);
        sut.Limit = 5;
        sut.Position = ToasterPosition.TopCenter;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var toaster = html.QuerySelector("sel-toaster");
        var viewport = toaster?.QuerySelector("[data-slot=toast-viewport]");
        await Assert.That(toaster?.GetAttribute("duration")).IsEqualTo("8000");
        await Assert.That(toaster?.GetAttribute("limit")).IsEqualTo("5");
        await Assert.That(viewport?.GetAttribute("data-position")).IsEqualTo("top-center");
    }

    [Test]
    public async Task ProcessAsync_WhenClassIsSet_AppliesItToViewport()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var toaster = html.QuerySelector("sel-toaster");
        var viewport = toaster?.QuerySelector("[data-slot=toast-viewport]");
        await Assert.That(toaster?.HasAttribute("class")).IsFalse();
        await Assert.That(viewport?.ClassList.Contains("sa-toast-viewport")).IsTrue();
        await Assert.That(viewport?.ClassList.Contains("existing-class")).IsTrue();
    }

    [Test]
    public async Task ProcessAsync_Always_RendersTemplateWithIconForEachType()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var template =
            html.QuerySelector("template[data-slot=toast-template]") as IHtmlTemplateElement;
        var toast = template?.Content.QuerySelector("[data-slot=toast]");
        var icons = toast
            ?.QuerySelectorAll("[data-slot=toast-icon] [data-toast-icon]")
            .Select(icon => icon.GetAttribute("data-toast-icon") ?? "");
        await Assert.That(toast?.GetAttribute("role")).IsEqualTo("dialog");
        await Assert.That(toast?.QuerySelector("[data-slot=toast-close]")).IsNotNull();
        await Assert.That(toast?.QuerySelector("a[data-slot=toast-action]")).IsNotNull();
        await Assert
            .That(icons)
            .IsEquivalentTo(new[] { "success", "info", "warning", "error", "loading" });
    }

    [Test]
    public async Task ProcessAsync_WhenDurationIsNegative_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Duration = TimeSpan.FromSeconds(-1);

        // Act
        Func<Task> act = async () =>
        {
            using var html = await TagHelperRenderer.RenderAsync(sut);
        };

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_WhenLimitIsLessThanOne_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Limit = 0;

        // Act
        Func<Task> act = async () =>
        {
            using var html = await TagHelperRenderer.RenderAsync(sut);
        };

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    private static ToasterTagHelper CreateSut(RenderingContext context)
    {
        context.ViewContext.TempData = new TempDataDictionary(
            context.ViewContext.HttpContext,
            new EmptyTempDataProvider()
        );

        return new ToasterTagHelper(context.Icons) { ViewContext = context.ViewContext };
    }

    private sealed class EmptyTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) =>
            new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
