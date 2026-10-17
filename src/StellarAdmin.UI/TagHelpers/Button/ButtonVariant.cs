namespace StellarAdmin.UI;

/// <summary>
///     Specifies the visual style of a button.
/// </summary>
public enum ButtonVariant
{
    /// <summary>
    ///     The standard, primary style.
    /// </summary>
    Default,

    /// <summary>
    ///     A style that signals a destructive action.
    /// </summary>
    Destructive,

    /// <summary>
    ///     A border with a transparent background.
    /// </summary>
    Outline,

    /// <summary>
    ///     A quieter, secondary style.
    /// </summary>
    Secondary,

    /// <summary>
    ///     No background until the pointer is over the button.
    /// </summary>
    Ghost,

    /// <summary>
    ///     A style that looks like a link.
    /// </summary>
    Link,
}
