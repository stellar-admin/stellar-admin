namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays a field as a one-time code input.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
public sealed class OneTimeCodeEditorHandler(OneTimeCodeEditor editor)
    : FieldEditorHandler<OneTimeCodeEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/OneTimeCode";
}
