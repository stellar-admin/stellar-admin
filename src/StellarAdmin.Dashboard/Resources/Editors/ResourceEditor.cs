using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures an editor template for a resource form field.
/// </summary>
public abstract class ResourceEditor : EditorOptions
{
    /// <summary>
    ///     The MVC editor template name. Defaults to the editor type's name.
    /// </summary>
    public virtual string TemplateName => GetType().Name;

    /// <summary>
    ///     Loads data needed when rendering the editor for a request.
    /// </summary>
    public virtual Task<object?> PrepareAsync(
        IServiceProvider services,
        CancellationToken cancellationToken
    ) => Task.FromResult<object?>(null);
}
