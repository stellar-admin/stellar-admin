namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Whether a choice editor starts with an empty choice, which clears the value.
/// </summary>
public enum EmptyChoice
{
    /// <summary>
    ///     An optional value gets an empty choice and a required value doesn't. A select also shows one while a
    ///     required value is unset, so that no other choice appears selected. A collection property never gets one.
    /// </summary>
    Auto,

    /// <summary>
    ///     Always adds an empty choice, except for a collection property.
    /// </summary>
    Include,

    /// <summary>
    ///     Never adds an empty choice.
    /// </summary>
    Omit,
}
