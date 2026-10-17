namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's create form for a model type.
/// </summary>
/// <typeparam name="TModel">The type of the model the create form binds to.</typeparam>
public sealed class ResourceCreateOptions<TModel> : ResourceCreateOptions
{
    /// <summary>
    ///     Gets or sets the delegate that creates the form's model.
    /// </summary>
    /// <value>
    ///     The delegate, or <see langword="null" /> to use the model's parameterless constructor.
    ///     The default is <see langword="null" />.
    /// </value>
    public Func<TModel>? Factory { get; set; }

    /// <inheritdoc />
    public override Type ModelType => typeof(TModel);

    /// <summary>
    ///     Creates a model for the form.
    /// </summary>
    /// <returns>
    ///     A model from <see cref="Factory" />, or from the model's parameterless constructor when
    ///     there is no factory.
    /// </returns>
    public TModel CreateInstance() =>
        Factory is { } factory ? factory() : Activator.CreateInstance<TModel>()!;

    internal override object CreateModel() => CreateInstance()!;
}
