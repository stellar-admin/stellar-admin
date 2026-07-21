using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The amount of vertical spacing between the children of a <c>&lt;sa-stack&gt;</c>.
/// </summary>
public enum StackGap
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

internal static class StackGapExtensions
{
    extension(StackGap gap)
    {
        public ThemeToken GetThemeToken() =>
            gap switch
            {
                StackGap.ExtraSmall => new ThemeToken("sa-stack-gap-xs"),
                StackGap.Small => new ThemeToken("sa-stack-gap-sm"),
                StackGap.Default => new ThemeToken("sa-stack-gap-md"),
                StackGap.Large => new ThemeToken("sa-stack-gap-lg"),
                StackGap.ExtraLarge => new ThemeToken("sa-stack-gap-xl"),
                _ => new ThemeToken("sa-stack-gap-md"),
            };
    }
}
