using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Sidebar;

namespace StellarAdmin.IntegrationTests.Sidebar;

public class SidebarBrandingTests
{
    private const string Header = "[data-slot='sidebar-header'] [data-slot='sidebar-menu-button']";

    [Test]
    public async Task NoBranding_ShowsApplicationNameWithoutLogo()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var header = document.RequiredElement(Header);
        await Assert.That(header.GetAttribute("href")).IsEqualTo("/stellaradmin");
        await Assert
            .That(header.RequiredElement(".sa-sidebar-brand-title").TextContent)
            .IsEqualTo("Stellar Admin Integration Tests");
        await Assert.That(header.QuerySelector(".sa-sidebar-brand-subtitle")).IsNull();
        await Assert.That(header.QuerySelector(".sa-sidebar-brand-mark, img")).IsNull();
    }

    [Test]
    public async Task Title_NamesHeaderAppHeaderAndBrowserTitle()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureBranding(branding =>
                {
                    branding.Title = "Voyager Travel";
                    branding.Subtitle = "Admin Console";
                    branding.Href = "~/home";
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var header = document.RequiredElement(Header);
        await Assert.That(header.GetAttribute("href")).IsEqualTo("/home");
        await Assert
            .That(header.TextContents(".sa-sidebar-brand-text > span"))
            .IsEquivalentTo(["Voyager Travel", "Admin Console"]);
        await Assert.That(document.Title).EndsWith(" - Voyager Travel");
        await Assert
            .That(document.RequiredElement("meta[name='description']").GetAttribute("content"))
            .IsEqualTo("Voyager Travel");
        await Assert
            .That(document.RequiredElement("[data-slot='app-header'] .font-medium").TextContent)
            .IsEqualTo("Voyager Travel");
    }

    [Test]
    public async Task IconLogo_ShowsIconBesideTitle()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureBranding(branding => branding.Logo = BrandingLogo.Icon("compass"))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var header = document.RequiredElement(Header);
        await Assert.That(header.QuerySelector(".sa-sidebar-brand-mark > svg")).IsNotNull();
        await Assert.That(header.QuerySelector(".sa-sidebar-brand-title")).IsNotNull();
    }

    [Test]
    public async Task ImageLogo_ShowsImageBesideTitle()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureBranding(branding =>
                    branding.Logo = BrandingLogo.Image("~/img/logo.svg")
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var header = document.RequiredElement(Header);
        var image = header.RequiredElement("img.sa-sidebar-brand-logo");
        await Assert.That(image.GetAttribute("src")).IsEqualTo("/img/logo.svg");
        await Assert.That(image.HasAttribute("data-scheme")).IsFalse();
        await Assert.That(header.QuerySelector(".sa-sidebar-brand-title")).IsNotNull();
    }

    [Test]
    public async Task WordmarkWithDarkVariant_ReplacesTitleWithBothImages()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureBranding(branding =>
                {
                    branding.Title = "Voyager Travel";
                    branding.Logo = BrandingLogo.Wordmark(
                        "~/img/wordmark.svg",
                        "~/img/wordmark-dark.svg"
                    );
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var header = document.RequiredElement(Header);
        var images = header.QuerySelectorAll("img.sa-sidebar-brand-wordmark");
        await Assert
            .That(images.Select(image => image.GetAttribute("data-scheme") ?? ""))
            .IsEquivalentTo(["light", "dark"]);
        await Assert
            .That(images.Select(image => image.GetAttribute("src") ?? ""))
            .IsEquivalentTo(["/img/wordmark.svg", "/img/wordmark-dark.svg"]);
        await Assert
            .That(images.Select(image => image.GetAttribute("alt") ?? ""))
            .IsEquivalentTo(["Voyager Travel", "Voyager Travel"]);
        await Assert
            .That(images.Select(image => image.GetAttribute("data-layout") ?? ""))
            .IsEquivalentTo(["center", "center"]);
        await Assert.That(header.QuerySelector(".sa-sidebar-brand-text")).IsNull();
    }

    [Test]
    [Arguments(WordmarkLayout.Start, "start")]
    [Arguments(WordmarkLayout.Fill, "fill")]
    public async Task WordmarkWithLayout_RendersLayout(WordmarkLayout layout, string expected)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureBranding(branding =>
                    branding.Logo = BrandingLogo.Wordmark("~/img/wordmark.svg", layout)
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var image = document
            .RequiredElement(Header)
            .RequiredElement("img.sa-sidebar-brand-wordmark");
        await Assert.That(image.GetAttribute("data-layout")).IsEqualTo(expected);
    }
}
