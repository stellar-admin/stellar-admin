namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Where a toggle group editor's chips show their check mark while on.
/// </summary>
public enum ToggleGroupCheckPlacement
{
    /// <summary>
    ///     In place of the choice's media, so the chip keeps its width. A chip without media shows it before the text.
    /// </summary>
    ReplaceMedia,

    /// <summary>
    ///     Before the choice's media and text.
    /// </summary>
    Start,

    /// <summary>
    ///     After the choice's text.
    /// </summary>
    End,
}
