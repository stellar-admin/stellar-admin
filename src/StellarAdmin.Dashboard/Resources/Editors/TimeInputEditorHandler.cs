namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a time of day input.
/// </summary>
public sealed class TimeInputEditorHandler(TimeInputEditor editor)
    : FieldEditorHandler<TimeInputEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/TimeInput";
}
