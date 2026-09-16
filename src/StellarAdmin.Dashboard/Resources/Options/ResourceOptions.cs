namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     The configured options for the management screens of one entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public abstract class ResourceOptions<TEntity>
    where TEntity : class
{
    /// <summary>The options for the create page.</summary>
    public CreatePageOptions<TEntity> CreatePage { get; }

    /// <summary>The options for deleting an entity.</summary>
    public DeleteOptions<TEntity> Delete { get; }

    /// <summary>The options for the edit page.</summary>
    public EditPageOptions<TEntity> EditPage { get; }

    /// <summary>The options for the index page.</summary>
    public IndexPageOptions<TEntity> IndexPage { get; }

    protected ResourceOptions(
        IndexPageDefaults indexDefaults,
        FormPageDefaults createDefaults,
        FormPageDefaults editDefaults,
        DeleteDefaults<TEntity> deleteDefaults
    )
    {
        IndexPage = new IndexPageOptions<TEntity>(indexDefaults);
        CreatePage = new CreatePageOptions<TEntity>(createDefaults);
        EditPage = new EditPageOptions<TEntity>(editDefaults);
        Delete = new DeleteOptions<TEntity>(deleteDefaults);
    }
}
