namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a form section places its heading and fields.
/// </summary>
[GenerateDataAttributeText]
public enum FormSectionLayout
{
    /// <summary>
    ///     Displays the heading above the fields.
    /// </summary>
    Stacked,

    /// <summary>
    ///     Displays the heading beside the fields when space permits.
    /// </summary>
    Split,

    /// <summary>
    ///     Displays the heading and fields inside a bordered card.
    /// </summary>
    Card,
}
