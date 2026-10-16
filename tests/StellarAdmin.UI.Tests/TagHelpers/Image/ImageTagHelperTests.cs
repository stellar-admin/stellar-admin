using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Image;

public class ImageTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithSource_RendersLightboxButtonAndTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Source = "/images/kyoto.jpg";
        sut.Alt = "Kyoto";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var image = html.QuerySelector("button.sa-image[data-slot=image]");
        await Assert.That(image?.GetAttribute("aria-label")).IsEqualTo("Kyoto");
        await Assert.That(image?.GetAttribute("data-interaction")).IsEqualTo("lightbox");
        await Assert.That(image?.GetAttribute("data-state")).IsEqualTo("loading");
        await Assert.That(image?.GetAttribute("data-variant")).IsEqualTo("thumbnail");
        await Assert.That(image?.GetAttribute("data-size")).IsEqualTo("sm");
        await Assert.That(image?.GetAttribute("data-aspect-ratio")).IsEqualTo("square");
        await Assert
            .That(image?.QuerySelector("img")?.GetAttribute("src"))
            .IsEqualTo("/images/kyoto.jpg");
        await Assert
            .That(html.QuerySelectorAll("template[data-slot=image-lightbox-template]").Length)
            .IsEqualTo(1);
    }

    [Test]
    public async Task ProcessAsync_WithoutSource_RendersPlaceholder()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var image = html.QuerySelector("span.sa-image");
        await Assert.That(image?.GetAttribute("role")).IsEqualTo("img");
        await Assert.That(image?.GetAttribute("data-state")).IsEqualTo("empty");
        await Assert.That(image?.HasAttribute("data-interaction")).IsFalse();
        await Assert.That(image?.QuerySelector("img")).IsNull();
        await Assert.That(image?.QuerySelector("[data-slot=image-placeholder] svg")).IsNotNull();
        await Assert.That(html.QuerySelector("template")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_Figure_DefaultsToMediumAndOriginalWithFallbackText()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Source = "/images/kyoto.jpg";
        sut.Variant = ImageVariant.Figure;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var image = html.QuerySelector(".sa-image");
        await Assert.That(image?.GetAttribute("data-size")).IsEqualTo("md");
        await Assert.That(image?.GetAttribute("data-aspect-ratio")).IsEqualTo("original");
        await Assert
            .That(image?.QuerySelector("[data-slot=image-error]")?.TextContent)
            .IsEqualTo("Couldn't load the image");
    }

    [Test]
    public async Task ProcessAsync_WithoutInteraction_RendersImageWithAlt()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Source = "/images/kyoto.jpg";
        sut.Alt = "Kyoto";
        sut.Interaction = ImageInteraction.None;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var image = html.QuerySelector("span.sa-image");
        await Assert.That(image?.HasAttribute("data-interaction")).IsFalse();
        await Assert.That(image?.QuerySelector("img")?.GetAttribute("alt")).IsEqualTo("Kyoto");
        await Assert.That(html.QuerySelector("template")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_SecondLightboxImageInRequest_DoesNotRepeatTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var first = CreateSut(context);
        first.Source = "/images/kyoto.jpg";
        var sut = CreateSut(context);
        sut.Source = "/images/lisbon.jpg";
        using var _ = await TagHelperRenderer.RenderAsync(first);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelector("button.sa-image")).IsNotNull();
        await Assert.That(html.QuerySelector("template")).IsNull();
    }

    private static ImageTagHelper CreateSut(RenderingContext context) =>
        new(context.Icons) { ViewContext = context.ViewContext };
}
