namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-stack&gt;</c> spreads its children vertically.
/// </summary>
[GenerateDataAttributeText]
public enum StackJustify
{
    /// <summary>
    ///     The children are centered on the main axis.
    /// </summary>
    Center,

    /// <summary>
    ///     The children are packed at the start of the main axis.
    /// </summary>
    Start,

    /// <summary>
    ///     The children are packed at the end of the main axis.
    /// </summary>
    End,

    /// <summary>
    ///     The children have equal space between them and none at the edges.
    /// </summary>
    SpaceBetween,

    /// <summary>
    ///     The children have equal space around each of them.
    /// </summary>
    SpaceAround,
}
