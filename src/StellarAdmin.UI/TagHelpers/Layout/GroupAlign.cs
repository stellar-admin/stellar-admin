namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Controls how a <c>&lt;sa-group&gt;</c> aligns its children along the cross axis.
/// </summary>
public enum GroupAlign
{
    /// <summary>Children stretch to fill the cross axis.</summary>
    Stretch,

    /// <summary>Children are centered on the cross axis.</summary>
    Center,

    /// <summary>Children are aligned to the start of the cross axis.</summary>
    Start,

    /// <summary>Children are aligned to the end of the cross axis.</summary>
    End,

    /// <summary>Children are aligned along their text baselines.</summary>
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
