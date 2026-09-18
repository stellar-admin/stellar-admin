using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.CheckboxGroup;

public class CheckboxGroupTagHelperTests
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
        await Assert.That(html.QuerySelector("fieldset[disabled]")).IsNotNull();
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
}
