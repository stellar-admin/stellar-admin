using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Widgets;

/// <summary>
///     Describes a resource's create page that a widget renders on.
/// </summary>
/// <typeparam name="TModel">The type of the model the create form binds to.</typeparam>
/// <remarks>
///     The create page has no record yet, so its widgets have only what every page has.
/// </remarks>
public sealed class CreateWidgetContext<TModel> : WidgetContext
{
    internal CreateWidgetContext(HttpContext httpContext)
        : base(httpContext) { }
}
