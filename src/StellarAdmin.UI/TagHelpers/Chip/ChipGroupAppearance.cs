namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a chip group looks.
/// </summary>
[GenerateDataAttributeText]
public enum ChipGroupAppearance
{
    /// <summary>
    ///     A wrapping row of chips on the background that contains it.
    /// </summary>
    Plain,

    /// <summary>
    ///     A box styled like an input, with a focus ring and an invalid ring.
    /// </summary>
    Input,
}
