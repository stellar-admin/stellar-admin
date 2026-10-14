using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Sidebar;

namespace StellarAdmin.IntegrationTests.Layout;

public class HostPageLayoutTests
{
    [Test]
    public async Task HostPage_RendersInDefaultLayout()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.AddSidebarLink("Sales report", SidebarLinkTarget.Page("/SalesReport")),
            configureApp: app => app.MapRazorPages()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/SalesReport");

        // Assert
        await Assert.That(document.Title).IsEqualTo("Sales report - Stellar Admin Integration Tests");
        await Assert.That(document.QuerySelector("#sales-report")).IsNotNull();
        await Assert.That(document.QuerySelector("#--command-palette")).IsNotNull();
        await Assert.That(document.QuerySelector("template#admin-sheet-template")).IsNotNull();
        await Assert
            .That(
                document.TextContents(
                    "[data-slot='sidebar-group'] [data-slot='sidebar-menu-button'][data-active]"
                )
            )
            .IsEquivalentTo(["Sales report"]);
    }
}
