using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Infrastructure;

namespace StellarAdmin.IntegrationTests.Authorization;

public class HtmxAuthorizationTests
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task RefusedHtmxRequest_ChallengedOrForbidden_RefreshesPage(bool signedIn)
    {
        // Arrange
        await using var sut = await CreateAsync();
        using var client = sut.GetTestClient();
        if (signedIn)
        {
            TestAuthenticationHandler.SignIn(client, "ada");
        }

        client.DefaultRequestHeaders.Add("HX-Request", "true");

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        await Assert.That(response.Headers.GetValues("HX-Refresh")).IsEquivalentTo(["true"]);
    }

    [Test]
    [Arguments(false, false)]
    [Arguments(true, true)]
    public async Task RefusedRequest_NotHtmxOrBoosted_KeepsAppResponse(bool htmx, bool boosted)
    {
        // Arrange
        await using var sut = await CreateAsync();
        using var client = sut.GetTestClient();
        if (htmx)
        {
            client.DefaultRequestHeaders.Add("HX-Request", "true");
        }

        if (boosted)
        {
            client.DefaultRequestHeaders.Add("HX-Boosted", "true");
        }

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(response.Headers.Contains("HX-Refresh")).IsFalse();
    }

    [Test]
    public async Task RefusedHtmxRequestOutsideAdminPanel_KeepsAppResponse()
    {
        // Arrange
        await using var sut = await CreateAsync(configureApp: app =>
            app.MapGet("/reports", () => "Reports").RequireAuthorization()
        );
        using var client = sut.GetTestClient();
        client.DefaultRequestHeaders.Add("HX-Request", "true");

        // Act
        using var response = await client.GetAsync("/reports");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(response.Headers.Contains("HX-Refresh")).IsFalse();
    }

    [Test]
    public async Task AppHandlerRegisteredFirst_StillHandlesOtherRequests()
    {
        // Arrange
        await using var sut = await CreateAsync(services =>
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, TeapotResultHandler>()
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products");

        // Assert
        await Assert.That((int)response.StatusCode).IsEqualTo(StatusCodes.Status418ImATeapot);
    }

    private static Task<WebApplication> CreateAsync(
        Action<IServiceCollection>? configureServices = null,
        Action<WebApplication>? configureApp = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            resource => resource.RequireAuthorization(policy => policy.RequireRole("Catalog")),
            configureServices: services =>
            {
                TestAuthenticationHandler.Register(services);
                configureServices?.Invoke(services);
            },
            configureApp: configureApp
        );

    // An app's own handler, which answers every refused request with 418
    private sealed class TeapotResultHandler : IAuthorizationMiddlewareResultHandler
    {
        public Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult
        )
        {
            if (authorizeResult.Succeeded)
            {
                return next(context);
            }

            context.Response.StatusCode = StatusCodes.Status418ImATeapot;

            return Task.CompletedTask;
        }
    }
}
