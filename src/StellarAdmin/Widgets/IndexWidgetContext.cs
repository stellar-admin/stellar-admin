using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Widgets;

/// <summary>
///     Describes a resource's index page that a widget renders on.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <remarks>
///     The index page renders its widgets again when the search changes, so their delegates
///     receive the new search.
/// </remarks>
public sealed class IndexWidgetContext<TResource> : WidgetContext
{
    /// <summary>
    ///     Gets the identifier of the scope the index is filtered by.
    /// </summary>
    /// <value>
    ///     The identifier, or <see langword="null" /> when the index has no scopes.
    /// </value>
    public string? Scope { get; }

    /// <summary>
    ///     Gets the text the index is searched for.
    /// </summary>
    /// <value>
    ///     The trimmed text, or <see langword="null" /> when the index isn't searched.
    /// </value>
    public string? Search { get; }

    internal IndexWidgetContext(HttpContext httpContext, string? scope, string? search)
        : base(httpContext)
    {
        Scope = scope;
        Search = search;
    }
}
