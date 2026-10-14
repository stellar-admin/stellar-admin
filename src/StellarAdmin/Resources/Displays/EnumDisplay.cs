namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Displays an enum member by its <c>[Display]</c> name.
/// </summary>
/// <remarks>
///     The default display for enum properties, except flags.
/// </remarks>
public sealed class EnumDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Enum";
}
