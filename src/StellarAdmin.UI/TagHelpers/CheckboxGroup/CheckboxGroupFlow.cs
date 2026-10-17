namespace StellarAdmin.UI;

/// <summary>
///     Specifies the order in which a checkbox group fills its columns.
/// </summary>
public enum CheckboxGroupFlow
{
    /// <summary>
    ///     Fills each row from left to right before starting the next.
    /// </summary>
    Across,

    /// <summary>
    ///     Fills each column from top to bottom before starting the next.
    /// </summary>
    Down,
}
