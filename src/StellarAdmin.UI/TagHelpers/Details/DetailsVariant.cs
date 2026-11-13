namespace StellarAdmin.UI;

/// <summary>
///     The visual style of a <c>&lt;sa-details&gt;</c> list.
/// </summary>
[GenerateDataAttributeText]
public enum DetailsVariant
{
    /// <summary>
    ///     Lays out the fields in a grid with space between them.
    /// </summary>
    Default,

    /// <summary>
    ///     Stacks the fields in rows with a line between them.
    /// </summary>
    Divided,
}
