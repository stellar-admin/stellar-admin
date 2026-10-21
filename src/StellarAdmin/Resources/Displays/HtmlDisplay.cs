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
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The value is less than 1.
    /// </exception>
    public int LineClamp
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

            field = value;
        }
    } = 2;

    /// <inheritdoc />
    public override string TemplateName => "Displays/Html";
}
