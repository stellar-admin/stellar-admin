namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Displays a field as a one-time code input.
/// </summary>
public sealed class OneTimeCodeEditorHandler(OneTimeCodeEditor editor)
    : FieldEditorHandler<OneTimeCodeEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/OneTimeCode";
}
