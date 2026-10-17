namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Specifies how a <see cref="BooleanDisplay" /> shows its value.
/// </summary>
public enum BooleanDisplayAppearance
{
    /// <summary>
    ///     Shows an icon for <see langword="true" /> and another for <see langword="false" />.
    /// </summary>
    Icon,

    /// <summary>
    ///     Shows the display's text.
    /// </summary>
    Text,

    /// <summary>
    ///     Shows the icon followed by the text.
    /// </summary>
    IconAndText,
}
