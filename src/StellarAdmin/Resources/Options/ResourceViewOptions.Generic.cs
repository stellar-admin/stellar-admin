namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's view page for a model type.
/// </summary>
/// <typeparam name="TModel">The type of the model the view page displays.</typeparam>
public sealed class ResourceViewOptions<TModel> : ResourceViewOptions
{
    /// <inheritdoc />
    public override Type ModelType => typeof(TModel);
}
