namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows a date, a time or both.
/// </summary>
/// <remarks>
///     This is the default display for <see cref="DateTime" />, <see cref="DateTimeOffset" />,
///     <see cref="DateOnly" /> and <see cref="TimeOnly" /> properties.
/// </remarks>
public sealed class DateDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Date";

    /// <summary>
    ///     Gets or sets the format string, such as <c>d MMM yyyy</c>.
    /// </summary>
    /// <value>
    ///     The format string, or <see langword="null" /> to use the property's
    ///     <c>[DisplayFormat]</c>. Without one, the format follows the current culture: dates use
    ///     its long date without the weekday and with abbreviated month names, such as
    ///     <c>MMM d, yyyy</c> in en-US, times use its short time, and values with both use the two
    ///     joined by a space. The default is <see langword="null" />.
    /// </value>
    public string? Format { get; set; }
}
