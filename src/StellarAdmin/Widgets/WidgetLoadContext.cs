using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Widgets;

/// <summary>
///     Describes the request that a widget's handler loads the widget's model in.
/// </summary>
/// <remarks>
///     A widget that loads lazily runs its handler in a request of its own, after the page has
///     rendered, so the context describes that request rather than the page's.
/// </remarks>
public sealed class WidgetLoadContext
{
    /// <summary>
    ///     Gets the HTTP context of the request.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    ///     Gets the name of the widget.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the user who made the request.
    /// </summary>
    public ClaimsPrincipal User => HttpContext.User;

    internal WidgetLoadContext(HttpContext httpContext, string name)
    {
        HttpContext = httpContext;
        Name = name;
    }
}
