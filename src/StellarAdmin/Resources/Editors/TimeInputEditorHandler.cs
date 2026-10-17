namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a time of day input.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class TimeInputEditorHandler(TimeInputEditor editor)
    : FieldEditorHandler<TimeInputEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/TimeInput";
}
