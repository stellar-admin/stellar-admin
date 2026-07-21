namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Controls how a <c>&lt;sa-stack&gt;</c> aligns its children along the cross axis.
/// </summary>
public enum StackAlign
{
    /// <summary>Children stretch to fill the cross axis.</summary>
    Stretch,

    /// <summary>Children are centered on the cross axis.</summary>
    Center,

    /// <summary>Children are aligned to the start of the cross axis.</summary>
    Start,

    /// <summary>Children are aligned to the end of the cross axis.</summary>
    End,
}

internal static class StackAlignExtensions
{
    extension(StackAlign align)
    {
        public string GetClass() =>
            align switch
            {
                StackAlign.Stretch => "items-stretch",
                StackAlign.Center => "items-center",
                StackAlign.Start => "items-start",
                StackAlign.End => "items-end",
                _ => "items-stretch",
            };
    }
}
