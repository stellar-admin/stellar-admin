namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Displays a number with figures of equal width.
/// </summary>
/// <remarks>
///     The default display for numeric properties.
/// </remarks>
public sealed class NumberDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Number";

    /// <summary>
    ///     The format string, such as <c>N2</c> or <c>C</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the property's <c>[DisplayFormat]</c>, then the number as is.
    /// </remarks>
    public string? Format { get; set; }
}
