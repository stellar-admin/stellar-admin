namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows an enum member by its <c>[Display]</c> name.
/// </summary>
/// <remarks>
///     This is the default display for enum properties, except flags enums.
/// </remarks>
public sealed class EnumDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Enum";
}
