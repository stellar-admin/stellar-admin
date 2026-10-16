namespace StellarAdmin.Resources;

/// <summary>
///     What the index grid shows after an inline row is saved.
/// </summary>
public enum InlineRowAfterSave
{
    /// <summary>
    ///     Reloads the current page with its search, scope, sort and paging. A saved row that the page
    ///     doesn't show is confirmed by a toast.
    /// </summary>
    ReloadGrid,

    /// <summary>
    ///     Keeps the saved row where it was edited or added until the grid next reloads.
    /// </summary>
    KeepInPlace,
}
