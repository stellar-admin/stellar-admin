using StellarAdmin.Dashboard.Resources.Editors;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Identifies the editor configured by field options.
/// </summary>
public interface IFieldEditorOptions
{
    /// <summary>
    ///     The editor type.
    /// </summary>
    Type EditorType { get; }
}

/// <summary>
///     Associates field options with an editor.
/// </summary>
public interface IFieldEditorOptions<TEditor> : IFieldEditorOptions
    where TEditor : IFieldEditor
{
    Type IFieldEditorOptions.EditorType => typeof(TEditor);
}
