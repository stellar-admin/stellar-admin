using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.RichTextEditor;

public class RichTextEditorTagHelperTests
{
    [Test]
    public async Task ProcessAsync_BoundToValue_RendersTextareaToolbarAndEditingArea()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.For = new ModelExpression(
            "Bio",
            context.Metadata.GetModelExplorerForType(
                typeof(string),
                "<p>Meet <strong>Amara</strong></p>"
            )
        );
        sut.Placeholder = "Tell travellers about this guide";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-rich-text-editor[data-slot=rich-text-editor]");
        var textarea = editor?.QuerySelector(":scope > textarea");
        await Assert.That(textarea?.GetAttribute("name")).IsEqualTo("Bio");
        await Assert
            .That(textarea?.TextContent.Trim())
            .IsEqualTo("<p>Meet <strong>Amara</strong></p>");
        await Assert
            .That(textarea?.GetAttribute("placeholder"))
            .IsEqualTo("Tell travellers about this guide");
        await Assert.That(textarea?.GetAttribute("rows")).IsEqualTo("8");
        await Assert
            .That(
                editor?.QuerySelector("[data-slot=rich-text-editor-content]")?.GetAttribute("role")
            )
            .IsEqualTo("textbox");
        await Assert
            .That(editor?.QuerySelector("[data-slot=rich-text-editor-toolbar] [data-action=bold]"))
            .IsNotNull();
        await Assert.That(editor?.QuerySelector("prosekit-autocomplete-root")).IsNotNull();
        await Assert.That(editor?.QuerySelector("prosekit-table-handle-root")).IsNotNull();
        await Assert.That(editor?.QuerySelector("prosekit-inline-popover-root")).IsNotNull();
        await Assert.That(editor?.HasAttribute("data-upload-url")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_Always_RendersModuleScriptForTheEditorBundle()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.HttpContext.Request.PathBase = "/admin";
        var sut = CreateSut(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var script = html.QuerySelector("sel-rich-text-editor > script");
        await Assert.That(script?.GetAttribute("type")).IsEqualTo("module");
        await Assert
            .That(script?.GetAttribute("src"))
            .IsEqualTo("/admin/_content/StellarAdmin.UI/rich-text-editor.js");
    }

    [Test]
    public async Task ProcessAsync_WithStaticAssetCollection_UsesFingerprintedScriptUrl()
    {
        // Arrange
        using var context = new RenderingContext();
        const string label = "_content/StellarAdmin.UI/rich-text-editor.js";
        var assets = new ResourceAssetCollection([
            new ResourceAsset(
                "_content/StellarAdmin.UI/rich-text-editor.abc123.js",
                [new ResourceAssetProperty("label", label)]
            ),
        ]);
        context.ViewContext.HttpContext.SetEndpoint(
            new Endpoint(null, new EndpointMetadataCollection(new object[] { assets }), "Page")
        );
        var sut = CreateSut(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("sel-rich-text-editor > script")?.GetAttribute("src"))
            .IsEqualTo("/_content/StellarAdmin.UI/rich-text-editor.abc123.js");
    }

    [Test]
    public async Task ProcessAsync_WithUploadUrl_AddsUploadSettingsAndImageControls()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.UploadUrl = "/tours/upload";
        sut.MaxFileSize = 2 * 1024 * 1024;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-rich-text-editor");
        await Assert.That(editor?.GetAttribute("data-upload-url")).IsEqualTo("/tours/upload");
        await Assert.That(editor?.GetAttribute("data-max-file-size")).IsEqualTo("2097152");
        await Assert
            .That(editor?.GetAttribute("data-accept"))
            .IsEqualTo("image/jpeg,image/png,image/webp,image/gif,image/avif");
        await Assert
            .That(editor?.QuerySelector("[data-slot=rich-text-editor-toolbar] [data-action=image]"))
            .IsNotNull();
        await Assert.That(editor?.QuerySelector("[data-slash=image]")).IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_WithoutUploadUrl_LeavesOutImageControls()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-rich-text-editor");
        await Assert.That(editor?.QuerySelector("[data-action=image]")).IsNull();
        await Assert.That(editor?.QuerySelector("[data-slash=image]")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_ReadOnly_RendersNoToolbarAndFocusableContent()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.ReadOnly = true;
        sut.UploadUrl = "/tours/upload";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-rich-text-editor");
        await Assert.That(editor?.GetAttribute("data-readonly")).IsEqualTo("true");
        await Assert.That(editor?.HasAttribute("data-upload-url")).IsFalse();
        await Assert.That(editor?.QuerySelector("[data-slot=rich-text-editor-toolbar]")).IsNull();
        await Assert.That(editor?.QuerySelector("prosekit-autocomplete-root")).IsNull();
        await Assert.That(editor?.QuerySelector("textarea")?.HasAttribute("readonly")).IsTrue();
        var content = editor?.QuerySelector("[data-slot=rich-text-editor-content]");
        await Assert.That(content?.GetAttribute("aria-readonly")).IsEqualTo("true");
        await Assert.That(content?.GetAttribute("tabindex")).IsEqualTo("0");
    }

    [Test]
    public async Task ProcessAsync_WithLabelAndDescription_ConnectsThemToEditingAreaAndTextarea()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Label = "Guide bio";
        sut.Description = "Shown on the guide's profile.";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var label = html.QuerySelector("label");
        var description = html.QuerySelector("[data-slot=field-description]");
        var area = html.QuerySelector("[data-slot=rich-text-editor-content]");
        var textarea = html.QuerySelector("sel-rich-text-editor > textarea");
        await Assert.That(label?.Id).IsNotNull();
        await Assert.That(label?.HasAttribute("for")).IsFalse();
        await Assert.That(area?.GetAttribute("aria-labelledby")).IsEqualTo(label?.Id);
        await Assert.That(textarea?.GetAttribute("aria-labelledby")).IsEqualTo(label?.Id);
        await Assert.That(area?.GetAttribute("aria-describedby")).IsEqualTo(description?.Id);
        await Assert.That(textarea?.GetAttribute("aria-describedby")).IsEqualTo(description?.Id);
    }

