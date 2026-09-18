using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.RadioGroup;

public class RadioGroupTagHelperTests
{
    [Test]
    [Arguments(RadioGroupVariant.Default)]
    [Arguments(RadioGroupVariant.ChoiceCard)]
    public async Task ProcessAsync_UnboundOptions_RendersExistingInputAndFieldStructure(
        RadioGroupVariant variant
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "delivery",
            Value = "express",
            Variant = variant,
            Label = "Delivery",
            Description = "Choose a delivery method",
            Items = [new("Standard", "standard"), new("Express", "express")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("fieldset > legend")?.TextContent)
            .IsEqualTo("Delivery");
        await Assert
            .That(
                html.QuerySelectorAll(
                    "[data-slot=radio-group] .sa-input-control-wrapper > input.sa-radiobutton.peer"
                ).Length
            )
            .IsEqualTo(2);
        await Assert
            .That(html.QuerySelectorAll(".sa-radiobutton-indicator > svg").Length)
            .IsEqualTo(2);
        await Assert
            .That(html.QuerySelector("input:checked")?.GetAttribute("value"))
            .IsEqualTo("express");
        await Assert.That(html.QuerySelectorAll("input:checked").Length).IsEqualTo(1);
        await Assert
            .That(
                html.QuerySelectorAll("[data-slot=radio-group] > label > [data-slot=field]").Length
            )
            .IsEqualTo(variant == RadioGroupVariant.ChoiceCard ? 2 : 0);
        await Assert.That(html.QuerySelectorAll("input[type=hidden]").Length).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_PrefixedModelState_OverridesModelAndItemSelection()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "Order";
        context.ViewContext.ViewData.ModelState.SetModelValue("Order.Speed", "1", "1");
        context.ViewContext.ViewData.ModelState.AddModelError("Order.Speed", "Try again");
        context.ViewContext.ClientValidationEnabled = true;
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Speed",
                context.Metadata.GetModelExplorerForType(typeof(Speed?), Speed.Standard)
            ),
            Items = [new("Standard", "Standard", true), new("Express", "Express")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("input:checked")?.GetAttribute("value"))
            .IsEqualTo("Express");
        await Assert
            .That(html.QuerySelector("input")?.GetAttribute("name"))
            .IsEqualTo("Order.Speed");
        await Assert.That(html.QuerySelectorAll("input[aria-invalid=true]").Length).IsEqualTo(2);
        await Assert
            .That(html.QuerySelector(".sa-field-error")?.TextContent)
            .IsEqualTo("Try again");
    }

    [Test]
    public async Task ProcessAsync_ChildItem_RendersDescriptionAndDisabledState()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "speed",
        };
        var child = new RadioGroupItemTagHelper
        {
            Value = "express",
            Description = "Next day",
            Disabled = true,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            sut,
            parent => TagHelperRenderer.RenderChildAsync(child, parent)
        );

        // Assert
        await Assert.That(html.QuerySelector("input[disabled]")).IsNotNull();
        await Assert
            .That(html.QuerySelector(".sa-field-description")?.TextContent)
            .IsEqualTo("Next day");
        await Assert
            .That(html.QuerySelector("input")?.GetAttribute("aria-describedby"))
            .IsEqualTo(html.QuerySelector(".sa-field-description")?.Id);
        await Assert
            .That(html.QuerySelector("label")?.GetAttribute("for"))
            .IsEqualTo(html.QuerySelector("input")?.Id);
    }

    [Test]
    public async Task ProcessAsync_BoundWithExplicitSelection_RejectsAmbiguousConfiguration()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Value = "x",
            For = new ModelExpression(
                "Speed",
                context.Metadata.GetModelExplorerForType(typeof(string), "x")
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
    public async Task ProcessAsync_MixedSources_RejectsAmbiguousConfiguration()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "speed",
            Items = [new("Standard", "standard")],
        };

        // Act
        Func<Task> act = async () =>
        {
            using var html = await TagHelperRenderer.RenderAsync(
                sut,
                parent =>
                    TagHelperRenderer.RenderChildAsync(
                        new RadioGroupItemTagHelper { Value = "other" },
                        parent
                    )
            );
        };

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ProcessAsync_RepeatedGroup_GeneratesDistinctInputIds()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "speed",
            Items = [new("Standard", "standard")],
        };

        // Act
        using var first = await TagHelperRenderer.RenderAsync(sut);
        using var second = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(first.QuerySelector("input")?.Id)
            .IsNotEqualTo(second.QuerySelector("input")?.Id);
    }

    [Test]
    [Arguments(typeof(bool?), "true", "True")]
    [Arguments(
        typeof(Guid?),
        "B58E6B78-FB5A-4A82-B94D-9B951146528F",
        "b58e6b78-fb5a-4a82-b94d-9b951146528f"
    )]
    [Arguments(typeof(int?), "0042", "42")]
    [Arguments(typeof(ulong), "18446744073709551615", "18446744073709551615")]
    [Arguments(typeof(string), "Text", "Text")]
    public async Task ProcessAsync_TypedSubmission_MatchesCanonicalOption(
        Type type,
        string submitted,
        string option
    )
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.SetModelValue("Choice", submitted, submitted);
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Choice",
                context.Metadata.GetModelExplorerForType(type, null)
            ),
            Items = [new("Option", option)],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("input:checked")?.GetAttribute("value"))
            .IsEqualTo(option);
    }

    [Test]
    public async Task ProcessAsync_MultipleSubmittedRadioValues_UsesFirstLikeMvc()
    {
        // Arrange
        using var context = new RenderingContext();
        context.ViewContext.ViewData.ModelState.SetModelValue(
            "Choice",
            new[] { "first", "second" },
            "first,second"
        );
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "Choice",
            Items = [new("First", "first"), new("Second", "second")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("input:checked").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector("input:checked")?.GetAttribute("value"))
            .IsEqualTo("first");
    }

    [Test]
    [Arguments(typeof(int?))]
    [Arguments(typeof(int))]
    public async Task ProcessAsync_ModelWithoutSelection_DoesNotAddClientValidation(Type modelType)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            For = new ModelExpression(
                "Choice",
                context.Metadata.GetModelExplorerForType(modelType, null)
            ),
            Items = [new("One", "1")],
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll("input:checked").Length).IsEqualTo(0);
        await Assert.That(html.QuerySelectorAll("input[required]").Length).IsEqualTo(0);
    }

    public enum Speed
    {
        Standard,
        Express,
    }
}
