namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's create form.
/// </summary>
public abstract class ResourceCreateOptions : ResourceFormOptions
{
    /// <summary>
    ///     Gets the configuration of the inline row that creates resources.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when resources are created on the create
    ///     page.
    /// </value>
    public ResourceInlineCreateOptions? InlineRow { get; internal set; }

    /// <summary>
    ///     Gets the type of the model the create form binds to.
    /// </summary>
    public abstract Type ModelType { get; }

    internal abstract object CreateModel();
}
