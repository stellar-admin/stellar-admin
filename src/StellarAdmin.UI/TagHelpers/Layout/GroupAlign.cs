namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-group&gt;</c> aligns its children vertically.
/// </summary>
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

internal static class GroupAlignExtensions
{
    extension(GroupAlign align)
    {
        public string GetClass() =>
            align switch
            {
                GroupAlign.Stretch => "sa-group-align-stretch",
                GroupAlign.Center => "sa-group-align-center",
                GroupAlign.Start => "sa-group-align-start",
                GroupAlign.End => "sa-group-align-end",
                GroupAlign.Baseline => "sa-group-align-baseline",
                _ => "sa-group-align-start",
            };
    }
}
