namespace StellarAdmin.Resources.Options;

/// <summary>
///     Configures a resource's edit form.
/// </summary>
public abstract class ResourceEditOptions : ResourceFormOptions
{
    /// <summary>
    ///     The inline row configuration, or null when resources are edited on the edit page.
    /// </summary>
    public ResourceInlineEditOptions? InlineRow { get; internal set; }

    /// <summary>
    ///     The model used by the edit form.
    /// </summary>
    public abstract Type ModelType { get; }
}
