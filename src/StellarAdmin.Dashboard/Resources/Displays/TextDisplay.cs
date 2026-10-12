namespace StellarAdmin.Dashboard.Resources.Displays;

/// <summary>
///     Displays text. Multiline text keeps its line breaks.
/// </summary>
/// <remarks>
///     The default display for strings.
/// </remarks>
public sealed class TextDisplay : FieldDisplay
{
    /// <inheritdoc />
    public override string TemplateName => "Displays/Text";

    /// <summary>
    ///     The number of lines shown before the text is truncated, or <c>null</c> to show all of it.
    /// </summary>
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
