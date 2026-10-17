namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of a badge.
/// </summary>
public enum BadgeVariant
{
    /// <summary>
    ///     The standard, solid style.
    /// </summary>
    Default,

    /// <summary>
    ///     A muted, secondary style.
    /// </summary>
    Secondary,

    /// <summary>
    ///     A style that signals an error or a destructive outcome.
    /// </summary>
    Destructive,

    /// <summary>
    ///     A transparent background with a visible border.
    /// </summary>
    Outline,

    /// <summary>
    ///     No background until the pointer is over the badge.
    /// </summary>
    Ghost,

    /// <summary>
    ///     A style that looks like a link.
    /// </summary>
    Link,
}
