using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Builders;

/// <summary>
///     Configures the create page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class CreatePageBuilder<TEntity>
    where TEntity : class
{
    private readonly CreatePageOptions<TEntity> _options;

    /// <summary>
    ///     The layout of the form sections.
    /// </summary>
    /// <remarks>
    ///     Defaults to the application setting.
    /// </remarks>
    public FormSectionLayout? SectionLayout
    {
        get => _options.SectionLayout;
        set => _options.SectionLayout = value;
    }

    /// <summary>Sets the page subtitle, rendered below the title.</summary>
    /// <remarks>Defaults to <c>null</c>, which renders no subtitle.</remarks>
    public string? Subtitle
    {
        get => _options.Subtitle;
        set => _options.Subtitle = value;
    }

    /// <summary>Sets the page title.</summary>
    /// <remarks>Defaults to <c>null</c>, which uses the title of the resource.</remarks>
    public string? Title
    {
        get => _options.Title;
        set => _options.Title = value;
    }

    internal CreatePageBuilder(CreatePageOptions<TEntity> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Creates the new entity with <paramref name="factory" /> instead of the
    ///     parameterless constructor. Use it to set defaults the form does not cover, or for
    ///     an entity type without a parameterless constructor.
    /// </summary>
    /// <remarks>Repeat calls replace the factory.</remarks>
    public CreatePageBuilder<TEntity> CreateInstanceUsing(Func<TEntity> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.InstanceFactory = factory;

        return this;
    }

    /// <summary>Configures the fields of the form.</summary>
    public CreatePageBuilder<TEntity> Fields(Action<FormFieldsBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new FormFieldsBuilder<TEntity>(_options.MutableItems));

        return this;
    }
}
