namespace StellarAdmin.TagHelpers;

/// <summary>
///     The visual style of a bubble.
/// </summary>
public enum BubbleVariant
{
    /// <summary>A strongly emphasized bubble, for the current user's own messages.</summary>
    Default,

    /// <summary>A neutral bubble for standard conversation content.</summary>
    Secondary,

    /// <summary>A lower-emphasis bubble for supporting content.</summary>
    Muted,

    /// <summary>A bubble with a subtle tint of the primary color.</summary>
    Tinted,

    /// <summary>A bordered bubble with no fill.</summary>
    Outline,

    /// <summary>An unframed bubble, for content that should read as plain text.</summary>
    Ghost,

    /// <summary>A bubble styled to convey an error or a failed action.</summary>
    Destructive,
}

internal static class BubbleVariantExtensions
{
    extension(BubbleVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                BubbleVariant.Default => "default",
                BubbleVariant.Secondary => "secondary",
                BubbleVariant.Muted => "muted",
                BubbleVariant.Tinted => "tinted",
                BubbleVariant.Outline => "outline",
                BubbleVariant.Ghost => "ghost",
                BubbleVariant.Destructive => "destructive",
                _ => string.Empty,
            };
    }
}
