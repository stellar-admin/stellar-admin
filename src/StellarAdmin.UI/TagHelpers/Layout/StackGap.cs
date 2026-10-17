namespace StellarAdmin.UI;

/// <summary>
///     Specifies the vertical space between the children of a <c>&lt;sa-stack&gt;</c>.
/// </summary>
public enum StackGap
{
    /// <summary>
    ///     The standard space.
    /// </summary>
    Default,

    /// <summary>
    ///     An extra-small space.
    /// </summary>
    ExtraSmall,

    /// <summary>
    ///     A small space.
    /// </summary>
    Small,

    /// <summary>
    ///     A large space.
    /// </summary>
    Large,

    /// <summary>
    ///     An extra-large space.
    /// </summary>
    ExtraLarge,
}

internal static class StackGapExtensions
{
    extension(StackGap gap)
    {
        public string GetClassName() =>
            gap switch
            {
                StackGap.Default => "sa-stack-gap-md",
                StackGap.ExtraSmall => "sa-stack-gap-xs",
                StackGap.Small => "sa-stack-gap-sm",
                StackGap.Large => "sa-stack-gap-lg",
                StackGap.ExtraLarge => "sa-stack-gap-xl",
                _ => "sa-stack-gap-md",
            };
    }
}
