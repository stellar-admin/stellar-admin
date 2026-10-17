namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents a scope tab of an index page.
/// </summary>
public sealed class ResourceIndexScopeViewModel
{
    /// <summary>
    ///     Gets a value that indicates whether the scope is selected.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the scope is selected; otherwise, <see langword="false" />.
    /// </value>
    public bool IsActive { get; }

    /// <summary>
    ///     Gets the value that the tab's link puts in the query string, or
    ///     <see langword="null" /> for the default scope.
    /// </summary>
    /// <remarks>
    ///     The default scope's link has no scope parameter.
    /// </remarks>
    public string? QueryValue { get; }

    /// <summary>
    ///     Gets the identifier of the scope.
    /// </summary>
    public string Slug { get; }

    /// <summary>
    ///     Gets the label of the tab.
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
