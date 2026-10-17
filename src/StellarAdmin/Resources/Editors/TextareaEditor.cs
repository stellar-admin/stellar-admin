namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a multi-line text area editor.
/// </summary>
public sealed class TextareaEditor : FieldEditor
{
    /// <summary>
    ///     Gets or sets the hint text displayed while the text area is empty.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    ///     Gets or sets the number of visible lines of text.
    /// </summary>
    /// <value>
    ///     The number of lines, or <see langword="null" /> to grow with the content. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     A text area with a number of lines keeps that height and scrolls longer text.
    /// </remarks>
    public int? Rows { get; set; }

    /// <inheritdoc />
    public override string TemplateName => "Editors/Textarea";
}
