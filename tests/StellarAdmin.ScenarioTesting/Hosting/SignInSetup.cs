using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VoyagerTravel.Data;

namespace StellarAdmin.ScenarioTesting.Hosting;

// ASP.NET Core Identity on the seeded Voyager Travel users, with a sign-in page that needs no password: it
// lists the users, and ?email= signs one in with Identity's cookie. A spec signs in with "user:" or a signIn
// step; Identity sends the browser here when a page needs a signed-in user
public static class SignInSetup
{
    public const string AccessDeniedPath = "/scenario/access-denied";
    public const string SignInPath = "/scenario/sign-in";

    public static IServiceCollection AddScenarioSignIn(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<DisplayNameClaimsPrincipalFactory>();
        services.ConfigureApplicationCookie(cookie =>
        {
            cookie.LoginPath = SignInPath;
            cookie.AccessDeniedPath = AccessDeniedPath;
        });

        return services;
    }

    public static IEndpointRouteBuilder MapScenarioSignIn(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(SignInPath, SignInAsync);
        endpoints.MapGet(
            AccessDeniedPath,
            (HttpContext context) =>
                Page(
                    "Access denied",
                    $"<p>{Encode(context.User.Identity?.Name)} can't open this page.</p>"
                        + $"<p><a href=\"{SignInPath}\">Sign in as someone else</a></p>"
                )
        );

        return endpoints;
    }

    private static string Encode(string? text) => WebUtility.HtmlEncode(text ?? "");

    private static IResult Page(string title, string body, int statusCode = 200) =>
        Results.Content(
            $"<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>{title}</title></head>"
                + $"<body><h1>{title}</h1>{body}</body></html>",
            "text/html",
            statusCode: statusCode
        );

    // Without an email, lists the users; with one, signs that user in and returns to returnUrl
    private static async Task<IResult> SignInAsync(
        string? email,
        string? returnUrl,
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn
    )
    {
        var target = returnUrl is ['/', not '/' and not '\\', ..] or "/" ? returnUrl : "/";
        if (email is not null)
        {
            if (await users.FindByEmailAsync(email) is not { } user)
            {
                return Page("Sign in", $"<p>No user has the email {Encode(email)}.</p>", 404);
            }

            await signIn.SignInAsync(user, isPersistent: false);

            return Results.Redirect(target);
        }

        var items = new List<string>();
        foreach (var user in users.Users.OrderBy(user => user.Email).ToList())
        {
            var roles = string.Join(", ", await users.GetRolesAsync(user));
            var href =
                $"{SignInPath}?email={Uri.EscapeDataString(user.Email!)}&returnUrl={Uri.EscapeDataString(target)}";
            items.Add(
                $"<li><a href=\"{Encode(href)}\">{Encode(user.DisplayName)}</a> {Encode(user.Email)} ({Encode(roles)})</li>"
            );
        }

        return Page("Sign in", $"<ul>{string.Concat(items)}</ul>");
    }

    // Names the signed-in user by their display name rather than their user name, which is their email
    private sealed class DisplayNameClaimsPrincipalFactory(
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole> roles,
        IOptions<IdentityOptions> options
    ) : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(users, roles, options)
    {
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            if (identity.FindFirst(identity.NameClaimType) is { } name)
            {
                identity.RemoveClaim(name);
            }

            identity.AddClaim(new Claim(identity.NameClaimType, user.DisplayName));

            return identity;
        }
    }
}
