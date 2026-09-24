using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Theme;

public class DashboardThemeTests
{
    [Test]
    public async Task DefaultConfiguration_RendersNovaWithoutWebFonts()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("link[href*='StellarAdmin.TagHelpers/stellar-admin.']")
                    .GetAttribute("href")
            )
            .Contains("stellar-admin.shadcn.nova.css");
        await Assert.That(document.QuerySelector("link[href*='fonts.googleapis.com']")).IsNull();
    }

    [Test]
    [Arguments(DashboardTheme.Aurora, "aurora")]
    [Arguments(DashboardTheme.Concourse, "concourse")]
    [Arguments(DashboardTheme.Ice, "ice")]
    [Arguments(DashboardTheme.Ledger, "ledger")]
    [Arguments(DashboardTheme.Meridian, "meridian")]
    [Arguments(DashboardTheme.Observatory, "observatory")]
    [Arguments(DashboardTheme.Parallax, "parallax")]
    [Arguments(DashboardTheme.ShadcnLuma, "shadcn.luma")]
    [Arguments(DashboardTheme.ShadcnLyra, "shadcn.lyra")]
    [Arguments(DashboardTheme.ShadcnMaia, "shadcn.maia")]
    [Arguments(DashboardTheme.ShadcnMira, "shadcn.mira")]
    [Arguments(DashboardTheme.ShadcnNova, "shadcn.nova")]
    [Arguments(DashboardTheme.ShadcnRhea, "shadcn.rhea")]
    [Arguments(DashboardTheme.ShadcnSera, "shadcn.sera")]
    [Arguments(DashboardTheme.ShadcnVega, "shadcn.vega")]
    public async Task SelectedTheme_RendersItsShippedStylesheet(
        DashboardTheme selectedTheme,
        string expectedName
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.ConfigureTheme(theme => theme.Name = selectedTheme)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("link[href*='StellarAdmin.TagHelpers/stellar-admin.']")
                    .GetAttribute("href")
            )
            .Contains($"stellar-admin.{expectedName}.css");
        await Assert.That(document.QuerySelector("link[href*='fonts.googleapis.com']")).IsNull();
    }

    [Test]
    public async Task SuggestedFontsEnabled_RendersSelectedFamilies()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.ConfigureTheme(theme =>
                {
                    theme.Name = DashboardTheme.ShadcnSera;
                    theme.IncludeSuggestedFonts = true;
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/Product");
        var fontLink = document.RequiredElement("link[href^='https://fonts.googleapis.com/css2']");

        // Assert
        await Assert.That(fontLink.GetAttribute("href")).Contains("Noto+Sans:wght@400..700");
        await Assert.That(fontLink.GetAttribute("href")).Contains("Playfair+Display:wght@400..700");
        await Assert.That(fontLink.GetAttribute("href")).Contains("display=swap");
        await Assert.That(document.QuerySelectorAll("link[rel='preconnect']").Length).IsEqualTo(2);
    }
}
