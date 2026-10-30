using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;
using StellarAdmin.Widgets;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's edit page.
/// </summary>
/// <typeparam name="TModel">The type of the model the edit form binds to.</typeparam>
public sealed class ResourceEditBuilder<TModel>
{
    private readonly Action<Action<ResourceEditOptions<TModel>>> _configure;
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the layout of the form's sections.
    /// </summary>
    /// <value>
    ///     The default is the layout set in <see cref="StellarAdminFormsOptions.SectionLayout" />.
    /// </value>
    public FormSectionLayout? SectionLayout
    {
        set => _configure(options => options.SectionLayout = value);
    }

    /// <summary>
    ///     Sets the submit button label.
    /// </summary>
    /// <value>
    ///     The default is the label set with <see cref="ResourceEditLabelsBuilder.SubmitLabel" />.
    /// </value>
    public string? SubmitLabel
    {
        set => _configure(options => options.SubmitLabel = value);
    }

    /// <summary>
    ///     Sets the page title.
    /// </summary>
    /// <value>
    ///     The default is the title set with <see cref="ResourceEditLabelsBuilder.Title" />.
    /// </value>
    public string? Title
    {
        set => _configure(options => options.Title = value);
    }

    internal ResourceEditBuilder(
        IServiceCollection services,
        Action<Action<ResourceEditOptions<TModel>>> configure
    )
    {
        _services = services;
        _configure = configure;
    }

    /// <summary>
    ///     Configures the form's fields.
    /// </summary>
    /// <param name="configure">A delegate that adds and configures the fields.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceEditBuilder<TModel> Fields(Action<ResourceFieldsBuilder<TModel>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(
            new ResourceFormFieldsBuilder<TModel>(action => _configure(options => action(options)))
        );

        return this;
    }

    /// <summary>
    ///     Edits resources in their index grid row instead of on the edit page.
    /// </summary>
    /// <returns>A builder that configures the inline row.</returns>
    /// <remarks>
    ///     Every editable field must match an index column, by name or through
    ///     <see cref="ResourceColumnBuilder{TResource}.EditsField(string)" />. The resource needs a
    ///     key. The edit page is not available.
    /// </remarks>
    public ResourceInlineEditBuilder UseInlineRow()
    {
        var source = ConfigurationSource.Capture();
        _configure(options => options.InlineRow ??= new() { Source = source });

        return new(configure => _configure(options => configure(options.InlineRow ??= new())));
    }

    /// <summary>
    ///     Edits resources in their index grid row instead of on the edit page.
    /// </summary>
    /// <param name="configure">A delegate that configures the inline row.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Every editable field must match an index column, by name or through
    ///     <see cref="ResourceColumnBuilder{TResource}.EditsField(string)" />. The resource needs a
    ///     key. The edit page is not available.
    /// </remarks>
    public ResourceEditBuilder<TModel> UseInlineRow(Action<ResourceInlineEditBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(UseInlineRow());

        return this;
    }

    /// <summary>
    ///     Configures the widgets shown on the edit page.
    /// </summary>
    /// <param name="configure">A delegate that adds the widgets.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A widget's <c>Placement</c> shows it above the form, beside it or below it. Widgets
    ///     beside the form stay in view as the form scrolls. The widgets' delegates receive the
    ///     record the form edits. A resource whose rows are edited inline has no edit page, so
    ///     its edit widgets are a configuration problem, and aren't shown.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceEditBuilder<TModel> Widgets(
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
