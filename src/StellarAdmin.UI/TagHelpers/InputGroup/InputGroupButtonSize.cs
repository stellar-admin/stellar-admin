namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a button in an input group.
/// </summary>
[GenerateDataAttributeText]
public enum InputGroupButtonSize
{
    /// <summary>
    ///     An extra-small button.
    /// </summary>
    [DataAttributeText("xs")]
    ExtraSmall,

    /// <summary>
    ///     A small button.
    /// </summary>
    [DataAttributeText("sm")]
    Small,

    /// <summary>
    ///     An extra-small square button for a single icon.
    /// </summary>
    [DataAttributeText("icon-xs")]
    IconExtraSmall,

    /// <summary>
    ///     A small square button for a single icon.
    /// </summary>
    [DataAttributeText("icon-sm")]
    IconSmall,
}
