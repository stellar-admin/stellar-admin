using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StellarAdmin.IntegrationTests.Infrastructure;

// Authenticates from the test headers like TestAuthenticationHandler, and records each sign-out. A scheme with
// a provider logout URL redirects there on sign-out, as a remote scheme such as OpenID Connect does.
internal sealed class RecordingSignOutHandler(
    IOptionsMonitor<RecordingSignOutOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    SignOutLog log
) : SignOutAuthenticationHandler<RecordingSignOutOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await Context.AuthenticateAsync(TestAuthenticationHandler.SchemeName);

        return result.Succeeded
            ? AuthenticateResult.Success(new AuthenticationTicket(result.Principal, Scheme.Name))
            : AuthenticateResult.NoResult();
    }

    protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
    {
        log.Schemes.Add(Scheme.Name);
        if (Options.ProviderLogoutUrl is { } providerLogoutUrl)
        {
            Response.Redirect(
                $"{providerLogoutUrl}?returnUrl={Uri.EscapeDataString(properties?.RedirectUri ?? "")}"
            );
        }

        return Task.CompletedTask;
    }
}

internal sealed class RecordingSignOutOptions : AuthenticationSchemeOptions
{
    public string? ProviderLogoutUrl { get; set; }
}

internal sealed class SignOutLog
{
    public List<string> Schemes { get; } = [];
}
