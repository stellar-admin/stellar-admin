namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a local date and time input.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class DateTimeInputEditorHandler(DateTimeInputEditor editor)
    : FieldEditorHandler<DateTimeInputEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/DateTimeInput";
}
