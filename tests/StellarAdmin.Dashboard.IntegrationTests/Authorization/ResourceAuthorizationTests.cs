using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Authorization;

public class ResourceAuthorizationTests
{
    [Test]
    [Arguments("/stellaradmin/Product", HttpStatusCode.Unauthorized)]
    [Arguments("/stellaradmin/Product/Create", HttpStatusCode.Unauthorized)]
    [Arguments("/stellaradmin/CustomProduct", HttpStatusCode.OK)]
    [Arguments("/stellaradmin", HttpStatusCode.OK)]
    public async Task RequiredAuthorization_AnonymousRequest_ProtectsOnlyThatResource(
        string url,
        HttpStatusCode expectedStatusCode
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource => resource.RequireAuthorization(),
            dashboard =>
                dashboard.AddResource<CustomProduct>(resource =>
                    resource.UseDataSource<CustomProductDataSource>()
                ),
            TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(url);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(expectedStatusCode);
    }

    [Test]
    [Arguments("/stellaradmin/Product", HttpStatusCode.Forbidden)]
    [Arguments("/stellaradmin", HttpStatusCode.OK)]
    public async Task DashboardAndResourceRequirements_UserMissingResourceRole_AppliesBoth(
        string url,
        HttpStatusCode expectedStatusCode
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource => resource.RequireAuthorization(policy => policy.RequireRole("Catalog")),
            dashboard => dashboard.RequireAuthorization(policy => policy.RequireRole("Staff")),
            TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", "Staff");

        // Act
        using var response = await client.GetAsync(url);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(expectedStatusCode);
    }

    [Test]
    public async Task DashboardAndResourceRequirements_UserWithBothRoles_RendersResource()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource => resource.RequireAuthorization(policy => policy.RequireRole("Catalog")),
            dashboard => dashboard.RequireAuthorization(policy => policy.RequireRole("Staff")),
            TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", "Staff", "Catalog");

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    [Arguments(new[] { "Catalog" }, HttpStatusCode.Forbidden)]
    [Arguments(new[] { "Catalog", "Pricing" }, HttpStatusCode.OK)]
    public async Task RepeatedRequirements_AuthenticatedRequest_RequiresEveryPolicy(
        string[] roles,
        HttpStatusCode expectedStatusCode
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
            {
                resource.RequireAuthorization("CatalogPolicy");
                resource.RequireAuthorization("PricingPolicy");
            },
            configureServices: services =>
            {
                TestAuthenticationHandler.Register(services);
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("CatalogPolicy", policy => policy.RequireRole("Catalog"));
                    options.AddPolicy("PricingPolicy", policy => policy.RequireRole("Pricing"));
                });
            }
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", roles);

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(expectedStatusCode);
    }
}
