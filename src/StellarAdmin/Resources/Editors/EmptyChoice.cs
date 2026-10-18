namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Specifies whether a choice editor starts with an empty choice, which clears the value.
/// </summary>
public enum EmptyChoice
{
    /// <summary>
    ///     Adds an empty choice when the value is optional.
    /// </summary>
    /// <remarks>
    ///     A select also adds one while a required value is unset or matches no choice, so that
    ///     no other choice appears selected. A collection property never gets one.
    /// </remarks>
    Auto,

    /// <summary>
    ///     Always adds an empty choice.
    /// </summary>
    /// <remarks>
    ///     A collection property never gets one.
    /// </remarks>
    Include,

    /// <summary>
    ///     Never adds an empty choice.
    /// </summary>
    Omit,
}
