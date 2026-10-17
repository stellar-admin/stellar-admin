namespace StellarAdmin.UI;

/// <summary>
///     Specifies the horizontal space between the children of a <c>&lt;sa-group&gt;</c>.
/// </summary>
public enum GroupGap
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

internal static class GroupGapExtensions
{
    extension(GroupGap gap)
    {
        public string GetClassName() =>
            gap switch
            {
                GroupGap.Default => "sa-group-gap-md",
                GroupGap.ExtraSmall => "sa-group-gap-xs",
                GroupGap.Small => "sa-group-gap-sm",
                GroupGap.Large => "sa-group-gap-lg",
                GroupGap.ExtraLarge => "sa-group-gap-xl",
                _ => "sa-group-gap-md",
            };
    }
}
