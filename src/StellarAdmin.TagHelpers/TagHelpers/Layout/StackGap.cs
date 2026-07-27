namespace StellarAdmin.TagHelpers;

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
        public string GetClassName() =>
            gap switch
            {
                StackGap.ExtraSmall => "sa-stack-gap-xs",
                StackGap.Small => "sa-stack-gap-sm",
                StackGap.Default => "sa-stack-gap-md",
                StackGap.Large => "sa-stack-gap-lg",
                StackGap.ExtraLarge => "sa-stack-gap-xl",
                _ => "sa-stack-gap-md",
            };
    }
}
