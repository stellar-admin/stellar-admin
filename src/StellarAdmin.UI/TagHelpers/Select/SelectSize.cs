namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a <c>&lt;sa-select&gt;</c>.
/// </summary>
[GenerateDataAttributeText]
public enum SelectSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A smaller, more compact size.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
