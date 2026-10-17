namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-group&gt;</c> spreads its children horizontally.
/// </summary>
public enum GroupJustify
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

internal static class GroupJustifyExtensions
{
    extension(GroupJustify justify)
    {
        public string GetClass() =>
            justify switch
            {
                GroupJustify.Center => "sa-group-justify-center",
                GroupJustify.Start => "sa-group-justify-start",
                GroupJustify.End => "sa-group-justify-end",
                GroupJustify.SpaceBetween => "sa-group-justify-between",
                GroupJustify.SpaceAround => "sa-group-justify-around",
                _ => "sa-group-justify-start",
            };
    }
}
