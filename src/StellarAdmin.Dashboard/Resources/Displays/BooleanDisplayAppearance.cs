namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     How a <see cref="BooleanDisplay" /> shows its value.
/// </summary>
public enum BooleanDisplayAppearance
{
    /// <summary>
    ///     A check for <c>true</c> and a muted cross for <c>false</c>.
    /// </summary>
    Icon,

    /// <summary>
    ///     The display's text.
    /// </summary>
    Text,

    /// <summary>
    ///     The icon followed by the text.
    /// </summary>
    IconAndText,
}
