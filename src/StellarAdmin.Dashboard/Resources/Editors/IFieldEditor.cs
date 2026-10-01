namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Identifies the handler that prepares a field editor.
/// </summary>
public interface IFieldEditor
{
    /// <summary>
    ///     The handler type.
    /// </summary>
    Type HandlerType { get; }
}

/// <summary>
///     Associates a field editor with its handler.
/// </summary>
public interface IFieldEditor<THandler> : IFieldEditor
    where THandler : IFieldEditorHandler
{
    Type IFieldEditor.HandlerType => typeof(THandler);
}
