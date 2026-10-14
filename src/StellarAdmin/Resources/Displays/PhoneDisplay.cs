namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Displays a phone number as a <c>tel:</c> link.
/// </summary>
/// <remarks>
///     The default display for properties marked <c>[Phone]</c>.
/// </remarks>
public sealed class PhoneDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Phone";
}
