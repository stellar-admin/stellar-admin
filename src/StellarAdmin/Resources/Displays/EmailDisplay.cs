namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows an email address as a <c>mailto:</c> link.
/// </summary>
/// <remarks>
///     This is the default display for properties marked <c>[EmailAddress]</c>.
/// </remarks>
public sealed class EmailDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Email";
}
