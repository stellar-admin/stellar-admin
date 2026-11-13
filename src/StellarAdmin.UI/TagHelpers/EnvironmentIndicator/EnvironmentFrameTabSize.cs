namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of the tab that names the environment on a <c>&lt;sa-environment-frame&gt;</c>.
/// </summary>
[GenerateDataAttributeText]
public enum EnvironmentFrameTabSize
{
    /// <summary>
    ///     A small tab.
    /// </summary>
    [DataAttributeText("sm")]
    Small,

    /// <summary>
    ///     A medium tab.
    /// </summary>
    [DataAttributeText("md")]
    Medium,

    /// <summary>
    ///     A large tab.
    /// </summary>
    [DataAttributeText("lg")]
    Large,

    /// <summary>
    ///     An extra large tab.
    /// </summary>
    [DataAttributeText("xl")]
    ExtraLarge,
}
