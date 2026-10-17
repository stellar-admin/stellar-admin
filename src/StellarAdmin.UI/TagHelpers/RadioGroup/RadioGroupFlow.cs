namespace StellarAdmin.UI;

/// <summary>
///     Specifies the order in which a radio group fills its columns.
/// </summary>
public enum RadioGroupFlow
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
