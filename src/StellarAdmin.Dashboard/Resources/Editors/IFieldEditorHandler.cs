namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Prepares a form field for rendering with an editor template.
/// </summary>
public interface IFieldEditorHandler
{
    /// <summary>
    ///     The MVC editor template name.
    /// </summary>
    string TemplateName { get; }

    /// <summary>
    ///     Loads data needed to render the field.
    /// </summary>
    Task<object?> PrepareAsync(FieldEditorContext context, CancellationToken cancellationToken);
}

/// <summary>
///     Prepares a form field configured by the specified editor.
/// </summary>
public interface IFieldEditorHandler<TEditor> : IFieldEditorHandler
    where TEditor : FieldEditor;
