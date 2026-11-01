namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's create form.
/// </summary>
public abstract class ResourceCreateOptions : ResourceFormOptions
{
    /// <summary>
    ///     Gets the configuration of saving a new resource and starting the next one.
    /// </summary>
    /// <value>
    ///     The configuration, or <see langword="null" /> when the form only saves.
    /// </value>
    public ResourceAddAnotherOptions? AddAnother { get; internal set; }

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

    // Fills a new model the form shows, after it's created, such as with defaults that need services
    internal Func<IServiceProvider, object, CancellationToken, Task>? Initializer { get; set; }

    internal abstract object CreateModel();

    // A new model for a form the user fills in. A model that's only bound, to load a field's items or from a submitted
    // form, comes from CreateModel
    internal async Task<object> CreateModelAsync(
        IServiceProvider services,
        CancellationToken cancellationToken
    )
    {
        var model = CreateModel();
        if (Initializer is { } initializer)
        {
            await initializer(services, model, cancellationToken);
        }

        return model;
    }
}
