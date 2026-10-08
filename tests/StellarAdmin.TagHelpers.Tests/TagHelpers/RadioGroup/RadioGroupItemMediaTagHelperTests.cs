using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.RadioGroup;

public class RadioGroupItemMediaTagHelperTests
{
    [Test]
    public async Task ProcessAsync_DescribedOption_PlacesMediaInTheLabel()
    {
        // Arrange
        using var context = new RenderingContext();
        var group = new RadioGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "transport",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(
            group,
            parent =>
                TagHelperRenderer.RenderChildAsync(
                    new RadioGroupItemTagHelper { Value = "train", Description = "City to city" },
                    parent,
                    async item =>
                        await TagHelperRenderer.RenderChildAsync(
                            new RadioGroupItemMediaTagHelper(),
                            item,
                            _ => Task.FromResult("<svg class=\"media\"></svg>")
                        ) + "Train"
                )
        );

        // Assert
        var label = html.QuerySelector("[data-slot=field-content] > label[data-slot=field-label]");
        await Assert.That(label?.FirstElementChild?.ClassName).IsEqualTo("media");
        await Assert
            .That(html.QuerySelector("[data-slot=field-description]")?.TextContent)
            .IsEqualTo("City to city");
    }

    [Test]
    public async Task ProcessAsync_OutsideARadioGroupItem_Throws()
    {
        // Arrange
        using var context = new RenderingContext();
        var group = new CheckboxGroupTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "transport",
        };

        // Act
        var act = async () =>
        {
            using var _ = await TagHelperRenderer.RenderAsync(
                group,
                parent =>
                    TagHelperRenderer.RenderChildAsync(
                        new CheckboxGroupItemTagHelper { Value = "train" },
                        parent,
                        item =>
                            TagHelperRenderer.RenderChildAsync(
                                new RadioGroupItemMediaTagHelper(),
                                item
                            )
                    )
            );
        };

        // Assert
        await Assert.That(act).Throws<InvalidOperationException>();
    }
}
