namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Specifies where a toggle group editor's chips show their check mark while on.
/// </summary>
public enum ToggleGroupCheckPlacement
{
    /// <summary>
    ///     In place of the choice's media, so the chip keeps its width.
    /// </summary>
    /// <remarks>
    ///     A chip without media shows the check mark before the text.
    /// </remarks>
    ReplaceMedia,

    /// <summary>
    ///     Before the choice's media and text.
    /// </summary>
    Start,

    /// <summary>
    ///     After the choice's title.
    /// </summary>
    End,
}
