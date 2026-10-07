namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     How a toggle group editor displays its choices.
/// </summary>
public enum ToggleGroupAppearance
{
    /// <summary>
    ///     Separate rounded chips that show a check mark when on.
    /// </summary>
    Chips,

    /// <summary>
    ///     Toggles joined into a single connected group.
    /// </summary>
    Joined,

    /// <summary>
    ///     Separate toggle buttons.
    /// </summary>
    Buttons,
}
