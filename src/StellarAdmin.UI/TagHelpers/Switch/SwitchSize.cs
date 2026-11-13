namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a switch.
/// </summary>
[GenerateDataAttributeText]
public enum SwitchSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A smaller, more compact switch.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
