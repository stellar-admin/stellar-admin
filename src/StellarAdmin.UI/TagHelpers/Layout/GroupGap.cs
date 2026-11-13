namespace StellarAdmin.UI;

/// <summary>
///     Specifies the horizontal space between the children of a <c>&lt;sa-group&gt;</c>.
/// </summary>
[GenerateDataAttributeText]
public enum GroupGap
{
    /// <summary>
    ///     The standard space.
    /// </summary>
    Default,

    /// <summary>
    ///     An extra-small space.
    /// </summary>
    [DataAttributeText("xs")]
    ExtraSmall,

    /// <summary>
    ///     A small space.
    /// </summary>
    [DataAttributeText("sm")]
    Small,

    /// <summary>
    ///     A large space.
    /// </summary>
    [DataAttributeText("lg")]
    Large,

    /// <summary>
    ///     An extra-large space.
    /// </summary>
    [DataAttributeText("xl")]
    ExtraLarge,
}
