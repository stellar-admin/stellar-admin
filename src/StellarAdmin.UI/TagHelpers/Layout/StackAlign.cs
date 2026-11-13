namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-stack&gt;</c> aligns its children horizontally.
/// </summary>
[GenerateDataAttributeText]
public enum StackAlign
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
}
