using StellarAdmin.Diagnostics;

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
    /// <remarks>
    ///     A value less than 1 is a configuration problem, and the setting keeps its value.
    /// </remarks>
    public int? LineClamp
    {
        get;
        set
        {
            if (value is < 1)
            {
                ResourceProblemSink.ReportInvalidNumber(
                    nameof(LineClamp),
                    value.Value,
                    "1 or more"
                );
                return;
            }

            field = value;
        }
    }
}