    [Test]
    public async Task ProcessAsync_WithClassNames_AddsClassesToParts()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.ClassNames = new RichTextEditorClassNames
        {
            Control = "max-w-2xl",
            Editor = "prose",
            Toolbar = "sticky",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-rich-text-editor");
        await Assert.That(editor?.ClassList.Contains("max-w-2xl")).IsTrue();
        await Assert
            .That(
                editor
                    ?.QuerySelector("[data-slot=rich-text-editor-content]")
                    ?.ClassList.Contains("prose")
            )
            .IsTrue();
        await Assert
            .That(
                editor
                    ?.QuerySelector("[data-slot=rich-text-editor-toolbar]")
                    ?.ClassList.Contains("sticky")
            )
            .IsTrue();
    }

    [Test]
    public async Task ProcessAsync_RowsNotPositive_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Rows = 0;

        // Act
        var act = async () => (await TagHelperRenderer.RenderAsync(sut)).Dispose();

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_Disabled_RendersToolbarWithDisabledTextarea()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = CreateSut(context);
        sut.Disabled = true;
        sut.UploadUrl = "/tours/upload";

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-rich-text-editor");
        await Assert.That(editor?.GetAttribute("data-disabled")).IsEqualTo("true");
        await Assert.That(editor?.HasAttribute("data-readonly")).IsFalse();
        await Assert.That(editor?.HasAttribute("data-upload-url")).IsFalse();
        await Assert
            .That(editor?.QuerySelector("[data-slot=rich-text-editor-toolbar]"))
            .IsNotNull();
        await Assert.That(editor?.QuerySelector("prosekit-autocomplete-root")).IsNull();
        await Assert.That(editor?.QuerySelector("textarea")?.HasAttribute("disabled")).IsTrue();
        var content = editor?.QuerySelector("[data-slot=rich-text-editor-content]");
        await Assert.That(content?.GetAttribute("aria-disabled")).IsEqualTo("true");
        await Assert.That(content?.HasAttribute("tabindex")).IsFalse();
    }

    private static RichTextEditorTagHelper CreateSut(RenderingContext context) =>
        new(context.Generator, context.Icons) { ViewContext = context.ViewContext, Name = "Bio" };
}
