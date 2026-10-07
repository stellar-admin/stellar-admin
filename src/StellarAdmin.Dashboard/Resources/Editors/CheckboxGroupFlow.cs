namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     The order in which a checkbox group editor fills its columns.
/// </summary>
public enum CheckboxGroupFlow
{
    /// <summary>
    ///     Fills each column from top to bottom before starting the next.
    /// </summary>
    Down,

    /// <summary>
    ///     Fills each row from left to right before starting the next.
    /// </summary>
    Across,
}
