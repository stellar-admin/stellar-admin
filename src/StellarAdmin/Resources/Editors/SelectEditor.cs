namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures a select editor.
/// </summary>
public sealed class SelectEditor : ChoiceEditor
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Select";

    internal override bool KeepsEmptyChoiceWhileUnset => true;
}
