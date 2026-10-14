namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Displays a boolean as an icon, text, or both.
/// </summary>
/// <remarks>
///     The default display for <see cref="bool" /> properties.
/// </remarks>
public sealed class BooleanDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Boolean";

    /// <summary>
    ///     How the value is shown.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="BooleanDisplayAppearance.Icon" />.
    /// </remarks>
    public BooleanDisplayAppearance Appearance { get; set; } = BooleanDisplayAppearance.Icon;

    /// <summary>
    ///     The text for <c>false</c>, which screen readers announce for the icon.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>No</c>.
    /// </remarks>
    public string FalseText { get; set; } = "No";

    /// <summary>
    ///     The text for <c>true</c>, which screen readers announce for the icon.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>Yes</c>.
    /// </remarks>
    public string TrueText { get; set; } = "Yes";
}
