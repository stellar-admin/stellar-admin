using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.TagHelpers.Input;

public partial class InputTagHelperTests
{
    [Test]
    [Arguments("text", "input", "sa-input")]
    [Arguments("checkbox", "span", "sa-input-control-wrapper")]
    [Arguments("radio", "span", "sa-input-control-wrapper")]
    public async Task ProcessAsync_WhenClassNamesAreConfigured_AppliesFieldClasses(
        string type,
        string element,
        string structuralClass
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            InputTypeName = type,
            ClassNames = new InputClassNames
            {
                Root = "custom-root",
                Label = "custom-label",
                Description = "custom-description",
                Error = "custom-error",
                Content = "custom-content",
                Control = "custom-control",
            },
            Label = "Name",
            Description = "Help",
            Error = "Invalid",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".custom-root").Length).IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll("div.sa-field.custom-root").Length).IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".custom-label").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("label.sa-field-label.custom-label").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".custom-description").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("p.sa-field-description.custom-description").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".custom-error").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-field-error.custom-error").Length)
            .IsEqualTo(1);
        await Assert.That(html.QuerySelectorAll(".custom-control").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector($"{element}.{structuralClass}.custom-control"))
            .IsNotNull();
    }

    [Test]
    [Arguments("checkbox")]
    [Arguments("radio")]
    public async Task ProcessAsync_WhenControlHasContent_AppliesContentClass(string type)
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            InputTypeName = type,
            ClassNames = new InputClassNames
            {
                Root = "custom-root",
                Label = "custom-label",
                Description = "custom-description",
                Error = "custom-error",
                Content = "custom-content",
                Control = "custom-control",
            },
            Label = "Name",
            Description = "Help",
            Error = "Invalid",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".custom-content").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelectorAll("div.sa-field-content.custom-content").Length)
            .IsEqualTo(1);
    }

    [Test]
    [Arguments("text", "sa-input")]
    [Arguments("checkbox", "sa-checkbox")]
    [Arguments("radio", "sa-radiobutton")]
    public async Task ProcessAsync_WhenExplicitClassIsSupplied_PreservesInputClass(
        string type,
        string structuralClass
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            InputTypeName = type,
            ClassNames = new InputClassNames
            {
                Root = "custom-root",
                Label = "custom-label",
                Description = "custom-description",
                Error = "custom-error",
                Content = "custom-content",
                Control = "custom-control",
            },
            Label = "Name",
            Description = "Help",
            Error = "Invalid",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(html.QuerySelectorAll(".existing-class").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector($"input.{structuralClass}.existing-class"))
            .IsNotNull();
    }

    [Test]
    [Arguments("text", "input", "sa-input")]
    [Arguments("checkbox", "span", "sa-input-control-wrapper")]
    [Arguments("radio", "span", "sa-input-control-wrapper")]
    public async Task ProcessAsync_WhenFieldIsSuppressed_PreservesOnlyControlClasses(
        string type,
        string element,
        string structuralClass
    )
    {
        // Arrange
        using var context = new RenderingContext();
        var sut = new InputTagHelper(context.Generator, context.Icons)
        {
            ViewContext = context.ViewContext,
            InputTypeName = type,
            ClassNames = new InputClassNames
            {
                Root = "custom-root",
                Label = "custom-label",
                Description = "custom-description",
                Error = "custom-error",
                Content = "custom-content",
                Control = "custom-control",
            },
            Label = "Name",
            Description = "Help",
            Error = "Invalid",
        };
        sut.ShouldRenderField = false;

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(
                html.QuerySelector(
                    ".custom-root, .custom-label, .custom-description, .custom-error"
                )
            )
            .IsNull();
        await Assert.That(html.QuerySelectorAll(".custom-control").Length).IsEqualTo(1);
        await Assert
            .That(html.QuerySelector($"{element}.{structuralClass}.custom-control"))
            .IsNotNull();
    }
}
