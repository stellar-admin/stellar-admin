namespace StellarAdmin.UI;

/// <summary>
///     Specifies where on the screen toasts appear.
/// </summary>
[GenerateDataAttributeText]
public enum ToasterPosition
{
    /// <summary>
    ///     Toasts appear in the top-left corner.
    /// </summary>
    TopLeft,

    /// <summary>
    ///     Toasts appear centered along the top edge.
    /// </summary>
    TopCenter,

    /// <summary>
    ///     Toasts appear in the top-right corner.
    /// </summary>
    TopRight,

    /// <summary>
    ///     Toasts appear in the bottom-left corner.
    /// </summary>
    BottomLeft,

    /// <summary>
    ///     Toasts appear centered along the bottom edge.
    /// </summary>
    BottomCenter,

    /// <summary>
    ///     Toasts appear in the bottom-right corner.
    /// </summary>
    BottomRight,
}
