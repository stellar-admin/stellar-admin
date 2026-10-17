namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-stack&gt;</c> aligns its children horizontally.
/// </summary>
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

internal static class StackAlignExtensions
{
    extension(StackAlign align)
    {
        public string GetClass() =>
            align switch
            {
                StackAlign.Stretch => "sa-stack-align-stretch",
                StackAlign.Center => "sa-stack-align-center",
                StackAlign.Start => "sa-stack-align-start",
                StackAlign.End => "sa-stack-align-end",
                _ => "sa-stack-align-stretch",
            };
    }
}
