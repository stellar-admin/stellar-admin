using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's view page.
/// </summary>
public sealed class ResourceViewBuilder<TModel>
{
    private readonly Action<Action<ResourceViewOptions<TModel>>> _configure;

    /// <summary>
    ///     The layout of the page's sections.
    /// </summary>
    public FormSectionLayout? SectionLayout
    {
        set => _configure(options => options.SectionLayout = value);
    }

    /// <summary>
    ///     The page title.
    /// </summary>
    public string? Title
    {
        set => _configure(options => options.Title = value);
    }

    internal ResourceViewBuilder(Action<Action<ResourceViewOptions<TModel>>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Configures the page's fields.
    /// </summary>
    public ResourceViewBuilder<TModel> Fields(Action<ResourceViewFieldsBuilder<TModel>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(
            new ResourceViewPageFieldsBuilder<TModel>(action =>
                _configure(options => action(options))
            )
        );

        return this;
    }
}
