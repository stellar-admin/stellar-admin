using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Infrastructure;

namespace StellarAdmin.IntegrationTests.Authorization;

public class StellarAdminAuthorizationTests
{
    [Test]
    [Arguments("/stellaradmin")]
    [Arguments("/stellaradmin/products")]
    [Arguments("/stellaradmin/products/create")]
    [Arguments("/stellaradmin/home/icons")]
    public async Task RequiredAuthorization_AnonymousRequest_ReturnsUnauthorized(string url)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin => admin.RequireAuthorization(),
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin => admin.RequireAuthorization(),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada");

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task RequiredAuthorization_AnonymousFormSubmission_ReturnsUnauthorized()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin => admin.RequireAuthorization(),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/products/create",
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin => admin.RequireAuthorization("AdminOnly"),
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.RequireAuthorization(policy => policy.RequireRole("Admin")),
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
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" }),
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
    public async Task RouteConvention_AnonymousRequest_ReturnsUnauthorized()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: TestAuthenticationHandler.Register,
            configureRoute: route => route.RequireAuthorization()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
