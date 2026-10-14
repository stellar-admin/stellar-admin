using Microsoft.AspNetCore.TestHost;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceViewOverrideTests
{
    [Test]
    [Arguments("", "Custom inventory")]
    [Arguments("/create", "Create Custom item")]
    [Arguments("/edit/item-1", "Edit Custom item")]
    public async Task ApplicationOverride_RendersResourceSpecificRazorView(
        string action,
        string expected
    )
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.AddResource<CustomProduct>(resource =>
                {
                    resource.SingularLabel = "Custom item";
                    resource.PluralLabel = "Custom inventory";
                    resource.UseDataSource<CustomProductDataSource>();
                    resource.UseKey(product => product.Code);
                    resource.AllowCreate();
                    resource.AllowEdit();
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/custom-products" + action);

        // Assert
        await Assert
            .That(document.RequiredElement("[data-application-view]").TextContent.Trim())
            .IsEqualTo(expected);
    }
}
