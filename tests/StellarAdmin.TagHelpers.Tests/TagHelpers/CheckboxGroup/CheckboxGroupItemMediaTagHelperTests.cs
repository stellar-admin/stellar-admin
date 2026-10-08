using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.CheckboxGroup;

public class CheckboxGroupItemMediaTagHelperTests
{
    [Test]
    public async Task ProcessAsync_DefaultVariant_PlacesMediaBeforeTheText()
    {
        // Arrange
        using var context = new RenderingContext();
        var group = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "guides",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            group,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new CheckboxGroupItemTagHelper { Value = "ana" },
                    parent,
                    async item =>
                        await TagHelperRenderer.RenderChildAsync(
                            new CheckboxGroupItemMediaTagHelper(),
                            item,
                            _ => Task.FromResult("<span class=\"media\"></span>")
                        ) + "Ana Ribeiro"
                )
        );

        // Assert
        var label = html.QuerySelector("[data-slot=field] > label[data-slot=field-label]");
        await Assert.That(label?.FirstElementChild?.ClassName).IsEqualTo("media");
        await Assert.That(label?.TextContent).IsEqualTo("Ana Ribeiro");
        await Assert.That(html.QuerySelectorAll(".media").Length).IsEqualTo(1);
    }

    [Test]
    public async Task ProcessAsync_ChoiceCardVariant_PlacesMediaBeforeTheContent()
    {
        // Arrange
        using var context = new RenderingContext();
        var group = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "guides",
            Variant = CheckboxGroupVariant.ChoiceCard,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            group,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new CheckboxGroupItemTagHelper { Value = "ana", Description = "Lisbon tours" },
                    parent,
                    async item =>
                        await TagHelperRenderer.RenderChildAsync(
                            new CheckboxGroupItemMediaTagHelper(),
                            item,
                            _ => Task.FromResult("<span class=\"media\"></span>")
                        ) + "Ana Ribeiro"
                )
        );

        // Assert
        var field = html.QuerySelector("label > [data-slot=field]");
        await Assert.That(field?.FirstElementChild?.ClassName).IsEqualTo("media");
        await Assert.That(field?.Children[1].GetAttribute("data-slot")).IsEqualTo("field-content");
        await Assert
            .That(html.QuerySelector(".sa-field-title")?.TextContent)
            .IsEqualTo("Ana Ribeiro");
    }

    [Test]
    public async Task ProcessAsync_SecondMedia_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var group = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "guides",
        };

        // Act
        var act = async () =>
        {
            using var _ = await TagHelperRenderer.RenderAsync(
                group,
                parent =>
                    TagHelperRenderer.RenderChildAsync(
                        new CheckboxGroupItemTagHelper { Value = "ana" },
                        parent,
                        async item =>
                            await TagHelperRenderer.RenderChildAsync(
                                new CheckboxGroupItemMediaTagHelper(),
                                item
                            )
                            + await TagHelperRenderer.RenderChildAsync(
                                new CheckboxGroupItemMediaTagHelper(),
                                item
                            )
                    )
            );
        };

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
