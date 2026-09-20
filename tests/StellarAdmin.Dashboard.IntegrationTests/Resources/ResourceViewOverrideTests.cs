using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class ResourceViewOverrideTests
{
    [Test]
    [Arguments("", "Custom inventory")]
    [Arguments("/Create", "Create Custom item")]
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
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await new HtmlParser().ParseDocumentAsync(
            await client.GetStringAsync("/stellaradmin/CustomProduct" + action)
        );

        // Assert
        await Assert
            .That(document.QuerySelector("[data-application-view]")?.TextContent.Trim())
            .IsEqualTo(expected);
    }
}
