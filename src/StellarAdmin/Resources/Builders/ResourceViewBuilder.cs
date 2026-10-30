using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;
using StellarAdmin.Widgets;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's view page.
/// </summary>
/// <typeparam name="TModel">The type of the model the page displays.</typeparam>
public sealed class ResourceViewBuilder<TModel>
{
    private readonly Action<Action<ResourceViewOptions<TModel>>> _configure;
    private readonly IServiceCollection _services;

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

    internal ResourceViewBuilder(
        IServiceCollection services,
        Action<Action<ResourceViewOptions<TModel>>> configure
    )
    {
        _services = services;
        _configure = configure;
    }

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

    /// <summary>
    ///     Configures the widgets shown on the view page.
    /// </summary>
    /// <param name="configure">A delegate that adds the widgets.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A widget's <c>Placement</c> shows it above the page's fields, beside them or below
    ///     them. The widgets' delegates receive the record the page shows.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceViewBuilder<TModel> Widgets(
        Action<WidgetsBuilder<RecordWidgetContext<TModel>>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var widgets = new List<WidgetOptions>();
        configure(new(_services, widgets));
        _configure(options => options.Widgets.AddRange(widgets));

        return this;
    }
}
