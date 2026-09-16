using StellarAdmin.Pro.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures the edit page.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class EditPageBuilder<TEntity>
    where TEntity : class
{
    private readonly EditPageOptions<TEntity> _options;

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

    internal EditPageBuilder(EditPageOptions<TEntity> options)
    {
        _options = options;
    }

    /// <summary>Configures the fields of the form.</summary>
    public EditPageBuilder<TEntity> Fields(Action<FormFieldsBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new FormFieldsBuilder<TEntity>(_options.MutableItems));

        return this;
    }
}
