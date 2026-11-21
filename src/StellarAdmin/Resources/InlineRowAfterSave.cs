namespace StellarAdmin.Resources;

/// <summary>
///     Specifies what a table shows after an inline row is saved.
/// </summary>
public enum InlineRowAfterSave
{
    /// <summary>
    ///     Reloads the current page with its search, scope, sort and paging. A toast confirms a
    ///     saved row that the page doesn't show.
    /// </summary>
    ReloadGrid,

    /// <summary>
    ///     Keeps the saved row where it was edited or added until the grid next reloads.
    /// </summary>
    KeepInPlace,
}
