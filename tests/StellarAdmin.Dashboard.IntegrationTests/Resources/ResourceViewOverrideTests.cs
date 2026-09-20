using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceViewOverrideTests
{
    [Test]
    [Arguments("", "Custom inventory")]
    [Arguments("/Create", "Create Custom item")]
    [Arguments("/Edit/item-1", "Edit Custom item")]
    public async Task ApplicationOverride_RendersResourceSpecificRazorView(
        string action,
        string expected
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.AddResource<CustomProduct>(resource =>
                {
                    resource.SingularLabel = "Custom item";
                    resource.PluralLabel = "Custom inventory";
                    resource.UseDataSource<CustomProductDataSource>();
                    resource.UseKey(product => product.Code);
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/CustomProduct" + action);

        // Assert
        await Assert
            .That(document.RequiredElement("[data-application-view]").TextContent.Trim())
            .IsEqualTo(expected);
    }
}
