namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of a bubble.
/// </summary>
[GenerateDataAttributeText]
public enum BubbleVariant
{
    /// <summary>
    ///     A strong style, for the current user's own messages.
    /// </summary>
    Default,

    /// <summary>
    ///     A neutral style, for standard messages.
    /// </summary>
    Secondary,

    /// <summary>
    ///     A quieter style, for supporting content.
    /// </summary>
    Muted,

    /// <summary>
    ///     A subtle tint of the primary color.
    /// </summary>
    Tinted,

    /// <summary>
    ///     A border with no fill.
    /// </summary>
    Outline,

    /// <summary>
    ///     No frame, for content that reads as plain text.
    /// </summary>
    Ghost,

    /// <summary>
    ///     A style that signals an error or a failed action.
    /// </summary>
    Destructive,
}
