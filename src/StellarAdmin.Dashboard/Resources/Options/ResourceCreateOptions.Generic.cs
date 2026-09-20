namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource's create page using a typed model.
/// </summary>
public sealed class ResourceCreateOptions<TModel> : ResourceCreateOptions
{
    /// <summary>
    ///     The factory used to instantiate the form model.
    /// </summary>
    /// <remarks>
    ///     When null, instances are created using the model's parameterless constructor.
    /// </remarks>
    public Func<TModel>? Factory { get; set; }

    /// <inheritdoc />
    public override Type ModelType => typeof(TModel);

    /// <summary>
    ///     Creates a form model using the configured factory or its parameterless constructor.
    /// </summary>
    public TModel CreateInstance() =>
        Factory is { } factory ? factory() : Activator.CreateInstance<TModel>()!;

    internal override object CreateModel() => CreateInstance()!;
}
