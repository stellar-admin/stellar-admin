using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Field;

public partial class FieldInputBaseTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WhenVerticalFieldHasClassNames_AppliesClassesToFieldParts()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            Name = "value",
            Label = "Label text",
            Description = "Description text",
            Error = "Error text",
            ClassNames = new InputClassNames
            {
                Root = "custom-root",
                Content = "custom-content",
                Label = "custom-label",
                Description = "custom-description",
                Error = "custom-error",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field].sa-field.custom-root").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-label].custom-label").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-description].custom-description").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-error].custom-error").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".custom-content").Length).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_WhenHorizontalFieldHasClassNames_AppliesClassesToFieldParts()
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new SwitchTagHelper(context.Generator)
        {
            ViewContext = context.ViewContext,
            Name = "value",
            Label = "Label text",
            Description = "Description text",
            Error = "Error text",
            ClassNames = new SwitchClassNames
            {
                Root = "custom-root",
                Content = "custom-content",
                Label = "custom-label",
                Description = "custom-description",
                Error = "custom-error",
            },
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field].sa-field.custom-root").Length)
            .IsEqualTo(1);
        await Assert
            .That(
                html.QuerySelectorAll(
                    "[data-slot=field] > [data-slot=field-content].sa-field-content.custom-content"
                ).Length
            )
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-label].custom-label").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-description].custom-description").Length)
            .IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("[data-slot=field-error].custom-error").Length)
            .IsEqualTo(1);
    }
}
