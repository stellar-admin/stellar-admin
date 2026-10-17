namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-stack&gt;</c> spreads its children vertically.
/// </summary>
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

internal static class StackJustifyExtensions
{
    extension(StackJustify justify)
    {
        public string GetClass() =>
            justify switch
            {
                StackJustify.Center => "sa-stack-justify-center",
                StackJustify.Start => "sa-stack-justify-start",
                StackJustify.End => "sa-stack-justify-end",
                StackJustify.SpaceBetween => "sa-stack-justify-between",
                StackJustify.SpaceAround => "sa-stack-justify-around",
                _ => "sa-stack-justify-start",
            };
    }
}
