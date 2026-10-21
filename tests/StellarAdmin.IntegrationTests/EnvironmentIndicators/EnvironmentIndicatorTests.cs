using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Infrastructure;

namespace StellarAdmin.IntegrationTests.EnvironmentIndicators;

public class EnvironmentIndicatorTests
{
    private const string Indicator =
        "[data-slot='environment-stripe'], [data-slot='environment-frame'], [data-slot='environment-banner']";

    [Test]
    public async Task NotEnabled_ShowsNoIndicator()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert.That(document.QuerySelector(Indicator)).IsNull();
    }

    [Test]
    public async Task Stripe_ShowsLabelColorAndTabSize()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.EnableEnvironmentIndicator(
                    EnvironmentIndicator.Stripe(
                        "Testing",
                        stripe =>
                        {
                            stripe.Color = EnvironmentColor.Warning;
                            stripe.TabSize = EnvironmentIndicatorTabSize.Large;
                        }
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products");

        // Assert
        var stripe = document.RequiredElement("[data-slot='environment-stripe']");
        await Assert.That(stripe.GetAttribute("data-color")).IsEqualTo("warning");
        var tab = stripe.RequiredElement(".sa-environment-tab");
        await Assert.That(tab.TextContent).IsEqualTo("Testing");
        await Assert.That(tab.GetAttribute("data-size")).IsEqualTo("lg");
    }

    [Test]
    public async Task Frame_ShowsNeutralFrameWithMediumTabByDefault()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.EnableEnvironmentIndicator(EnvironmentIndicator.Frame("Local"))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var frame = document.RequiredElement("[data-slot='environment-frame']");
        await Assert.That(frame.GetAttribute("data-color")).IsEqualTo("neutral");
        await Assert
            .That(frame.RequiredElement(".sa-environment-tab").GetAttribute("data-size"))
            .IsEqualTo("md");
    }

    [Test]
    public async Task BannerWithCustomColor_ShowsDescriptionAndCustomColors()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.EnableEnvironmentIndicator(
                    EnvironmentIndicator.Banner(
                        "Live",
                        banner =>
                        {
                            banner.Color = EnvironmentColor.Custom("#b91c1c", "#f87171");
                            banner.Description = "Changes affect live customers";
                        }
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        var banner = document.RequiredElement("[data-slot='environment-banner']");
        await Assert
            .That(banner.GetAttribute("style"))
            .IsEqualTo("--_custom: #b91c1c; --_custom-dark: #f87171");
        await Assert
            .That(banner.RequiredElement(".sa-environment-banner-label").TextContent)
            .IsEqualTo("Live");
        await Assert
            .That(banner.RequiredElement(".sa-environment-banner-description").TextContent)
            .IsEqualTo("Changes affect live customers");
    }

    [Test]
    public async Task EnabledTwice_ShowsLastIndicator()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin
                    .EnableEnvironmentIndicator(EnvironmentIndicator.Stripe("Testing"))
                    .EnableEnvironmentIndicator(EnvironmentIndicator.Banner("Live"))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll(Indicator)
                    .Select(element => element.GetAttribute("data-slot"))
            )
            .IsEquivalentTo(["environment-banner"]);
    }
}
