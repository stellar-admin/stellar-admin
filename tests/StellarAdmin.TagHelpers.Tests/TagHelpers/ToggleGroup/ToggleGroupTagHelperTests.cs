using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.ToggleGroup;

public partial class ToggleGroupTagHelperTests
{
    [Test]
    public async Task ProcessAsync_MultipleCollectionBinding_ChecksBoundValuesAndRendersMarker()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Type = ToggleGroupType.Multiple,
            For = new ModelExpression(
                "Days",
                context.Metadata.GetModelExplorerForType(typeof(int[]), new[] { 2, 3 })
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                RenderItemsAsync(
                    parent,
                    new ToggleGroupItemTagHelper { Value = "1" },
                    new ToggleGroupItemTagHelper { Value = "2" },
                    new ToggleGroupItemTagHelper { Value = "3" }
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelectorAll("input[type=checkbox][name=Days]").Length)
            .IsEqualTo(3);
        await Assert.That(html.QuerySelectorAll("input:checked").Length).IsEqualTo(2);
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.GetAttribute("name"))
            .IsEqualTo("__sa_checkbox_group.Days");
    }

    [Test]
    public async Task ProcessAsync_ModelBoundInsideTemplate_NamesItemsWithTemplatePrefix()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "Entity.Cabin";
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "",
                context.Metadata.GetModelExplorerForType(typeof(string), "economy")
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent => RenderItemsAsync(parent, new ToggleGroupItemTagHelper { Value = "economy" })
        );

        // Assert
        await Assert
            .That(html.QuerySelector("input[type=radio]:checked")?.GetAttribute("name"))
            .IsEqualTo("Entity.Cabin");
    }

    [Test]
    public async Task ProcessAsync_MultipleWithEveryItemDisabled_OmitsMarker()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Type = ToggleGroupType.Multiple,
            For = new ModelExpression(
                "Days",
                context.Metadata.GetModelExplorerForType(typeof(int[]), new[] { 1 })
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                RenderItemsAsync(
                    parent,
                    new ToggleGroupItemTagHelper { Value = "1", Disabled = true }
                )
        );

        // Assert
        await Assert.That(html.QuerySelectorAll("input[type=hidden]").Length).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_SingleBooleanBinding_ChecksMatchingItem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Featured",
                context.Metadata.GetModelExplorerForType(typeof(bool), true)
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                RenderItemsAsync(
                    parent,
                    new ToggleGroupItemTagHelper { Value = "true" },
                    new ToggleGroupItemTagHelper { Value = "false" }
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelector("input:checked")?.GetAttribute("value"))
            .IsEqualTo("true");
        await Assert.That(html.QuerySelectorAll("input[type=hidden]").Length).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_PostedValues_TakePrecedenceOverModel()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.SetModelValue("Days", Array.Empty<string>(), "");
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Type = ToggleGroupType.Multiple,
            For = new ModelExpression(
                "Days",
                context.Metadata.GetModelExplorerForType(typeof(int[]), new[] { 2 })
            ),
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent => RenderItemsAsync(parent, new ToggleGroupItemTagHelper { Value = "2" })
        );

        // Assert
        await Assert.That(html.QuerySelectorAll("input:checked").Length).IsEqualTo(0);
    }

    private static async Task<string> RenderItemsAsync(
        TagHelperContext parent,
        params ToggleGroupItemTagHelper[] items
    )
    {
        var html = "";
        foreach (var item in items)
        {
            html += await TagHelperRenderer.RenderChildAsync(item, parent);
        }

        return html;
    }
}
