using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
internal sealed class LogoutController(
    StellarAdminOptions options,
    IAuthenticationSchemeProvider authenticationSchemes
) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index()
    {
        var logout = options.UserMenu.Logout;
        var redirectUrl = logout.RedirectPath is { } redirectPath
            ? Url.Content(redirectPath)
            : Url.Action("Index", "Home") ?? "/";
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        // Without configured schemes, sign out of the scheme that signed the user in. The default sign-out
        // scheme falls back to the sign-in scheme, which with ASP.NET Core Identity is the external cookie.
        var schemes = logout.AuthenticationSchemes.ToList();
        if (
            schemes.Count == 0
            && await authenticationSchemes.GetDefaultAuthenticateSchemeAsync() is { } scheme
        )
        {
            schemes.Add(scheme.Name);
        }

        if (schemes.Count == 0)
        {
            await HttpContext.SignOutAsync(properties);
        }

        // In order, as MVC's SignOutResult does: a remote scheme such as OpenID Connect redirects to the
        // identity provider, which returns to the redirect URL afterwards
        foreach (var name in schemes)
        {
            await HttpContext.SignOutAsync(name, properties);
        }

        // The cookie handler only redirects from its own logout path, so the endpoint redirects unless a
        // handler already has
        if (Response.HasStarted || Response.Headers.Location.Count > 0)
        {
            return new EmptyResult();
        }

        return Redirect(redirectUrl);
    }
}
