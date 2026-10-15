namespace StellarAdmin.Resources.Options;

/// <summary>
///     Configures a resource's create form.
/// </summary>
public abstract class ResourceCreateOptions : ResourceFormOptions
{
    /// <summary>
    ///     The inline row configuration, or null when resources are created on the create page.
    /// </summary>
    public ResourceInlineCreateOptions? InlineRow { get; internal set; }

    /// <summary>
    ///     The model used by the create form.
    /// </summary>
    public abstract Type ModelType { get; }

    internal abstract object CreateModel();
}
