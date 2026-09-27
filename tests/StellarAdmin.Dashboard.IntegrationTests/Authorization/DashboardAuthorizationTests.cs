using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

namespace StellarAdmin.Dashboard.IntegrationTests.Authorization;

public class DashboardAuthorizationTests
{
    [Test]
    [Arguments("/stellaradmin")]
    [Arguments("/stellaradmin/Product")]
    [Arguments("/stellaradmin/Product/Create")]
    public async Task RequiredAuthorization_AnonymousRequest_ReturnsUnauthorized(string url)
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard => dashboard.RequireAuthorization(),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync(url);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task RequiredAuthorization_AuthenticatedRequest_RendersPage()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard => dashboard.RequireAuthorization(),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada");

        // Act
        using var response = await client.GetAsync("/stellaradmin/Product");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task RequiredAuthorization_AnonymousFormSubmission_ReturnsUnauthorized()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard => dashboard.RequireAuthorization(),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/Product/Create",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["Name"] = "Tent" })
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Arguments("Editor", HttpStatusCode.Forbidden)]
    [Arguments("Admin", HttpStatusCode.OK)]
    public async Task RequiredPolicyName_AuthenticatedRequest_AppliesPolicy(
        string role,
        HttpStatusCode expectedStatusCode
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard => dashboard.RequireAuthorization("AdminOnly"),
            configureServices: services =>
            {
                TestAuthenticationHandler.Register(services);
                services.AddAuthorization(options =>
                    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
                );
            }
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", role);

        // Act
        using var response = await client.GetAsync("/stellaradmin");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(expectedStatusCode);
    }

    [Test]
    [Arguments("Editor", HttpStatusCode.Forbidden)]
    [Arguments("Admin", HttpStatusCode.OK)]
    public async Task ConfiguredPolicy_AuthenticatedRequest_AppliesPolicy(
        string role,
        HttpStatusCode expectedStatusCode
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.RequireAuthorization(policy => policy.RequireRole("Admin")),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", role);

        // Act
        using var response = await client.GetAsync("/stellaradmin");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(expectedStatusCode);
    }

    [Test]
    [Arguments("Editor", HttpStatusCode.Forbidden)]
    [Arguments("Admin", HttpStatusCode.OK)]
    public async Task RequiredAuthorizeData_AuthenticatedRequest_AppliesRoles(
        string role,
        HttpStatusCode expectedStatusCode
    )
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            configureDashboard: dashboard =>
                dashboard.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", role);

        // Act
        using var response = await client.GetAsync("/stellaradmin");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(expectedStatusCode);
    }
}
