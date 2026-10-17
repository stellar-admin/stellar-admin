using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's view page.
/// </summary>
/// <typeparam name="TModel">The type of the model the page displays.</typeparam>
public sealed class ResourceViewBuilder<TModel>
{
    private readonly Action<Action<ResourceViewOptions<TModel>>> _configure;

    /// <summary>
    ///     Sets the layout of the page's sections.
    /// </summary>
    /// <value>
    ///     The layout, or <see langword="null" /> to use
    ///     <see cref="StellarAdminFormsOptions.SectionLayout" />. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     A section's own layout overrides the page's.
    /// </remarks>
    public FormSectionLayout? SectionLayout
    {
        set => _configure(options => options.SectionLayout = value);
    }

    /// <summary>
    ///     Sets the page title.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceViewLabelsBuilder.Title" />. The default is <see langword="null" />.
    /// </value>
    public string? Title
    {
        set => _configure(options => options.Title = value);
    }

    internal ResourceViewBuilder(Action<Action<ResourceViewOptions<TModel>>> configure) =>
        _configure = configure;

    /// <summary>
    ///     Configures the page's fields.
    /// </summary>
    /// <param name="configure">A delegate that adds and configures the fields.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
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
