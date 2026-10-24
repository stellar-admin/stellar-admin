using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.CheckboxGroup;

public partial class CheckboxGroupTagHelperTests
{
    [Test]
    [Arguments(CheckboxGroupVariant.Default)]
    [Arguments(CheckboxGroupVariant.ChoiceCard)]
    public async Task ProcessAsync_CollectionBinding_RendersRepeatedNamesAndSeparateMarker(
        CheckboxGroupVariant variant
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Variant = variant,
            For = new ModelExpression(
                "Roles",
                context.Metadata.GetModelExplorerForType(typeof(int[]), new[] { 2, 3 })
            ),
            Items = [new("One", "1"), new("Two", "2"), new("Three", "3")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelectorAll("input[type=checkbox][name=Roles]").Length)
            .IsEqualTo(3);
        await Assert.That(html.QuerySelectorAll("input:checked").Length).IsEqualTo(2);
        await Assert
            .That(
                html.QuerySelectorAll(
                    "input[required], [data-choice-min], [data-choice-max]"
                ).Length
            )
            .IsEqualTo(0);
        await Assert
            .That(html.QuerySelectorAll("input[type=hidden][name=Roles]").Length)
            .IsEqualTo(0);
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.GetAttribute("name"))
            .IsEqualTo("__sa_checkbox_group.Roles");
        await Assert
            .That(html.QuerySelectorAll(".sa-checkbox-indicator > svg").Length)
            .IsEqualTo(3);
        await Assert
            .That(html.QuerySelector("[data-slot=field] > [data-slot=field-label]")?.TextContent)
            .IsEqualTo("Roles");
        await Assert
            .That(html.QuerySelector("[data-slot=checkbox-group]")?.GetAttribute("role"))
            .IsEqualTo("group");
        await Assert
            .That(
                html.QuerySelectorAll(
                    "[data-slot=checkbox-group] > label > [data-slot=field]"
                ).Length
            )
            .IsEqualTo(variant == CheckboxGroupVariant.ChoiceCard ? 3 : 0);
    }

    [Test]
    public async Task ProcessAsync_EmptyModelState_DoesNotRestoreModelSelection()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.SetModelValue("Roles", Array.Empty<string>(), "");
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Roles",
                context.Metadata.GetModelExplorerForType(typeof(int[]), new[] { 2 })
            ),
            Items = [new("Two", "2", true)],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("input:checked").Length).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_DisabledGroup_DisablesOptionsAndOmitsMarker()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "roles",
            Values = new[] { "admin" },
            Disabled = true,
            Items = [new("Administrator", "admin")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=checkbox-group][aria-disabled=true]"))
            .IsNotNull();
        await Assert
            .That(html.QuerySelector("input[type=checkbox][disabled][checked]"))
            .IsNotNull();
        await Assert.That(html.QuerySelectorAll("input[type=hidden]").Length).IsEqualTo(0);
    }

    [Test]
    [Arguments(typeof(bool))]
    [Arguments(typeof(byte[]))]
    [Arguments(typeof(int?[]))]
    [Arguments(typeof(object[]))]
    public async Task ProcessAsync_UnsupportedModelType_Throws(Type modelType)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Roles",
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
    public async Task ProcessAsync_WithColumns_WritesColumnsOnGroup()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "amenities",
            ColumnsMedium = 2,
            Items = [new("Wi-Fi", "wifi"), new("Lounge", "lounge"), new("Transfer", "transfer")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var group = html.QuerySelector("[data-slot=checkbox-group]");
        await Assert.That(group?.GetAttribute("data-columns")).IsEqualTo("multiple");
        await Assert.That(group?.HasAttribute("data-flow")).IsFalse();
        await Assert.That(group?.GetAttribute("style")).IsEqualTo("--sa-cols-md:2;--sa-cols-lg:2");
    }

    [Test]
    public async Task ProcessAsync_WithColumnsFlowingDown_WritesRowsPerBreakpoint()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "amenities",
            ColumnsSmall = 2,
            ColumnsLarge = 3,
            Flow = CheckboxGroupFlow.Down,
            Items =
            [
                new("Wi-Fi", "wifi"),
                new("Lounge", "lounge"),
                new("Priority", "priority"),
                new("Baggage", "baggage"),
                new("Transfer", "transfer"),
            ],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var group = html.QuerySelector("[data-slot=checkbox-group]");
        await Assert.That(group?.GetAttribute("data-flow")).IsEqualTo("down");
        await Assert
            .That(group?.GetAttribute("style"))
            .IsEqualTo(
                "--sa-cols-sm:2;--sa-cols-md:2;--sa-cols-lg:3;--sa-rows:5;--sa-rows-sm:3;--sa-rows-md:3;--sa-rows-lg:2"
            );
    }

    [Test]
    public async Task ProcessAsync_WithOneColumn_LeavesOutColumns()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "amenities",
            Columns = 1,
            Flow = CheckboxGroupFlow.Down,
            Items = [new("Wi-Fi", "wifi"), new("Lounge", "lounge")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var group = html.QuerySelector("[data-slot=checkbox-group]");
        await Assert.That(group?.HasAttribute("data-columns")).IsFalse();
        await Assert.That(group?.HasAttribute("data-flow")).IsFalse();
        await Assert.That(group?.HasAttribute("style")).IsFalse();
    }

    [Test]
    [Arguments(0, null)]
    [Arguments(13, null)]
    [Arguments(1, 13)]
    public async Task ProcessAsync_WithInvalidColumns_ThrowsInvalidOperationException(
        int columns,
        int? columnsMedium
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "amenities",
            Columns = columns,
            ColumnsMedium = columnsMedium,
            Items = [new("Wi-Fi", "wifi")],
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
    public async Task ProcessAsync_ReadOnlyGroup_MarksOptionsReadonlyAndKeepsMarker()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "roles",
            Values = new[] { "admin" },
            ReadOnly = true,
            Items = [new("Administrator", "admin"), new("Editor", "editor")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(
                html.QuerySelectorAll(
                    "input[type=checkbox][readonly][aria-readonly=true][onclick='return false']"
                ).Length
            )
            .IsEqualTo(2);
        await Assert.That(html.QuerySelectorAll("input:disabled").Length).IsEqualTo(0);
        await Assert
            .That(html.QuerySelector("[data-slot=checkbox-group]")?.HasAttribute("aria-readonly"))
            .IsFalse();
        await Assert
            .That(html.QuerySelector("input[type=hidden]")?.GetAttribute("name"))
            .IsEqualTo("__sa_checkbox_group.roles");
    }
}
