namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     The configured options for an edit page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class EditPageOptions<TEntity> : FormPageOptions<TEntity>
    where TEntity : class
{
    public EditPageOptions(FormPageDefaults defaults)
        : base(defaults) { }
}
