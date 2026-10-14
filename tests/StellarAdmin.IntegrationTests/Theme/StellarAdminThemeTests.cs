using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Infrastructure;

namespace StellarAdmin.IntegrationTests.Theme;

public class StellarAdminThemeTests
{
    [Test]
    public async Task DefaultConfiguration_RendersTheStylesheetWithoutWebFonts()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("link[href*='StellarAdmin.UI/stellar-admin.css']")
                    .GetAttribute("href")
            )
            .StartsWith("/_content/StellarAdmin.UI/stellar-admin.css");
        await Assert.That(document.QuerySelector("link[href*='fonts.googleapis.com']")).IsNull();
    }

    [Test]
    public async Task ThemeAndFontStylesheets_RenderAfterTheBundleInRegistrationOrder()
    {
        // Arrange
        const string Fonts =
            "https://fonts.googleapis.com/css2?family=Lexend:wght@300..700&display=swap";
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.AddStylesheet(Fonts);
                admin.AddStylesheet("~/css/theme.css");
                admin.AddStylesheet("~/css/overrides.css");
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");
        var hrefs = document
            .QuerySelectorAll("link[rel='stylesheet']")
            .Select(link => link.GetAttribute("href") ?? "")
            .ToList();

        // Assert
        var bundle = hrefs.FindIndex(href => href.Contains("StellarAdmin.UI/stellar-admin.css"));
        var theme = hrefs.FindIndex(href => href.StartsWith("/css/theme.css"));
        var overrides = hrefs.FindIndex(href => href.StartsWith("/css/overrides.css"));
        await Assert.That(bundle).IsGreaterThanOrEqualTo(0);
        await Assert.That(hrefs).Contains(Fonts);
        await Assert.That(theme).IsGreaterThan(bundle);
        await Assert.That(overrides).IsGreaterThan(theme);
    }
}
