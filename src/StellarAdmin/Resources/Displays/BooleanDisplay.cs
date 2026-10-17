namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows a Boolean value as an icon, text or both.
/// </summary>
/// <remarks>
///     This is the default display for <see cref="bool" /> properties.
/// </remarks>
public sealed class BooleanDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Boolean";

    /// <summary>
    ///     Gets or sets how the value is shown.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the value is shown. The default is
    ///     <see cref="BooleanDisplayAppearance.Icon" />.
    /// </value>
    public BooleanDisplayAppearance Appearance { get; set; } = BooleanDisplayAppearance.Icon;

    /// <summary>
    ///     Gets or sets the text for <see langword="false" />.
    /// </summary>
    /// <value>
    ///     The text. The default is <c>No</c>.
    /// </value>
    /// <remarks>
    ///     When only the icon is shown, screen readers still announce the text.
    /// </remarks>
    public string FalseText { get; set; } = "No";

    /// <summary>
    ///     Gets or sets the text for <see langword="true" />.
    /// </summary>
    /// <value>
    ///     The text. The default is <c>Yes</c>.
    /// </value>
    /// <remarks>
    ///     When only the icon is shown, screen readers still announce the text.
    /// </remarks>
    public string TrueText { get; set; } = "Yes";
}
