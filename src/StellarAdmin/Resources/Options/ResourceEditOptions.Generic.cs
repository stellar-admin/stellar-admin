namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores the configuration of a resource's edit form for a model type.
/// </summary>
/// <typeparam name="TModel">The type of the model the edit form binds to.</typeparam>
public sealed class ResourceEditOptions<TModel> : ResourceEditOptions
{
    /// <inheritdoc />
    public override Type ModelType => typeof(TModel);
}
