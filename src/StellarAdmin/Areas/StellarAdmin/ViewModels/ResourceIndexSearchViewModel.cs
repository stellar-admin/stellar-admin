namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The search box of an index page.
/// </summary>
public sealed class ResourceIndexSearchViewModel
{
    /// <summary>
    ///     The search box placeholder.
    /// </summary>
    public string Placeholder { get; }

    /// <summary>
    ///     The search term entered, or <c>null</c> when the list is unfiltered.
    /// </summary>
    public string? Term { get; }

    internal ResourceIndexSearchViewModel(string? term, string placeholder)
    {
        Term = term;
        Placeholder = placeholder;
    }
}
