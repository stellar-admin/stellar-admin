namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The amount of horizontal spacing between the children of a <c>&lt;sa-group&gt;</c>.
/// </summary>
public enum GroupGap
{
    /// <summary>Extra-small spacing between children.</summary>
    ExtraSmall,

    /// <summary>Small spacing between children.</summary>
    Small,

    /// <summary>The default spacing between children.</summary>
    Default,

    /// <summary>Large spacing between children.</summary>
    Large,

    /// <summary>Extra-large spacing between children.</summary>
    ExtraLarge,
}

internal static class GroupGapExtensions
{
    extension(GroupGap gap)
    {
        public string GetClassName() =>
            gap switch
            {
                GroupGap.ExtraSmall => "sa-group-gap-xs",
                GroupGap.Small => "sa-group-gap-sm",
                GroupGap.Default => "sa-group-gap-md",
                GroupGap.Large => "sa-group-gap-lg",
                GroupGap.ExtraLarge => "sa-group-gap-xl",
                _ => "sa-group-gap-md",
            };
    }
}
