namespace StellarAdmin.Areas.StellarAdmin.ViewModels;

/// <summary>
///     Represents the search box of an index page.
/// </summary>
public sealed class ResourceIndexSearchViewModel
{
    /// <summary>
    ///     Gets the placeholder text of the search box.
    /// </summary>
    public string Placeholder { get; }

    /// <summary>
    ///     Gets the search term, or <see langword="null" /> when the list isn't searched.
    /// </summary>
    public string? Term { get; }

    internal ResourceIndexSearchViewModel(string? term, string placeholder)
    {
        Term = term;
        Placeholder = placeholder;
    }
}
