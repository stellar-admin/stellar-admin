using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.EnvironmentIndicator;

public class EnvironmentStripeTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersNeutralStripeWithMediumTab()
    {
        // Arrange
        var sut = new EnvironmentStripeTagHelper { Label = "Testing" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var stripe = html.QuerySelector("div[data-slot=environment-stripe]");
        await Assert.That(stripe?.ClassList).Contains("sa-environment-stripe");
        await Assert.That(stripe?.ClassList).Contains("existing-class");
        await Assert.That(stripe?.GetAttribute("data-color")).IsEqualTo("neutral");
        await Assert.That(stripe?.HasAttribute("style")).IsFalse();
        var tab = stripe?.QuerySelector("span.sa-environment-tab");
        await Assert.That(tab?.GetAttribute("data-size")).IsEqualTo("md");
        await Assert.That(tab?.TextContent).IsEqualTo("Testing");
    }

    [Test]
    public async Task ProcessAsync_WithColorAndTabSize_RendersThem()
    {
        // Arrange
        var sut = new EnvironmentStripeTagHelper
        {
            Label = "Testing",
            Color = EnvironmentStripeColor.Warning,
            TabSize = EnvironmentStripeTabSize.ExtraLarge,
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=environment-stripe]")?.GetAttribute("data-color"))
            .IsEqualTo("warning");
        await Assert
            .That(html.QuerySelector(".sa-environment-tab")?.GetAttribute("data-size"))
            .IsEqualTo("xl");
    }

    [Test]
    public async Task ProcessAsync_WithCustomColors_SetsThemInline()
    {
        // Arrange
        var sut = new EnvironmentStripeTagHelper
        {
            Label = "Staging",
            CustomColor = "#7c3aed",
            CustomDarkColor = "#a78bfa",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert
            .That(html.QuerySelector("[data-slot=environment-stripe]")?.GetAttribute("style"))
            .IsEqualTo("--_custom: #7c3aed; --_custom-dark: #a78bfa");
    }

    [Test]
    public async Task ProcessAsync_WithWhiteSpaceLabel_ThrowsArgumentException()
    {
        // Arrange
        var sut = new EnvironmentStripeTagHelper { Label = " " };

        // Act
        Func<Task> act = () => TagHelperRenderer.RenderAsync(sut);

        // Assert
        await Assert.That(act).Throws<ArgumentException>();
    }
}
