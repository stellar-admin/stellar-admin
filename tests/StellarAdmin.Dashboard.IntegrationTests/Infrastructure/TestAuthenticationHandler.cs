using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StellarAdmin.Dashboard.IntegrationTests.Infrastructure;

// Authenticates requests from test headers, so each client chooses its own user and roles.
internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string RolesHeader = "X-Test-Roles";
    public const string SchemeName = "Test";
    public const string UserHeader = "X-Test-User";

    public static void Register(IServiceCollection services)
    {
        services
            .AddAuthentication(SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(SchemeName, null);
    }

    public static void SignIn(HttpClient client, string userName, params string[] roles)
    {
        client.DefaultRequestHeaders.Add(UserHeader, userName);
        client.DefaultRequestHeaders.Add(RolesHeader, string.Join(',', roles));
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var userName))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new(ClaimTypes.Name, userName.ToString()) };
        claims.AddRange(
            Request
                .Headers[RolesHeader]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(role => new Claim(ClaimTypes.Role, role))
        );

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));

        return Task.FromResult(
            AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName))
        );
    }
}
