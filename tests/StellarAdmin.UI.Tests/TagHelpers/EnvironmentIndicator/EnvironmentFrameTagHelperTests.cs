using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.EnvironmentIndicator;

public class EnvironmentFrameTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersNeutralFrameWithMediumTab()
    {
        // Arrange
        var sut = new EnvironmentFrameTagHelper { Label = "Local" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var frame = html.QuerySelector("div[data-slot=environment-frame]");
        await Assert.That(frame?.ClassList).Contains("sa-environment-frame");
        await Assert.That(frame?.GetAttribute("data-color")).IsEqualTo("neutral");
        var tab = frame?.QuerySelector("span.sa-environment-tab");
        await Assert.That(tab?.GetAttribute("data-size")).IsEqualTo("md");
        await Assert.That(tab?.TextContent).IsEqualTo("Local");
    }

    [Test]
    public async Task ProcessAsync_WithOnlyCustomDarkColor_SetsOnlyDarkColorInline()
    {
        // Arrange
        var sut = new EnvironmentFrameTagHelper
        {
            Label = "Local",
            Color = EnvironmentFrameColor.Info,
            CustomDarkColor = "#a78bfa",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var frame = html.QuerySelector("[data-slot=environment-frame]");
        await Assert.That(frame?.GetAttribute("data-color")).IsEqualTo("info");
        await Assert.That(frame?.GetAttribute("style")).IsEqualTo("--_custom-dark: #a78bfa");
    }
}
