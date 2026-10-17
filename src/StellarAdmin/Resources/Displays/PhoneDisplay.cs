namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows a phone number as a <c>tel:</c> link.
/// </summary>
/// <remarks>
///     A value without digits shows as text. This is the default display for properties marked
///     <c>[Phone]</c>.
/// </remarks>
public sealed class PhoneDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Phone";
}
