namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows text.
/// </summary>
/// <remarks>
///     Text marked <c>[DataType(DataType.MultilineText)]</c> keeps its line breaks. This is the
///     default display for strings.
/// </remarks>
public sealed class TextDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Text";

    /// <summary>
    ///     Gets or sets the number of lines shown before the text is truncated.
    /// </summary>
    /// <value>
    ///     The number of lines, or <see langword="null" /> to show all of the text. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The value is less than 1.
    /// </exception>
    public int? LineClamp
    {
        get;
        set
        {
            if (value is { } lines)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(lines, 1);
            }

            field = value;
        }
    }
}
