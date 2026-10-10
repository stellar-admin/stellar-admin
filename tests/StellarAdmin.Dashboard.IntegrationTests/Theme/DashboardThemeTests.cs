using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Theme;

public class DashboardThemeTests
{
    [Test]
    public async Task DefaultConfiguration_RendersTheStylesheetWithoutPresetOrWebFonts()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("link[href*='StellarAdmin.TagHelpers/stellar-admin.css']")
                    .GetAttribute("href")
            )
            .StartsWith("/_content/StellarAdmin.TagHelpers/stellar-admin.css");
        await Assert
            .That(document.QuerySelector("link[href*='StellarAdmin.TagHelpers/presets/']"))
            .IsNull();
        await Assert.That(document.QuerySelector("link[href*='fonts.googleapis.com']")).IsNull();
    }

    [Test]
    [Arguments(DashboardThemePreset.Ledger, "ledger")]
    [Arguments(DashboardThemePreset.Ops, "ops")]
    [Arguments(DashboardThemePreset.Soft, "soft")]
    public async Task SelectedPreset_RendersItsStylesheetAfterTheBundle(
        DashboardThemePreset selectedPreset,
        string expectedName
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.ConfigureTheme(theme => theme.Preset = selectedPreset)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");
        var hrefs = document
            .QuerySelectorAll("link[rel='stylesheet']")
            .Select(link => link.GetAttribute("href") ?? "")
            .ToList();

        // Assert
        var bundle = hrefs.FindIndex(href =>
            href.Contains("StellarAdmin.TagHelpers/stellar-admin.css")
        );
        var preset = hrefs.FindIndex(href =>
            href.Contains($"StellarAdmin.TagHelpers/presets/{expectedName}.css")
        );
        await Assert.That(bundle).IsGreaterThanOrEqualTo(0);
        await Assert.That(preset).IsGreaterThan(bundle);
        await Assert.That(document.QuerySelector("link[href*='fonts.googleapis.com']")).IsNull();
    }

    [Test]
    public async Task Stylesheet_RendersAfterThePreset()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.ConfigureTheme(theme =>
                {
                    theme.Preset = DashboardThemePreset.Soft;
                    theme.Stylesheet = "~/css/theme.css";
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");
        var hrefs = document
            .QuerySelectorAll("link[rel='stylesheet']")
            .Select(link => link.GetAttribute("href") ?? "")
            .ToList();

        // Assert
        var preset = hrefs.FindIndex(href =>
            href.Contains("StellarAdmin.TagHelpers/presets/soft.css")
        );
        var stylesheet = hrefs.FindIndex(href => href.StartsWith("/css/theme.css"));
        await Assert.That(preset).IsGreaterThanOrEqualTo(0);
        await Assert.That(stylesheet).IsGreaterThan(preset);
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
                    theme.Preset = DashboardThemePreset.Ledger;
                    theme.IncludeSuggestedFonts = true;
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");
        var fontLink = document.RequiredElement("link[href^='https://fonts.googleapis.com/css2']");

        // Assert
        await Assert.That(fontLink.GetAttribute("href")).Contains("Lexend:wght@300..700");
        await Assert.That(fontLink.GetAttribute("href")).Contains("display=swap");
        await Assert.That(document.QuerySelectorAll("link[rel='preconnect']").Length).IsEqualTo(2);
    }
}
