namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows a number with digits of equal width.
/// </summary>
/// <remarks>
///     This is the default display for numeric properties.
/// </remarks>
public sealed class NumberDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Number";

    /// <summary>
    ///     Gets or sets the format string, such as <c>N2</c> or <c>C</c>.
    /// </summary>
    /// <value>
    ///     The format string, or <see langword="null" /> to use the property's
    ///     <c>[DisplayFormat]</c>, if it has one. The default is <see langword="null" />.
    /// </value>
    public string? Format { get; set; }
}
