namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     Displays an email address as a <c>mailto:</c> link.
/// </summary>
/// <remarks>
///     The default display for properties marked <c>[EmailAddress]</c>.
/// </remarks>
public sealed class EmailDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Email";
}
