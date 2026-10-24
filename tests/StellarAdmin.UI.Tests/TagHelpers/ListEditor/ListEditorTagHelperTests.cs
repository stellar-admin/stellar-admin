using System.ComponentModel.DataAnnotations;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.ListEditor;

public class ListEditorTagHelperTests
{
    [Test]
    public async Task ProcessAsync_BoundList_RendersIndexedRowsTemplateAndMarker()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-collection-editor[data-slot=list-editor]");
        await Assert.That(editor).IsNotNull();
        await Assert
            .That(
                html.QuerySelectorAll("ol > li input[type=text]")
                    .Select(input => $"{input.GetAttribute("name")}={input.GetAttribute("value")}")
            )
            .IsEquivalentTo(
                new[] { "Highlights[0]=Fado in Alfama", "Highlights[1]=Pastéis de nata" }
            );
        await Assert.That(editor!.GetAttribute("data-name")).IsEqualTo("Highlights");
        await Assert.That(html.QuerySelector("template")).IsNotNull();
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.GetAttribute("name"))
            .IsEqualTo("__sa_checkbox_group.Highlights");
        await Assert
            .That(html.QuerySelector(".sa-collection-editor-add")?.GetAttribute("commandfor"))
            .IsEqualTo(editor.Id);
        await Assert
            .That(editor.GetAttribute("aria-labelledby"))
            .IsEqualTo(html.QuerySelector("[data-slot=field-label]")?.Id);
        await Assert
            .That(html.QuerySelector(".sa-collection-editor-empty")?.HasAttribute("hidden"))
            .IsTrue();
    }

    [Test]
    public async Task ProcessAsync_LengthAttribute_SetsItemLimitsAndCount()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-collection-editor")!;
        await Assert.That(editor.GetAttribute("data-min-items")).IsEqualTo("1");
        await Assert.That(editor.GetAttribute("data-max-items")).IsEqualTo("8");
        await Assert
            .That(html.QuerySelector(".sa-collection-editor-count")?.TextContent)
            .IsEqualTo("2 of 8");
        await Assert.That(editor.GetAttribute("data-texts")).Contains("Add at least one item.");
    }

    [Test]
    public async Task ProcessAsync_ItemSettings_RendersBrowserChecksOnEachInput()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
            Pattern = "[A-Z]{3}",
            PatternMessage = "Use a three-letter code.",
            MaxLength = 3,
            Casing = ListEditorCasing.Upper,
            Placeholder = "LIS",
            Prefix = "✈",
            Unique = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var input = html.QuerySelector("ol > li input")!;
        await Assert.That(input.GetAttribute("pattern")).IsEqualTo("[A-Z]{3}");
        await Assert
            .That(input.GetAttribute("data-pattern-message"))
            .IsEqualTo("Use a three-letter code.");
        await Assert.That(input.GetAttribute("maxlength")).IsEqualTo("3");
        await Assert.That(input.GetAttribute("data-casing")).IsEqualTo("upper");
        await Assert.That(input.GetAttribute("placeholder")).IsEqualTo("LIS");
        await Assert
            .That(
                html.QuerySelector(
                    "ol > li .sa-collection-editor-affix[data-side=start]"
                )?.TextContent
            )
            .IsEqualTo("✈");
        await Assert
            .That(html.QuerySelector("sel-collection-editor")?.HasAttribute("data-unique"))
            .IsTrue();
    }

    [Test]
    [Arguments(typeof(int[]), "1")]
    [Arguments(typeof(List<decimal>), "any")]
    public async Task ProcessAsync_NumberList_RendersNumberInputsWithStep(Type type, string step)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression("Days", context.Metadata.GetModelExplorerForType(type, null)),
            Min = 1,
            Max = 31,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var input = ((IHtmlTemplateElement)html.QuerySelector("template")!)
            .Content.QuerySelector("input")!
            .OuterHtml;
        await Assert.That(input).Contains("type=\"number\"");
        await Assert.That(input).Contains($"step=\"{step}\"");
        await Assert.That(input).Contains("min=\"1\"");
        await Assert.That(input).Contains("max=\"31\"");
    }

    [Test]
    [Arguments(typeof(int[]), null, "end")]
    [Arguments(typeof(List<string>), null, null)]
    [Arguments(typeof(int[]), ListEditorAlign.Start, null)]
    [Arguments(typeof(List<string>), ListEditorAlign.End, "end")]
    public async Task ProcessAsync_Align_AlignsNumbersToTheEndByDefault(
        Type type,
        ListEditorAlign? align,
        string? expected
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression("Days", context.Metadata.GetModelExplorerForType(type, null)),
            Align = align,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(
                ((IHtmlTemplateElement)html.QuerySelector("template")!)
                    .Content.QuerySelector(".sa-collection-editor-cell")
                    ?.GetAttribute("data-align")
            )
            .IsEqualTo(expected);
    }

    [Test]
    public async Task ProcessAsync_PostedRows_RendersAttemptedValuesAndItemErrors()
    {
        // Arrange
        using var context = new RenderingContext();
        var state = context.ViewContext.ViewData.ModelState;
        state.SetModelValue("Highlights[0]", "Tram 28", "Tram 28");
        state.SetModelValue("Highlights[1]", "Tram 28", "Tram 28");
        state.AddModelError("Highlights[1]", "Tram 28 is already in the list.");
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(
                html.QuerySelectorAll("ol > li input")
                    .Select(input => input.GetAttribute("value") ?? "")
            )
            .IsEquivalentTo(new[] { "Tram 28", "Tram 28" });
        var invalid = html.QuerySelector("ol > li[data-invalid]")!;
        var error = invalid.QuerySelector(".sa-collection-editor-item-error")!;
        await Assert.That(error.TextContent).IsEqualTo("Tram 28 is already in the list.");
        await Assert
            .That(
                invalid.QuerySelector("input[aria-invalid=true]")?.GetAttribute("aria-describedby")
            )
            .IsEqualTo(error.Id);
    }

    [Test]
    public async Task ProcessAsync_ItemErrorWithoutPostedRows_RendersModelRowsWithTheError()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.AddModelError("Highlights[1]", "Too long.");
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("ol > li").Length).IsEqualTo(2);
        await Assert
            .That(
                html.QuerySelector(
                    "ol > li:nth-child(2) .sa-collection-editor-item-error"
                )?.TextContent
            )
            .IsEqualTo("Too long.");
    }

    [Test]
    public async Task ProcessAsync_EmptySubmission_RendersNoRows()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.SetModelValue(
            "Highlights",
            Array.Empty<string>(),
            ""
        );
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
            EmptyText = "No highlights yet",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("ol > li").Length).IsEqualTo(0);
        await Assert.That(html.QuerySelector("ol")?.HasAttribute("hidden")).IsTrue();
        var empty = html.QuerySelector(".sa-collection-editor-empty")!;
        await Assert.That(empty.HasAttribute("hidden")).IsFalse();
        await Assert.That(empty.TextContent).IsEqualTo("No highlights yet");
    }

    [Test]
    public async Task ProcessAsync_ReadOnly_RendersReadonlyInputsAndMarkerWithoutActions()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
            ReadOnly = true,
            Sortable = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("sel-collection-editor")?.HasAttribute("data-readonly"))
            .IsTrue();
        await Assert
            .That(html.QuerySelector("sel-collection-editor")?.HasAttribute("data-sortable"))
            .IsFalse();
        await Assert.That(html.QuerySelectorAll("button, template").Length).IsEqualTo(0);
        await Assert
            .That(
                html.QuerySelectorAll("input[type=text][readonly]")
                    .Select(input => input.GetAttribute("name") + "=" + input.GetAttribute("value"))
            )
            .IsEquivalentTo(
                new[] { "Highlights[0]=Fado in Alfama", "Highlights[1]=Pastéis de nata" }
            );
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.GetAttribute("name"))
            .IsEqualTo("__sa_checkbox_group.Highlights");
    }

    [Test]
    public async Task ProcessAsync_Disabled_DisablesControlsAndMarkerButNotTemplate()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
            Disabled = true,
            Sortable = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("sel-collection-editor")?.HasAttribute("disabled"))
            .IsTrue();
        await Assert
            .That(
                html.QuerySelectorAll("input, button")
                    .All(control => control.HasAttribute("disabled"))
            )
            .IsTrue();
        await Assert.That(html.QuerySelectorAll("input[type=text]").Length).IsEqualTo(2);
        await Assert.That(html.QuerySelector("template")?.InnerHtml.Contains("disabled")).IsFalse();
    }

    [Test]
    public async Task ProcessAsync_SortableNumbered_RendersHandleWithNumber()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
            Sortable = true,
            Numbered = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var handle = html.QuerySelector("ol > li:nth-child(2) .sa-collection-editor-handle")!;
        await Assert.That(handle.GetAttribute("aria-label")).IsEqualTo("Move item 2");
        await Assert
            .That(handle.QuerySelector(".sa-collection-editor-number")?.TextContent)
            .IsEqualTo("2");
        await Assert.That(handle.QuerySelector("svg")).IsNotNull();
    }

    [Test]
    public async Task ProcessAsync_EmptyItemAttributes_RendersThemAsUnset()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
            Pattern = "",
            PatternMessage = "",
            Placeholder = "",
            Prefix = "",
            Suffix = "",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var input = html.QuerySelector("ol > li input[data-part='value']")!;
        await Assert.That(input.HasAttribute("pattern")).IsFalse();
        await Assert.That(input.HasAttribute("data-pattern-message")).IsFalse();
        await Assert.That(input.HasAttribute("placeholder")).IsFalse();
        await Assert.That(html.QuerySelector(".sa-collection-editor-affix")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_Texts_ReplaceTheDefaultTexts()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, new Trip()),
            Texts = new ListEditorTexts
            {
                Count = "{count} de 8",
                Remove = "Remover item {n}",
                TooFew = "Adicione pelo menos um item.",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-collection-editor")!;
        await Assert
            .That(html.QuerySelector(".sa-collection-editor-count")?.TextContent)
            .IsEqualTo("2 de 8");
        await Assert
            .That(
                html.QuerySelector("ol > li .sa-collection-editor-remove")
                    ?.GetAttribute("aria-label")
            )
            .IsEqualTo("Remover item 1");
        await Assert
            .That(editor.GetAttribute("data-texts"))
            .Contains("Adicione pelo menos um item.");
        await Assert.That(editor.GetAttribute("data-texts")).Contains("Item {n}");
    }

    [Test]
    [Arguments(typeof(Guid[]))]
    [Arguments(typeof(DayOfWeek[]))]
    [Arguments(typeof(int?[]))]
    [Arguments(typeof(string))]
    public async Task ProcessAsync_UnsupportedModelType_Throws(Type modelType)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ListEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Values",
                context.Metadata.GetModelExplorerForType(modelType, null)
            ),
        };

        // Act
        Func<Task> act = async () =>
        {
            using var html = await TagHelperRenderer.RenderAsync(sut);
        };

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    private static ModelExpression For(RenderingContext context, Trip trip) =>
        new(
            "Highlights",
            context
                .Metadata.GetModelExplorerForType(typeof(Trip), trip)
                .GetExplorerForProperty("Highlights")
        );

    public class Trip
    {
        [Length(1, 8)]
        public List<string> Highlights { get; set; } = ["Fado in Alfama", "Pastéis de nata"];
    }
}
