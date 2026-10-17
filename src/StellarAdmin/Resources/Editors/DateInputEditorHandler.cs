namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a date input.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class DateInputEditorHandler(DateInputEditor editor)
    : FieldEditorHandler<DateInputEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/DateInput";
}
