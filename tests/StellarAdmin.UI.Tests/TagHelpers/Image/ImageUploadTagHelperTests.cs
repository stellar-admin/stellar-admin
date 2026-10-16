using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.Image;

public class ImageUploadTagHelperTests
{
    [Test]
    public async Task ProcessAsync_BoundToValue_RendersHiddenInputAndFilledDropzone()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = new ModelExpression(
            "Photo",
            context.Metadata.GetModelExplorerForType(typeof(string), "photos/kyoto.jpg?v=2")
        );

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var upload = html.QuerySelector("sel-image-upload[data-slot=image-upload]");
        var hidden = upload?.QuerySelector("input[type=hidden][data-slot=image-upload-value]");
        await Assert.That(hidden?.GetAttribute("name")).IsEqualTo("Photo");
        await Assert.That(hidden?.GetAttribute("value")).IsEqualTo("photos/kyoto.jpg?v=2");
        await Assert.That(upload?.GetAttribute("data-state")).IsEqualTo("filled");
        await Assert.That(upload?.GetAttribute("data-layout")).IsEqualTo("dropzone");
        await Assert.That(upload?.GetAttribute("data-file-name")).IsEqualTo("kyoto.jpg");
        await Assert.That(upload?.GetAttribute("data-upload-url")).IsEqualTo("/upload");
        await Assert
            .That(upload?.QuerySelector(".sa-image img")?.GetAttribute("src"))
            .IsEqualTo("photos/kyoto.jpg?v=2");
        await Assert
            .That(upload?.QuerySelector("[data-slot=image-upload-drop]")?.HasAttribute("hidden"))
            .IsTrue();
        await Assert
            .That(upload?.QuerySelector("[data-slot=image-upload-media]")?.HasAttribute("hidden"))
            .IsFalse();
    }

    [Test]
    public async Task ProcessAsync_WithoutValue_ShowsDropTargetAndHint()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Accept = "image/png,image/webp";
        sut.MaxFileSize = 100 * 1024;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var upload = html.QuerySelector("sel-image-upload");
        await Assert.That(upload?.GetAttribute("data-state")).IsEqualTo("empty");
        await Assert.That(upload?.GetAttribute("data-max-file-size")).IsEqualTo("102400");
        await Assert
            .That(upload?.QuerySelector("input[type=file]")?.GetAttribute("accept"))
            .IsEqualTo("image/png,image/webp");
        await Assert
            .That(upload?.QuerySelector("[data-slot=image-upload-drop]")?.HasAttribute("hidden"))
            .IsFalse();
        await Assert
            .That(upload?.QuerySelector("[data-slot=image-upload-note]")?.TextContent)
            .IsEqualTo("PNG or WebP, up to 100 KB.");
    }

    [Test]
    public async Task ProcessAsync_WithSource_ShowsSourceInsteadOfValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Value = "kyoto.jpg";
        sut.Source = "https://cdn.voyager.travel/kyoto.jpg";
        sut.Layout = ImageUploadLayout.Compact;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector(".sa-image img")?.GetAttribute("src"))
            .IsEqualTo("https://cdn.voyager.travel/kyoto.jpg");
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.GetAttribute("value"))
            .IsEqualTo("kyoto.jpg");
    }

    [Test]
    public async Task ProcessAsync_CellLayout_RendersTriggerRemoveAndHiddenError()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Value = "kyoto.jpg";
        sut.Layout = ImageUploadLayout.Cell;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var trigger = html.QuerySelector("button[data-slot=image-upload-trigger]");
        await Assert.That(trigger?.GetAttribute("data-action")).IsEqualTo("pick");
        await Assert.That(trigger?.GetAttribute("aria-label")).IsEqualTo("Replace image");
        await Assert
            .That(html.QuerySelector("button.sa-image-upload-clear")?.GetAttribute("data-action"))
            .IsEqualTo("remove");
        var error = html.QuerySelector("[data-slot=image-upload-note]");
        await Assert.That(error?.ClassList.Contains("sa-cell-error")).IsTrue();
        await Assert.That(error?.HasAttribute("hidden")).IsTrue();
        await Assert.That(html.QuerySelector("template")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WithoutUploadUrl_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.UploadUrl = null;

        // Act
        var act = async () => await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_Disabled_DisablesActionsWithoutUploadUrl()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.UploadUrl = null;
        sut.Disabled = true;
        sut.Value = "kyoto.jpg";
        sut.Layout = ImageUploadLayout.Compact;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("sel-image-upload")?.GetAttribute("data-disabled"))
            .IsEqualTo("true");
        await Assert
            .That(html.QuerySelectorAll("button[data-action]").All(b => b.HasAttribute("disabled")))
            .IsTrue();
        await Assert
            .That(html.QuerySelector("input[type=file]")?.HasAttribute("disabled"))
            .IsTrue();
    }

    [Test]
    public async Task ProcessAsync_WithClassNames_AddsClassesToParts()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Photo";
        sut.Value = "kyoto.jpg";
        sut.ClassNames = new ImageUploadClassNames
        {
            Control = "max-w-md",
            Image = "shadow-lg",
            Label = "font-semibold",
            Note = "italic",
            Root = "col-span-2",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var upload = html.QuerySelector("sel-image-upload");
        await Assert.That(upload?.ClassList.Contains("max-w-md")).IsTrue();
        await Assert
            .That(upload?.QuerySelector(".sa-image")?.ClassList.Contains("shadow-lg"))
            .IsTrue();
        await Assert
            .That(upload?.QuerySelector("[data-slot=image-upload-note]")?.ClassName)
            .IsEqualTo("sa-image-upload-note italic");
        await Assert
            .That(html.QuerySelector("[data-slot=field]")?.ClassList.Contains("col-span-2"))
            .IsTrue();
        await Assert
            .That(
                html.QuerySelector("[data-slot=field-label]")?.ClassList.Contains("font-semibold")
            )
            .IsTrue();
    }

    private static ImageUploadTagHelper CreateSut(RenderingContext context) =>
        new(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            UploadUrl = "/upload",
        };
}
