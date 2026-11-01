using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;
using StellarAdmin.Widgets;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a resource's create page.
/// </summary>
/// <typeparam name="TModel">The type of the model the create form binds to.</typeparam>
public class ResourceCreateBuilder<TModel>
{
    private readonly Action<Action<ResourceCreateOptions<TModel>>> _configure;
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
    ///     The default is the label set with
    ///     <see cref="ResourceCreateLabelsBuilder.SubmitLabel" />.
    /// </value>
    public string? SubmitLabel
    {
        set => _configure(options => options.SubmitLabel = value);
    }

    /// <summary>
    ///     Sets the page title.
    /// </summary>
    /// <value>
    ///     The default is the title set with <see cref="ResourceCreateLabelsBuilder.Title" />.
    /// </value>
    public string? Title
    {
        set => _configure(options => options.Title = value);
    }

    internal ResourceCreateBuilder(
        IServiceCollection services,
        Action<Action<ResourceCreateOptions<TModel>>> configure
    )
    {
        _services = services;
        _configure = configure;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceCreateBuilder{TModel}" /> class
    ///     that configures the same create page as another builder.
    /// </summary>
    /// <param name="builder">The builder whose create page to configure.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    protected ResourceCreateBuilder(ResourceCreateBuilder<TModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _services = builder._services;
        _configure = builder._configure;
    }

    /// <summary>
    ///     Adds a button that saves the new resource and starts the next one.
    /// </summary>
    /// <returns>A builder that configures saving and adding another.</returns>
    /// <remarks>
    ///     The create page and the inline row both show the button. Ctrl+Shift+Enter or
    ///     Cmd+Shift+Enter presses it.
    /// </remarks>
    public ResourceAddAnotherBuilder<TModel> EnableAddAnother()
    {
        _configure(options => options.AddAnother ??= new());

        return new(configure => _configure(options => configure(options.AddAnother ??= new())));
    }

    /// <summary>
    ///     Adds and configures a button that saves the new resource and starts the next one.
    /// </summary>
    /// <param name="configure">A delegate that configures saving and adding another.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The create page and the inline row both show the button. Ctrl+Shift+Enter or
    ///     Cmd+Shift+Enter presses it.
    /// </remarks>
    public ResourceCreateBuilder<TModel> EnableAddAnother(
        Action<ResourceAddAnotherBuilder<TModel>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(EnableAddAnother());

        return this;
    }

    /// <summary>
    ///     Configures the form's fields.
    /// </summary>
    /// <param name="configure">A delegate that adds and configures the fields.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCreateBuilder<TModel> Fields(Action<ResourceFieldsBuilder<TModel>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(
            new ResourceFormFieldsBuilder<TModel>(action => _configure(options => action(options)))
        );

        return this;
    }

    /// <summary>
    ///     Sets the function that creates the model for the create form.
    /// </summary>
    /// <param name="factory">A function that returns a new model.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="factory" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Without a factory, the model is created with its parameterless constructor.
    /// </remarks>
    public ResourceCreateBuilder<TModel> UseFactory(Func<TModel> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _configure(options => options.Factory = factory);

        return this;
    }

    /// <summary>
    ///     Creates resources in a new row of the index grid instead of on the create page.
    /// </summary>
    /// <returns>A builder that configures the inline row.</returns>
    /// <remarks>
    ///     Every editable field must match an index column, by name or through
    ///     <see cref="ResourceColumnBuilder{TResource}.EditsField(string)" />. Every
    ///     <c>[Required]</c> model property must have an editable field. The resource needs a key.
    ///     The create page is not available.
    /// </remarks>
    public ResourceInlineCreateBuilder UseInlineRow()
    {
        var source = ConfigurationSource.Capture();
        _configure(options => options.InlineRow ??= new() { Source = source });

        return new(configure => _configure(options => configure(options.InlineRow ??= new())));
    }

    /// <summary>
    ///     Creates resources in a new row of the index grid instead of on the create page.
    /// </summary>
    /// <param name="configure">A delegate that configures the inline row.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Every editable field must match an index column, by name or through
    ///     <see cref="ResourceColumnBuilder{TResource}.EditsField(string)" />. Every
    ///     <c>[Required]</c> model property must have an editable field. The resource needs a key.
    ///     The create page is not available.
    /// </remarks>
    public ResourceCreateBuilder<TModel> UseInlineRow(Action<ResourceInlineCreateBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(UseInlineRow());

        return this;
    }

    /// <summary>
    ///     Configures the widgets shown on the create page.
    /// </summary>
    /// <param name="configure">A delegate that adds the widgets.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A widget's <c>Placement</c> shows it above the form, beside it or below it. Widgets
    ///     beside the form stay in view as the form scrolls. A resource whose rows are created
    ///     inline has no create page, so its create widgets are a configuration problem, and
    ///     aren't shown.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceCreateBuilder<TModel> Widgets(
        Action<WidgetsBuilder<CreateWidgetContext<TModel>>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var widgets = new List<WidgetOptions>();
        configure(new(_services, widgets));
        _configure(options => options.Widgets.AddRange(widgets));

        return this;
    }

    /// <summary>
    ///     Sets the function that fills a new model before the create form shows it.
    /// </summary>
    /// <param name="initializer">
    ///     A function that receives the request's services and the new model.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="initializer" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The function runs after the factory, wherever a create form is shown: on the create
    ///     page, in a new inline row, in the create sheet of a lookup, and in the next form after
    ///     saving and adding another, before the kept fields' values. It doesn't run for a
    ///     submitted form.
    /// </remarks>
    protected void UseInitializer(
        Func<IServiceProvider, TModel, CancellationToken, Task> initializer
    )
    {
        ArgumentNullException.ThrowIfNull(initializer);

        _configure(options =>
            options.Initializer = (services, model, cancellationToken) =>
                initializer(services, (TModel)model, cancellationToken)
        );
    }

    /// <summary>
    ///     Sets the property of the model that holds the resource, when the model wraps the
    ///     resource with fields of its own.
    /// </summary>
    /// <param name="propertyName">The name of the property that holds the resource.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="propertyName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="propertyName" /> is empty.
    /// </exception>
    /// <remarks>
    ///     Index columns match the resource's fields without the property's name, so the
    ///     <c>Product.Name</c> field is edited inline by the <c>Name</c> column.
    /// </remarks>
    protected void UseResourceProperty(string propertyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        _configure(options => options.ResourcePropertyName = propertyName);
    }
}
