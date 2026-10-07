using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Sidebar;
using TUnit.Assertions.Enums;

namespace StellarAdmin.Dashboard.IntegrationTests.Sidebar;

public class SidebarLinkTests
{
    private const string CommandItems = "#--command-palette [data-slot='command-item']";

    private const string SidebarLinks =
        "[data-slot='sidebar-group'] [data-slot='sidebar-menu-button']";

    [Test]
    public async Task CustomProviderGroup_MergesIntoGroupWithSameLabel()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureServices: services =>
                services.AddSingleton<ISidebarItemsProvider>(
                    new FixedSidebarItemsProvider(
                        new SidebarGroupItem(
                            "Resources",
                            [new SidebarLinkItem("Help", "/help") { Order = -1 }]
                        )
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents("[data-slot='sidebar-group-label']"))
            .IsEquivalentTo(["Resources"], CollectionOrdering.Matching);
        await Assert
            .That(document.TextContents(SidebarLinks))
            .IsEquivalentTo(["Help", "Products"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task Groups_AppearWhereTheirLabelFirstAppears()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
            {
                dashboard.AddSidebarLink(
                    "Revenue",
                    SidebarLinkTarget.Url("/revenue"),
                    link => link.Group = "Analytics"
                );
                dashboard.AddSidebarLink(
                    "Reports",
                    SidebarLinkTarget.Url("/reports"),
                    link => link.Group = "Resources"
                );
                dashboard.AddSidebarLink(
                    "Traffic",
                    SidebarLinkTarget.Url("/traffic"),
                    link => link.Group = "Analytics"
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents("[data-slot='sidebar-group-label']"))
            .IsEquivalentTo(["Resources", "Analytics"], CollectionOrdering.Matching);
        await Assert
            .That(document.TextContents(SidebarLinks))
            .IsEquivalentTo(
                ["Products", "Reports", "Revenue", "Traffic"],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task LinkInResourceGroup_SortsWithResourcesByOrder()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
            {
                dashboard.AddSidebarLink(
                    "Status",
                    SidebarLinkTarget.Url("https://status.example.com"),
                    link =>
                    {
                        link.Group = "Resources";
                        link.Order = 5;
                    }
                );
                dashboard.AddSidebarLink(
                    "Reports",
                    SidebarLinkTarget.Url("/reports"),
                    link =>
                    {
                        link.Group = "Resources";
                        link.Order = -1;
                    }
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");
        var links = document.QuerySelectorAll(SidebarLinks).ToArray();

        // Assert
        await Assert
            .That(links.Select(link => link.TextContent.Trim()).ToArray())
            .IsEquivalentTo(["Reports", "Products", "Status"], CollectionOrdering.Matching);
        await Assert
            .That(links.Select(link => link.GetAttribute("href")).ToArray())
            .IsEquivalentTo(
                new string?[]
                {
                    "/reports",
                    "/stellaradmin/products",
                    "https://status.example.com",
                },
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task OpenInNewTab_RendersTargetAndRel()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
            {
                dashboard.AddSidebarLink(
                    "Status",
                    SidebarLinkTarget.Url("https://status.example.com"),
                    link => link.OpenInNewTab = true
                );
                dashboard.AddSidebarLink("Reports", SidebarLinkTarget.Url("/reports"));
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        foreach (var selector in new[] { SidebarLinks, CommandItems })
        {
            var links = CustomLinks(document, selector);
            await Assert
                .That(links.Select(link => link.GetAttribute("target")).ToArray())
                .IsEquivalentTo(new string?[] { "_blank", null }, CollectionOrdering.Matching);
            await Assert
                .That(links.Select(link => link.GetAttribute("rel")).ToArray())
                .IsEquivalentTo(
                    new string?[] { "noopener noreferrer", null },
                    CollectionOrdering.Matching
                );
        }
    }

    [Test]
    [Arguments(new string[0], new[] { "Products" })]
    [Arguments(new[] { "Finance" }, new[] { "Products", "Invoices" })]
    public async Task RequiredAuthorization_ShowsLinkOnlyToAuthorizedUsers(
        string[] roles,
        string[] expectedLinks
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.AddSidebarLink(
                    "Invoices",
                    SidebarLinkTarget.Url("/invoices"),
                    link =>
                    {
                        link.Group = "Resources";
                        link.RequireAuthorization(policy => policy.RequireRole("Finance"));
                    }
                ),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", roles);

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(SidebarLinks))
            .IsEquivalentTo(expectedLinks, CollectionOrdering.Matching);
    }

    [Test]
    public async Task Targets_RenderHrefsInSidebarAndCommandPalette()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
            {
                dashboard.AddSidebarLink("Reports", SidebarLinkTarget.Page("/Reports"));
                dashboard.AddSidebarLink("Invoices", SidebarLinkTarget.Action("Index", "Invoices"));
                dashboard.AddSidebarLink(
                    "Status",
                    SidebarLinkTarget.Url("https://status.example.com")
                );
                dashboard.AddSidebarLink("Help", SidebarLinkTarget.Url("~/help"));
            },
            configureApp: app =>
            {
                app.MapRazorPages();
                app.MapDefaultControllerRoute();
            }
        );
        using var client = sut.GetTestClient();
        string?[] expectedHrefs = ["/Reports", "/Invoices", "https://status.example.com", "/help"];

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        foreach (var selector in new[] { SidebarLinks, CommandItems })
        {
            await Assert
                .That(
                    CustomLinks(document, selector)
                        .Select(link => link.GetAttribute("href"))
                        .ToArray()
                )
                .IsEquivalentTo(expectedHrefs, CollectionOrdering.Matching);
        }
    }

    [Test]
    public async Task UngroupedLinks_RenderInSidebarMenuWithoutHeading()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.AddSidebarLink("Reports", SidebarLinkTarget.Url("/reports"))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");
        var groups = document.QuerySelectorAll("[data-slot='sidebar-group']").ToArray();

        // Assert
        await Assert.That(groups.Length).IsEqualTo(2);
        await Assert.That(groups[1].QuerySelector("[data-slot='sidebar-group-label']")).IsNull();
        await Assert
            .That(
                groups[1]
                    .TextContents("[data-slot='sidebar-menu'] [data-slot='sidebar-menu-button']")
            )
            .IsEquivalentTo(["Reports"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task UngroupedLinks_SortByOrderInCommandPalette()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
            {
                dashboard.AddSidebarLink(
                    "Reports",
                    SidebarLinkTarget.Url("/reports"),
                    link => link.Order = 10
                );
                dashboard.AddSidebarLink("Overview", SidebarLinkTarget.Url("/overview"));
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");
        var ungroupedItems = document
            .QuerySelectorAll("#--command-palette [data-slot='command-group']")
            .First()
            .TextContents("[data-slot='command-item']");

        // Assert
        await Assert
            .That(ungroupedItems)
            .IsEquivalentTo(["Overview", "Reports"], CollectionOrdering.Matching);
    }

    private static IElement[] CustomLinks(IParentNode document, string selector)
    {
        return document
            .QuerySelectorAll(selector)
            .Where(link => link.TextContent.Trim() != "Products")
            .ToArray();
    }

    private sealed class FixedSidebarItemsProvider(params SidebarItem[] items)
        : ISidebarItemsProvider
    {
        public Task<SidebarItem[]> GetItemsAsync(HttpContext httpContext) => Task.FromResult(items);
    }
}
