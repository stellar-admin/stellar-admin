namespace StellarAdmin.Pro.Resources.Options;

/// <summary>
///     The configured options for a create page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class CreatePageOptions<TEntity> : FormPageOptions<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     The factory that creates the entity the page renders and binds, or <c>null</c> to
    ///     use the parameterless constructor of <typeparamref name="TEntity" />.
    /// </summary>
    public Func<TEntity>? InstanceFactory { get; internal set; }

    public CreatePageOptions(FormPageDefaults defaults)
        : base(defaults) { }

    /// <summary>Returns a new entity for the page to render and bind.</summary>
    public TEntity CreateInstance()
    {
        return InstanceFactory?.Invoke() ?? Activator.CreateInstance<TEntity>();
    }
}
