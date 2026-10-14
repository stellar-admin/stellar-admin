namespace StellarAdmin.Resources.Options;

/// <summary>
///     Configures a view page for a model.
/// </summary>
public sealed class ResourceViewOptions<TModel> : ResourceViewOptions
{
    /// <inheritdoc />
    public override Type ModelType => typeof(TModel);
}
