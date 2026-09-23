namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Prepares a form field for rendering with an editor template.
/// </summary>
public interface IFieldEditor
{
    /// <summary>
    ///     The MVC editor template name.
    /// </summary>
    string TemplateName { get; }

    /// <summary>
    ///     Loads data needed to render the field.
    /// </summary>
    Task<object?> PrepareAsync(CancellationToken cancellationToken);
}

/// <summary>
///     Prepares a form field using the specified options.
/// </summary>
public interface IFieldEditor<TOptions> : IFieldEditor;
