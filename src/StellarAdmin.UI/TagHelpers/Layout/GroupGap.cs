using StellarAdmin.UI.Theming;

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
        public ThemeToken GetThemeToken() =>
            gap switch
            {
                GroupGap.ExtraSmall => new ThemeToken("sa-group-gap-xs"),
                GroupGap.Small => new ThemeToken("sa-group-gap-sm"),
                GroupGap.Default => new ThemeToken("sa-group-gap-md"),
                GroupGap.Large => new ThemeToken("sa-group-gap-lg"),
                GroupGap.ExtraLarge => new ThemeToken("sa-group-gap-xl"),
                _ => new ThemeToken("sa-group-gap-md"),
            };
    }
}
