using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Home;

public class HomePageTests
{
    [Test]
    public async Task Development_ShowsSetupChecklist()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents("[data-slot='page-header-title']"))
            .IsEquivalentTo(["Welcome to StellarAdmin"]);
        await Assert
            .That(
                document
                    .QuerySelectorAll("[role='listitem'] [data-slot='collapsible']")
                    .Select(step => step.Id)
            )
            .IsEquivalentTo([
                "--setup-step-resource",
                "--setup-step-protect",
                "--setup-step-branding",
                "--setup-step-theme",
                "--setup-step-sidebar",
            ]);
        await Assert
            .That(document.TextContents("[role='listitem'] [data-slot='item-title']")[0])
            .IsEqualTo("1 resource registered");
    }

    [Test]
    public async Task Development_OpensFirstStepStillToDo()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.RequiredElement("#--setup-step-resource").HasAttribute("hidden"))
            .IsTrue();
        await Assert
            .That(document.RequiredElement("#--setup-step-protect").HasAttribute("hidden"))
            .IsFalse();
    }

    [Test]
    public async Task Production_LinksToResources()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            environmentName: "Production"
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert.That(document.QuerySelector("[data-setup-tabs]")).IsNull();
        await Assert
            .That(
                document
                    .QuerySelectorAll("a[data-slot='item']")
                    .Select(link => link.GetAttribute("href"))
            )
            .IsEquivalentTo(["/stellaradmin/products"]);
    }

    [Test]
    public async Task Production_LinksToResourcesInSidebarOrder()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.AddResource<CustomProduct>(
                    "catalog",
                    resource =>
                    {
                        resource.UseDataSource<CustomProductDataSource>();
                        resource.SidebarItem(item => item.Group = "Catalog");
                    }
                );
                admin.ConfigureSidebarGroup("Catalog", group => group.Order = 1);
            },
            environmentName: "Production"
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("a[data-slot='item']")
                    .Select(link => link.GetAttribute("href"))
            )
            .IsEquivalentTo(
                ["/stellaradmin/catalog", "/stellaradmin/products"],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task Production_WithoutVisibleResources_ShowsEmptyState()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configure: resource => resource.SidebarItem(item => item.Visible = false),
            environmentName: "Production"
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents("[data-slot='empty-title']"))
            .IsEquivalentTo(["Nothing here yet"]);
    }

    [Test]
    public async Task ChecklistTurnedOff_LinksToResourcesInDevelopment()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureHomePage(home => home.ShowSetupChecklist = false)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert.That(document.QuerySelector("[data-setup-tabs]")).IsNull();
        await Assert.That(document.QuerySelectorAll("a[data-slot='item']")).Count().IsEqualTo(1);
    }

    [Test]
    public async Task RequiredAuthorization_ShowsPanelProtected()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin => admin.RequireAuthorization(),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada");

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents("[role='listitem'] [data-slot='item-title']"))
            .Contains("Panel is protected");
    }

    [Test]
    public async Task RouteAuthorization_ShowsPanelProtected()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: TestAuthenticationHandler.Register,
            configureRoute: route => route.RequireAuthorization()
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada");

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents("[role='listitem'] [data-slot='item-title']"))
            .Contains("Panel is protected");
    }

    [Test]
    public async Task Authentication_ShowsUserMenuStep()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert.That(document.QuerySelector("#--setup-step-usermenu")).IsNotNull();
    }
}
