namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures a select editor.
/// </summary>
public sealed class SelectEditor : ChoiceEditor, IFieldEditor<SelectEditorHandler>
{
    internal override bool KeepsEmptyChoiceWhileUnset => true;
}
