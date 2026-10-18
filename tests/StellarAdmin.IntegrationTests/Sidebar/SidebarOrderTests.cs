using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Sidebar;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Sidebar;

public class SidebarOrderTests
{
    private const string GroupLabels = "[data-slot='sidebar-group-label']";

    private const string SidebarLinks =
        "[data-slot='sidebar-group'] [data-slot='sidebar-menu-button']";

    [Test]
    public async Task GroupWithOrder_ComesBeforeGroupsWithout()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.AddSidebarLink(
                    "Revenue",
                    SidebarLinkTarget.Url("/revenue"),
                    link => link.Group = "Analytics"
                );
                admin.AddSidebarLink(
                    "Deals",
                    SidebarLinkTarget.Url("/deals"),
                    link => link.Group = "Sales"
                );
                admin.ConfigureSidebarGroup("Sales", group => group.Order = 2);
                admin.ConfigureSidebarGroup("Analytics", group => group.Order = 1);
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(GroupLabels))
            .IsEquivalentTo(["Analytics", "Sales", "Resources"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task UngroupedLinkWithOrder_ComesBeforeGroupsWithout()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.AddSidebarLink("Help", SidebarLinkTarget.Url("/help"));
                admin.AddSidebarLink(
                    "Dashboard",
                    SidebarLinkTarget.Url("/dashboard"),
                    link => link.Order = 1
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(SidebarLinks))
            .IsEquivalentTo(["Dashboard", "Products", "Help"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task ItemsWithoutOrder_FollowRegistrationAcrossResourcesAndLinks()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.AddSidebarLink(
                    "Reports",
                    SidebarLinkTarget.Url("/reports"),
                    link => link.Group = "Resources"
                );
                admin.AddResource<CustomProduct>(
                    "catalog",
                    resource =>
                    {
                        resource.UseDataSource<CustomProductDataSource>();
                        resource.SidebarItem(item => item.Label = "Catalog");
                    }
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(SidebarLinks))
            .IsEquivalentTo(["Products", "Reports", "Catalog"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task SortGroupsByLabel_SortsGroupsAndUngroupedLinksWithoutOrder()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                admin.AddSidebarLink(
                    "Deals",
                    SidebarLinkTarget.Url("/deals"),
                    link => link.Group = "Sales"
                );
                admin.AddSidebarLink("Help", SidebarLinkTarget.Url("/help"));
                admin.AddSidebarLink(
                    "Revenue",
                    SidebarLinkTarget.Url("/revenue"),
                    link => link.Group = "Analytics"
                );
                admin.AddSidebarLink(
                    "Dashboard",
                    SidebarLinkTarget.Url("/dashboard"),
                    link => link.Order = 1
                );
                admin.ConfigureSidebar(sidebar => sidebar.SortGroupsByLabel = true);
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(SidebarLinks))
            .IsEquivalentTo(
                ["Dashboard", "Revenue", "Help", "Products", "Deals"],
                CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task SortItemsByLabel_SortsItemsWithoutOrderAfterItemsWithOrder()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                AddResourceLinks(admin);
                admin.ConfigureSidebar(sidebar => sidebar.SortItemsByLabel = true);
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(SidebarLinks))
            .IsEquivalentTo(["Status", "alerts", "Products", "Zones"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task GroupSortItemsByLabel_OverridesSidebarSetting()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
            {
                AddResourceLinks(admin);
                admin.ConfigureSidebar(sidebar => sidebar.SortItemsByLabel = true);
                admin.ConfigureSidebarGroup("Resources", group => group.SortItemsByLabel = false);
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin");

        // Assert
        await Assert
            .That(document.TextContents(SidebarLinks))
            .IsEquivalentTo(["Status", "Products", "Zones", "alerts"], CollectionOrdering.Matching);
    }

    private static void AddResourceLinks(StellarAdminBuilder admin)
    {
        admin.AddSidebarLink(
            "Zones",
            SidebarLinkTarget.Url("/zones"),
            link => link.Group = "Resources"
        );
        admin.AddSidebarLink(
            "alerts",
            SidebarLinkTarget.Url("/alerts"),
            link => link.Group = "Resources"
        );
        admin.AddSidebarLink(
            "Status",
            SidebarLinkTarget.Url("/status"),
            link =>
            {
                link.Group = "Resources";
                link.Order = 1;
            }
        );
    }
}
