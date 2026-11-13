namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of an alert.
/// </summary>
[GenerateDataAttributeText]
public enum AlertVariant
{
    /// <summary>
    ///     The standard alert style.
    /// </summary>
    Default,

    /// <summary>
    ///     A style that signals an error or a destructive outcome.
    /// </summary>
    Destructive,
}
