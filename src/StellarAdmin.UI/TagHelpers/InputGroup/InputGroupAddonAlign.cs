namespace StellarAdmin.UI;

/// <summary>
///     Specifies where an input group add-on is placed relative to the input.
/// </summary>
[GenerateDataAttributeText]
public enum InputGroupAddonAlign
{
    /// <summary>
    ///     Before the input, on the same row.
    /// </summary>
    Start,

    /// <summary>
    ///     After the input, on the same row.
    /// </summary>
    End,

    /// <summary>
    ///     On its own row above the input.
    /// </summary>
    Top,

    /// <summary>
    ///     On its own row below the input.
    /// </summary>
    Bottom,
}
