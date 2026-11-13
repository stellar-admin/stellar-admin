namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-group&gt;</c> aligns its children vertically.
/// </summary>
[GenerateDataAttributeText]
public enum GroupAlign
{
    /// <summary>
    ///     The children stretch to fill the cross axis.
    /// </summary>
    Stretch,

    /// <summary>
    ///     The children are centered on the cross axis.
    /// </summary>
    Center,

    /// <summary>
    ///     The children align to the start of the cross axis.
    /// </summary>
    Start,

    /// <summary>
    ///     The children align to the end of the cross axis.
    /// </summary>
    End,

    /// <summary>
    ///     The children align on their text baselines.
    /// </summary>
    Baseline,
}
