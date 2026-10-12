namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     Displays a date, a time, or both.
/// </summary>
/// <remarks>
///     The default display for <see cref="DateTime" />, <see cref="DateTimeOffset" />,
///     <see cref="DateOnly" /> and <see cref="TimeOnly" /> properties.
/// </remarks>
public sealed class DateDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Date";

    /// <summary>
    ///     The format string, such as <c>d MMM yyyy</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to the property's <c>[DisplayFormat]</c>, then <c>d MMM yyyy</c> for dates,
    ///     <c>HH:mm</c> for times and <c>d MMM yyyy, HH:mm</c> for both.
    /// </remarks>
    public string? Format { get; set; }
}
