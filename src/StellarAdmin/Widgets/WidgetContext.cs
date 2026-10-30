using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Widgets;

/// <summary>
///     Describes the page that a widget renders on.
/// </summary>
/// <remarks>
///     A widget's <c>Parameters</c> and <c>ShowWhen</c> delegates receive the context when the
///     page renders. A widget without a handler receives it as its partial view's model.
/// </remarks>
public class WidgetContext
{
    /// <summary>
    ///     Gets the HTTP context of the page's request.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    ///     Gets the services of the page's request.
    /// </summary>
    public IServiceProvider Services => HttpContext.RequestServices;

    /// <summary>
    ///     Gets the user who requested the page.
    /// </summary>
    public ClaimsPrincipal User => HttpContext.User;

    internal WidgetContext(HttpContext httpContext) => HttpContext = httpContext;
}
