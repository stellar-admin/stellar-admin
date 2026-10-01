namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a date input.
/// </summary>
public sealed class DateInputEditorHandler(DateInputEditor editor)
    : FieldEditorHandler<DateInputEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/DateInput";
}
