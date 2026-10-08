using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Sidebar;

namespace StellarAdmin.Dashboard.IntegrationTests.Layout;

public class HostPageLayoutTests
{
    [Test]
    public async Task HostPage_RendersInDashboardLayout()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.AddSidebarLink("Sales report", SidebarLinkTarget.Page("/SalesReport")),
            configureApp: app => app.MapRazorPages()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/SalesReport");

        // Assert
        await Assert.That(document.Title).IsEqualTo("Sales report - StellarAdmin");
        await Assert.That(document.QuerySelector("#sales-report")).IsNotNull();
        await Assert.That(document.QuerySelector("#--command-palette")).IsNotNull();
        await Assert.That(document.QuerySelector("template#dashboard-sheet-template")).IsNotNull();
        await Assert
            .That(
                document.TextContents(
                    "[data-slot='sidebar-group'] [data-slot='sidebar-menu-button'][data-active]"
                )
            )
            .IsEquivalentTo(["Sales report"]);
    }
}
