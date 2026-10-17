using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Infrastructure;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Sidebar;

public class LogoutTests
{
    [Test]
    public async Task NoSchemes_SignsOutOfAuthenticateSchemeAndRedirectsToPanel()
    {
        // Arrange
        var log = new SignOutLog();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: services =>
                AddSchemes(services, log, defaultScheme: "Local", signInScheme: "External")
        );
        using var client = CreateClient(sut);
        var form = await FormTestHelpers.PrepareForm(client, "/stellaradmin");

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/logout",
            new FormUrlEncodedContent(form)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(response.Headers.Location?.OriginalString).IsEqualTo("/stellaradmin");
        await Assert.That(log.Schemes).IsEquivalentTo(["Local"]);
    }

    [Test]
    public async Task ConfiguredSchemes_SignOutInOrderAndRemoteSchemeRedirects()
    {
        // Arrange
        var log = new SignOutLog();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureUserMenu(menu =>
                    menu.Logout(logout =>
                    {
                        logout.AddAuthenticationScheme("Local").AddAuthenticationScheme("Remote");
                        logout.RedirectPath = "~/signed-out";
                    })
                ),
            configureServices: services =>
                AddSchemes(services, log, defaultScheme: "Local", signInScheme: "Local")
        );
        using var client = CreateClient(sut);
        var form = await FormTestHelpers.PrepareForm(client, "/stellaradmin");

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/logout",
            new FormUrlEncodedContent(form)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert
            .That(response.Headers.Location?.OriginalString)
            .IsEqualTo("https://idp.example.com/logout?returnUrl=%2Fsigned-out");
        await Assert
            .That(log.Schemes)
            .IsEquivalentTo(["Local", "Remote"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task LocalSchemes_RedirectToRedirectPath()
    {
        // Arrange
        var log = new SignOutLog();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureAdmin: admin =>
                admin.ConfigureUserMenu(menu =>
                    menu.Logout(logout =>
                    {
                        logout.AddAuthenticationScheme("External").AddAuthenticationScheme("Local");
                        logout.RedirectPath = "~/";
                    })
                ),
            configureServices: services =>
                AddSchemes(services, log, defaultScheme: "Local", signInScheme: "Local")
        );
        using var client = CreateClient(sut);
        var form = await FormTestHelpers.PrepareForm(client, "/stellaradmin");

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/logout",
            new FormUrlEncodedContent(form)
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(response.Headers.Location?.OriginalString).IsEqualTo("/");
        await Assert
            .That(log.Schemes)
            .IsEquivalentTo(["External", "Local"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task MissingAntiforgeryToken_IsRejected()
    {
        // Arrange
        var log = new SignOutLog();
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            configureServices: services =>
                AddSchemes(services, log, defaultScheme: "Local", signInScheme: "Local")
        );
        using var client = CreateClient(sut);

        // Act
        using var response = await client.PostAsync(
            "/stellaradmin/logout",
            new FormUrlEncodedContent([])
        );

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(log.Schemes).IsEmpty();
    }

    private static void AddSchemes(
        IServiceCollection services,
        SignOutLog log,
        string defaultScheme,
        string signInScheme
    )
    {
        services.AddSingleton(log);
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = defaultScheme;
                options.DefaultSignInScheme = signInScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName,
                null
            )
            .AddScheme<RecordingSignOutOptions, RecordingSignOutHandler>("Local", null)
            .AddScheme<RecordingSignOutOptions, RecordingSignOutHandler>("External", null)
            .AddScheme<RecordingSignOutOptions, RecordingSignOutHandler>(
                "Remote",
                options => options.ProviderLogoutUrl = "https://idp.example.com/logout"
            );
    }

    private static HttpClient CreateClient(WebApplication sut)
    {
        var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada");

        return client;
    }
}
