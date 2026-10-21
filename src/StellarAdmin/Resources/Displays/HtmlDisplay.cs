using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Configures how an HTML value, such as the text of a rich text field, is displayed.
/// </summary>
/// <remarks>
///     <para>
///         Forms, details and view pages show the formatted text. Index columns show its text
///         without formatting, clamped to <see cref="LineClamp" /> lines.
///     </para>
///     <para>
///         The HTML is sanitized with <see cref="IRichTextSanitizer" /> before it is shown. This is
///         the default display for properties marked <c>[DataType(DataType.Html)]</c>.
///     </para>
/// </remarks>
public sealed class HtmlDisplay : FieldDisplay
{
    /// <summary>
    ///     Gets or sets the number of lines an index column shows before it cuts off the text.
    /// </summary>
    /// <value>
    ///     The number of lines. The default is 2.
    /// </value>
    /// <remarks>
    ///     A value less than 1 is a configuration problem, and the setting keeps its value.
    /// </remarks>
    public int LineClamp
    {
        get;
        set
        {
            if (value < 1)
            {
                ResourceProblemSink.ReportInvalidNumber(nameof(LineClamp), value, "1 or more");
                return;
            }

            field = value;
        }
    } = 2;

    /// <inheritdoc />
    public override string TemplateName => "Displays/Html";
}
