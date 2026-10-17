namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Identifies the handler that prepares data for a field editor.
/// </summary>
public interface IFieldEditor
{
    /// <summary>
    ///     Gets the type of the handler.
    /// </summary>
    Type HandlerType { get; }
}

/// <summary>
///     Associates a field editor with the handler that prepares its data.
/// </summary>
/// <typeparam name="THandler">The type of the handler.</typeparam>
public interface IFieldEditor<THandler> : IFieldEditor
    where THandler : IFieldEditorHandler
{
    Type IFieldEditor.HandlerType => typeof(THandler);
}
