using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.ToggleGroup;

public partial class ToggleGroupTagHelperTests
{
    [Test]
    [Arguments(null, "radio")]
    [Arguments(false, "radio")]
    [Arguments(true, "checkbox")]
    public async Task ProcessAsync_WithMultiple_RendersMatchingInputType(
        bool? multiple,
        string expectedType
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "cabin",
            Multiple = multiple,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                RenderItemsAsync(
                    parent,
                    new ToggleGroupItemTagHelper { Value = "economy" },
                    new ToggleGroupItemTagHelper { Value = "business" }
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelectorAll($"input[type={expectedType}][name=cabin]").Length)
            .IsEqualTo(2);
    }

    [Test]
    public async Task ProcessAsync_MultipleCollectionBinding_ChecksBoundValuesAndRendersMarker()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Multiple = true,
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
            Multiple = true,
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
            Multiple = true,
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

    [Test]
    public async Task ProcessAsync_UnboundWithValue_ChecksMatchingItem()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "cabin",
            Value = "business",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                RenderItemsAsync(
                    parent,
                    new ToggleGroupItemTagHelper { Value = "economy" },
                    new ToggleGroupItemTagHelper { Value = "business" }
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelector("input:checked")?.GetAttribute("value"))
            .IsEqualTo("business");
    }

    [Test]
    public async Task ProcessAsync_UnboundMultipleWithValues_ChecksMatchingItems()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "days",
            Multiple = true,
            Values = ["mon", "wed"],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                RenderItemsAsync(
                    parent,
                    new ToggleGroupItemTagHelper { Value = "mon" },
                    new ToggleGroupItemTagHelper { Value = "tue" },
                    new ToggleGroupItemTagHelper { Value = "wed" }
                )
        );

        // Assert
        await Assert
            .That(
                html.QuerySelectorAll("input:checked")
                    .Select(input => input.GetAttribute("value") ?? "")
            )
            .IsEquivalentTo(["mon", "wed"]);
    }

    [Test]
    public async Task ProcessAsync_ItemSelected_OverridesGroupValue()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "cabin",
            Value = "business",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                RenderItemsAsync(
                    parent,
                    new ToggleGroupItemTagHelper { Value = "economy", Selected = true },
                    new ToggleGroupItemTagHelper { Value = "business", Selected = false }
                )
        );

        // Assert
        await Assert
            .That(html.QuerySelector("input:checked")?.GetAttribute("value"))
            .IsEqualTo("economy");
    }

    [Test]
    [Arguments(false, false)]
    [Arguments(true, true)]
    public async Task ProcessAsync_ValueForWrongMode_Throws(bool multiple, bool useValue)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "cabin",
            Multiple = multiple,
            Value = useValue ? "economy" : null,
            Values = useValue ? null : ["economy"],
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
    public async Task ProcessAsync_ValueWithAspFor_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Value = "economy",
            For = new ModelExpression(
                "Cabin",
                context.Metadata.GetModelExplorerForType(typeof(string), "economy")
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
    public async Task ProcessAsync_MultipleReadOnly_MarksCheckboxesReadonlyAndRendersMarker()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new ToggleGroupTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "Days",
            Multiple = true,
            ReadOnly = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent =>
                RenderItemsAsync(
                    parent,
                    new ToggleGroupItemTagHelper { Value = "mon" },
                    new ToggleGroupItemTagHelper { Value = "tue", Disabled = true }
                )
        );

        // Assert
        await Assert
            .That(
                html.QuerySelectorAll(
                    "input[type=checkbox][readonly][aria-readonly=true][onclick='return false']"
                ).Length
            )
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelector("input[type=checkbox]:disabled")?.HasAttribute("readonly"))
            .IsFalse();
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.GetAttribute("name"))
            .IsEqualTo("__sa_checkbox_group.Days");
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
