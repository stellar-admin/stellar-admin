using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;
using StellarAdmin.Widgets;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Provides the settings shared by the builders that configure a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <typeparam name="TBuilder">
///     The type of the derived builder that methods return for chaining.
/// </typeparam>
public abstract class ResourceIndexBuilderBase<TResource, TBuilder>
    where TBuilder : ResourceIndexBuilderBase<TResource, TBuilder>
{
    /// <summary>
    ///     Sets the label of the cancel button on an inline row.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.CancelLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? CancelLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.CancelLabel = value
            );
    }

    /// <summary>
    ///     Sets the label of the create button.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.CreateLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? CreateLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.CreateLabel = value
            );
    }

    /// <summary>
    ///     Sets the message of the toast shown when a resource is added in a new row or saved with
    ///     the add another button.
    /// </summary>
    /// <value>
    ///     The message, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.CreatedMessage" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? CreatedMessage
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.CreatedMessage = value
            );
    }

    /// <summary>
    ///     Sets the label of the delete button.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.DeleteLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? DeleteLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.DeleteLabel = value
            );
    }

    /// <summary>
    ///     Sets the label of the edit link.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.EditLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? EditLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.EditLabel = value
            );
    }

    /// <summary>
    ///     Sets the description of the toast shown when a saved inline row isn't on the reloaded
    ///     page.
    /// </summary>
    /// <value>
    ///     The description, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.NotShownMessage" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? NotShownMessage
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.NotShownMessage = value
            );
    }

    /// <summary>
    ///     Sets the label of the save button on an inline row.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.SaveLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? SaveLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.SaveLabel = value
            );
    }

    /// <summary>
    ///     Sets the message of the toast shown when an edited inline row isn't on the reloaded
    ///     page.
    /// </summary>
    /// <value>
    ///     The message, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.SavedMessage" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? SavedMessage
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.SavedMessage = value
            );
    }

    /// <summary>
    ///     Sets the page title.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.Title" />. The default is <see langword="null" />.
    /// </value>
    public string? Title
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options => options.Index.Title = value);
    }

    /// <summary>
    ///     Sets the label of the view link.
    /// </summary>
    /// <value>
    ///     The label, or <see langword="null" /> to use the default from
    ///     <see cref="ResourceIndexLabelsBuilder.ViewLabel" />. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? ViewLabel
    {
        set =>
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.ViewLabel = value
            );
    }

    /// <summary>
    ///     Gets the service collection the builder adds the resource's configuration to.
    /// </summary>
    protected IServiceCollection Services { get; }

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="ResourceIndexBuilderBase{TResource, TBuilder}" /> class.
    /// </summary>
    /// <param name="services">
    ///     The service collection to add the resource's configuration to.
    /// </param>
    protected ResourceIndexBuilderBase(IServiceCollection services) => Services = services;

    /// <summary>
    ///     Configures the widgets shown on the index page.
    /// </summary>
    /// <param name="configure">A delegate that adds the widgets.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     A widget's <c>Placement</c> shows it above the grid, beside it or below it. The page
    ///     renders its widgets again when the search changes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public TBuilder Widgets(Action<WidgetsBuilder<IndexWidgetContext<TResource>>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var widgets = new List<WidgetOptions>();
        configure(new(Services, widgets));
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.Widgets.AddRange(widgets)
        );

        return (TBuilder)this;
    }

    /// <summary>
    ///     Enables searching on the index and creates a builder that configures it.
    /// </summary>
    /// <param name="createSearch">
    ///     A delegate that creates the search options, which the data source reads.
    /// </param>
    /// <returns>A builder that configures searching.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="createSearch" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The delegate runs each time the resource's options are created, so each service
    ///     provider gets its own search options.
    /// </remarks>
    protected ResourceSearchBuilder<TResource> CreateSearchBuilder(
        Func<ResourceSearchOptions> createSearch
    )
    {
        ArgumentNullException.ThrowIfNull(createSearch);
        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.Search = createSearch()
        );

        return new(Services);
    }
}
