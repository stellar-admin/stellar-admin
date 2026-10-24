using System.ComponentModel.DataAnnotations;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.DictionaryEditor;

public class DictionaryEditorTagHelperTests
{
    [Test]
    public async Task ProcessAsync_TextDictionary_RendersKeyAndValueInputsWithColumnHeads()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.LocalNames)),
            KeyLabel = "Language",
            ValueLabel = "Local name",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("sel-collection-editor")?.GetAttribute("data-slot"))
            .IsEqualTo("dictionary-editor");
        await Assert
            .That(
                html.QuerySelectorAll("ol > li input")
                    .Select(input => $"{input.GetAttribute("name")}={input.GetAttribute("value")}")
            )
            .IsEquivalentTo(
                new[]
                {
                    "LocalNames[0].Key=pt",
                    "LocalNames[0].Value=Passeio por Lisboa",
                    "LocalNames[1].Key=es",
                    "LocalNames[1].Value=Ruta por Lisboa",
                }
            );
        await Assert
            .That(
                html.QuerySelectorAll(".sa-collection-editor-head > span")
                    .Select(span => span.TextContent)
            )
            .IsEquivalentTo(new[] { "Language", "Local name", "" });
        await Assert
            .That(html.QuerySelector("ol > li [data-part=value]")?.GetAttribute("aria-label"))
            .IsEqualTo("Local name 1");
        await Assert
            .That(html.QuerySelector("sel-collection-editor")?.HasAttribute("data-unique"))
            .IsTrue();
    }

    [Test]
    public async Task ProcessAsync_EnumKey_RendersSelectOfMembersWithUsedKeysDisabled()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.SeasonalPrices)),
            ValuePrefix = "€",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var select = html.QuerySelector("ol > li:first-child select[data-part=key]")!;
        await Assert.That(select.GetAttribute("name")).IsEqualTo("SeasonalPrices[0].Key");
        await Assert
            .That(
                select
                    .QuerySelectorAll("option")
                    .Select(option => $"{option.GetAttribute("value")}:{option.TextContent}")
            )
            .IsEquivalentTo(
                new[] { ":", "Low:Low season", "High:High season", "Peak:Peak (holidays)" }
            );
        await Assert
            .That(select.QuerySelector("option[selected]")?.GetAttribute("value"))
            .IsEqualTo("Low");
        await Assert
            .That(
                select
                    .QuerySelectorAll("option[disabled]")
                    .Select(option => option.GetAttribute("value") ?? "")
            )
            .IsEquivalentTo(new[] { "High" });
        await Assert
            .That(html.QuerySelector("ol > li input[data-part=value]")?.GetAttribute("value"))
            .IsEqualTo("49.5");
        await Assert
            .That(html.QuerySelector("sel-collection-editor")?.GetAttribute("data-max-items"))
            .IsEqualTo("3");
        await Assert
            .That(
                html.QuerySelector(
                    "ol > li .sa-collection-editor-affix[data-side=start]"
                )?.TextContent
            )
            .IsEqualTo("€");
    }

    [Test]
    public async Task ProcessAsync_NumberValue_AlignsValueColumnToTheEnd()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.BaggageAllowance)),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(
                html.QuerySelectorAll(".sa-collection-editor-head > span")
                    .Select(span => span.GetAttribute("data-align") ?? "")
            )
            .IsEquivalentTo(new[] { "", "end", "" });
        await Assert
            .That(
                html.QuerySelectorAll("ol > li .sa-collection-editor-cell")
                    .Select(cell => cell.GetAttribute("data-align") ?? "")
            )
            .IsEquivalentTo(new[] { "", "end" });
    }

    [Test]
    public async Task ProcessAsync_ValueAlignStart_LeavesNumberValueAtTheStart()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.BaggageAllowance)),
            ValueAlign = DictionaryEditorAlign.Start,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("[data-align]").Length).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_KeyAndValueItems_RendersSelects()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.LocalNames)),
            KeyItems = [new("Portuguese", "pt"), new("Spanish", "es"), new("French", "fr")],
            ValueItems = [new("Inês Duarte", "ines")],
            ValuePlaceholder = "Choose a guide",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("ol > li select").Length).IsEqualTo(4);
        await Assert
            .That(
                ((IHtmlTemplateElement)html.QuerySelector("template")!)
                    .Content.QuerySelector("select[data-part=value] option")
                    ?.TextContent
            )
            .IsEqualTo("Choose a guide");
        await Assert
            .That(html.QuerySelector("sel-collection-editor")?.GetAttribute("data-max-items"))
            .IsEqualTo("3");
    }

    [Test]
    public async Task ProcessAsync_PostedRows_RendersAttemptedValuesAndValueError()
    {
        // Arrange
        using var context = new RenderingContext();
        var state = context.ViewContext.ViewData.ModelState;
        state.SetModelValue("BaggageAllowance[0].Key", "Economy", "Economy");
        state.SetModelValue("BaggageAllowance[0].Value", "heavy", "heavy");
        state.AddModelError("BaggageAllowance[0].Value", "Enter a number.");
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.BaggageAllowance)),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("ol > li").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector("ol > li [data-part=value]")?.GetAttribute("aria-invalid"))
            .IsEqualTo("true");
        await Assert
            .That(html.QuerySelector("ol > li [data-part=key]")?.HasAttribute("aria-invalid"))
            .IsFalse();
        await Assert
            .That(html.QuerySelector(".sa-collection-editor-item-error")?.TextContent)
            .IsEqualTo("Enter a number.");
    }

    [Test]
    public async Task ProcessAsync_ReadOnly_RendersItemTextWithoutControls()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.SeasonalPrices)),
            ReadOnly = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelectorAll("input, select, button, template").Length)
            .IsEqualTo(0);
        await Assert
            .That(
                html.QuerySelectorAll(".sa-collection-editor-text").Select(text => text.TextContent)
            )
            .IsEquivalentTo(new[] { "Low season", "49.5", "High season", "69" });
    }

    [Test]
    [Arguments(typeof(Dictionary<int, string>))]
    [Arguments(typeof(Dictionary<string, Guid>))]
    [Arguments(typeof(IReadOnlyDictionary<string, string>))]
    [Arguments(typeof(List<string>))]
    public async Task ProcessAsync_UnsupportedModelType_Throws(Type modelType)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
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

    [Test]
    public async Task ProcessAsync_EmptyKeyAndValueAttributes_RendersThemAsUnset()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.LocalNames)),
            KeyPattern = "",
            KeyPatternMessage = "",
            KeyPlaceholder = "",
            ValuePattern = "",
            ValuePatternMessage = "",
            ValuePlaceholder = "",
            ValuePrefix = "",
            ValueSuffix = "",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var inputs = html.QuerySelectorAll("ol > li:first-child input[data-part]");
        await Assert.That(inputs.Length).IsEqualTo(2);
        await Assert.That(inputs.Any(input => input.HasAttribute("pattern"))).IsFalse();
        await Assert
            .That(inputs.Any(input => input.HasAttribute("data-pattern-message")))
            .IsFalse();
        await Assert.That(inputs.Any(input => input.HasAttribute("placeholder"))).IsFalse();
        await Assert.That(html.QuerySelector(".sa-collection-editor-affix")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_Texts_ReplaceTheDefaultTexts()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new DictionaryEditorTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = For(context, nameof(Trip.LocalNames)),
            KeyLabel = "Language",
            Texts = new DictionaryEditorTexts
            {
                KeyRequired = "Escolha um idioma.",
                Remove = "Remover linha {n}",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var editor = html.QuerySelector("sel-collection-editor")!;
        await Assert
            .That(
                html.QuerySelector("ol > li .sa-collection-editor-remove")
                    ?.GetAttribute("aria-label")
            )
            .IsEqualTo("Remover linha 1");
        await Assert.That(editor.GetAttribute("data-texts")).Contains("Escolha um idioma.");
        await Assert.That(editor.GetAttribute("data-texts")).Contains("Value is required.");
    }

    private static ModelExpression For(RenderingContext context, string property) =>
        new(
            property,
            context
                .Metadata.GetModelExplorerForType(typeof(Trip), new Trip())
                .GetExplorerForProperty(property)
        );

    public class Trip
    {
        public Dictionary<string, int> BaggageAllowance { get; set; } = new() { ["Economy"] = 23 };

        public Dictionary<string, string> LocalNames { get; set; } =
            new() { ["pt"] = "Passeio por Lisboa", ["es"] = "Ruta por Lisboa" };

        public Dictionary<Season, decimal> SeasonalPrices { get; set; } =
            new() { [Season.Low] = 49.5m, [Season.High] = 69m };
    }

    public enum Season
    {
        [Display(Name = "Low season")]
        Low,

        [Display(Name = "High season")]
        High,

        [Display(Name = "Peak (holidays)")]
        Peak,
    }
}
