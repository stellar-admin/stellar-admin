namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Identifies the handler that prepares a field editor.
/// </summary>
public interface IFieldEditor
{
    /// <summary>
    ///     Gets the type of the handler that prepares the editor.
    /// </summary>
    Type HandlerType { get; }
}

/// <summary>
///     Associates a field editor with its handler.
/// </summary>
/// <typeparam name="THandler">The type of the handler that prepares the editor.</typeparam>
public interface IFieldEditor<THandler> : IFieldEditor
    where THandler : IFieldEditorHandler
{
    Type IFieldEditor.HandlerType => typeof(THandler);
}
