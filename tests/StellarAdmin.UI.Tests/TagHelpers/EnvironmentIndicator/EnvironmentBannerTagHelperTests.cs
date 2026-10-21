using StellarAdmin.UI.Tests.Support;

namespace StellarAdmin.UI.Tests.TagHelpers.EnvironmentIndicator;

public class EnvironmentBannerTagHelperTests
{
    [Test]
    public async Task ProcessAsync_WithDefaults_RendersNeutralBannerWithLabelOnly()
    {
        // Arrange
        var sut = new EnvironmentBannerTagHelper { Label = "Live" };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var banner = html.QuerySelector("div[data-slot=environment-banner]");
        await Assert.That(banner?.ClassList).Contains("sa-environment-banner");
        await Assert.That(banner?.GetAttribute("data-color")).IsEqualTo("neutral");
        await Assert
            .That(banner?.QuerySelector("strong.sa-environment-banner-label")?.TextContent)
            .IsEqualTo("Live");
        await Assert.That(banner?.QuerySelector(".sa-environment-banner-description")).IsNull();
    }

    [Test]
    public async Task ProcessAsync_WithColorAndDescription_RendersThem()
    {
        // Arrange
        var sut = new EnvironmentBannerTagHelper
        {
            Label = "Live",
            Color = EnvironmentBannerColor.Danger,
            Description = "Changes affect live customers",
        };

        // Act
        using var html = await TagHelperRenderer.RenderAsync(sut);

        // Assert
        var banner = html.QuerySelector("[data-slot=environment-banner]");
        await Assert.That(banner?.GetAttribute("data-color")).IsEqualTo("danger");
        await Assert
            .That(banner?.QuerySelector("span.sa-environment-banner-description")?.TextContent)
            .IsEqualTo("Changes affect live customers");
    }
}
