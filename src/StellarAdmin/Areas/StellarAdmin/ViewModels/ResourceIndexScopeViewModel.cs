namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     A scope tab of an index page.
/// </summary>
public sealed class ResourceIndexScopeViewModel
{
    /// <summary>
    ///     Whether the scope is the one currently applied.
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    ///     The value the tab's link puts in the query string.
    ///     It is <c>null</c> for the default scope, so its link has no scope parameter.
    /// </summary>
    public string? QueryValue { get; }

    /// <summary>
    ///     The value identifying the scope in the query string.
    /// </summary>
    public string Slug { get; }

    /// <summary>
    ///     The tab label.
    /// </summary>
    public string Title { get; }

    internal ResourceIndexScopeViewModel(
        string slug,
        string title,
        bool isActive,
        string? queryValue
    )
    {
        Slug = slug;
        Title = title;
        IsActive = isActive;
        QueryValue = queryValue;
    }
}
