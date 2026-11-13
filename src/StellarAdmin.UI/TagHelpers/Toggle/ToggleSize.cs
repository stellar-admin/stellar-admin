namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a toggle.
/// </summary>
[GenerateDataAttributeText]
public enum ToggleSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A small toggle.
    /// </summary>
    [DataAttributeText("sm")]
    Small,

    /// <summary>
    ///     A large toggle.
    /// </summary>
    [DataAttributeText("lg")]
    Large,
}
