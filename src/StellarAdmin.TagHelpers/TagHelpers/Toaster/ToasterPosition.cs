namespace StellarAdmin.TagHelpers;

/// <summary>
///     The corner or edge of the screen where a <c>&lt;sa-toaster&gt;</c> shows its toasts.
/// </summary>
public enum ToasterPosition
{
    /// <summary>Toasts appear in the top-left corner.</summary>
    TopLeft,

    /// <summary>Toasts appear centered along the top edge.</summary>
    TopCenter,

    /// <summary>Toasts appear in the top-right corner.</summary>
    TopRight,

    /// <summary>Toasts appear in the bottom-left corner.</summary>
    BottomLeft,

    /// <summary>Toasts appear centered along the bottom edge.</summary>
    BottomCenter,

    /// <summary>Toasts appear in the bottom-right corner.</summary>
    BottomRight,
}

internal static class ToasterPositionExtensions
{
    extension(ToasterPosition position)
    {
        public string GetDataAttributeText() =>
            position switch
            {
                ToasterPosition.TopLeft => "top-left",
                ToasterPosition.TopCenter => "top-center",
                ToasterPosition.TopRight => "top-right",
                ToasterPosition.BottomLeft => "bottom-left",
                ToasterPosition.BottomCenter => "bottom-center",
                ToasterPosition.BottomRight => "bottom-right",
                _ => string.Empty,
            };
    }
}
