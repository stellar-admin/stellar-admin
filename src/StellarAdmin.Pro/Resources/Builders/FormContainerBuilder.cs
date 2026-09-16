using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures the contents of a form container.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class FormContainerBuilder<TEntity>
    where TEntity : class
{
    private readonly FormContainerOptions _options;

    internal FormContainerBuilder(FormContainerOptions options)
    {
        _options = options;
    }

    /// <summary>
    ///     Configures the container's fields and nested containers.
    /// </summary>
    public FormContainerBuilder<TEntity> Fields(Action<FormFieldsBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new FormFieldsBuilder<TEntity>(_options.MutableItems));

        return this;
    }
}
