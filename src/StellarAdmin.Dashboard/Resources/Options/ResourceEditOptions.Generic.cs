namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures an edit form for a model.
/// </summary>
public sealed class ResourceEditOptions<TModel> : ResourceEditOptions
{
    /// <inheritdoc />
    public override Type ModelType => typeof(TModel);
}
