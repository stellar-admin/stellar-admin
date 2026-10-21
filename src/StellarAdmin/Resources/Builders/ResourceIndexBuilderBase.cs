using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

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
    ///     Configures the index columns.
    /// </summary>
    /// <param name="configure">A delegate that adds and configures the columns.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public TBuilder Columns(Action<ResourceColumnsBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(Services));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Sorts the index by a column in ascending order by default.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property of the column to sort by.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The property must belong to a sortable column. The index uses this order until the user
    ///     sorts by a column.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public TBuilder DefaultSortBy<TProperty>(Expression<Func<TResource, TProperty>> field) =>
        ConfigureDefaultSort(field, ResourceSortDirection.Ascending);

    /// <summary>
    ///     Sorts the index by a column in descending order by default.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property of the column to sort by.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The property must belong to a sortable column. The index uses this order until the user
    ///     sorts by a column.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public TBuilder DefaultSortByDescending<TProperty>(
        Expression<Func<TResource, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Descending);

    /// <summary>
    ///     Enables paging on the index.
    /// </summary>
    /// <returns>A builder that configures paging.</returns>
    public ResourcePagingBuilder<TResource> EnablePaging()
    {
        Services.Configure<ResourceOptions<TResource>>(options => options.Index.Paging = new());

        return new(Services);
    }

    /// <summary>
    ///     Enables and configures paging on the index.
    /// </summary>
    /// <param name="configure">A delegate that configures paging.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public TBuilder EnablePaging(Action<ResourcePagingBuilder<TResource>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnablePaging());

        return (TBuilder)this;
    }

    /// <summary>
    ///     Shows a partial view when a row is expanded.
    /// </summary>
    /// <param name="partialName">The partial view to render.</param>
    /// <returns>A builder that configures the rows.</returns>
    /// <remarks>
    ///     The partial view is rendered with the index page, with the row's resource as its model.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    public ResourceRowDetailsBuilder<TResource> EnableRowDetails(string partialName) =>
        RegisterRowDetails(partialName, null);

    /// <summary>
    ///     Shows a partial view when a row is expanded, and configures the rows.
    /// </summary>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="configure">A delegate that configures the rows.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The partial view is rendered with the index page, with the row's resource as its model.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="partialName" /> or <paramref name="configure" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    public TBuilder EnableRowDetails(
        string partialName,
        Action<ResourceRowDetailsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableRowDetails(partialName));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Shows a partial view with a model that a handler loads when a row is expanded.
    /// </summary>
    /// <typeparam name="TModel">The type of the partial view's model.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the model.</typeparam>
    /// <param name="partialName">The partial view to render.</param>
    /// <returns>A builder that configures the rows.</returns>
    /// <remarks>
    ///     The handler is registered as a scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    public ResourceRowDetailsBuilder<TResource> EnableRowDetails<TModel, THandler>(
        string partialName
    )
        where THandler : class, IResourceRowDetailHandler<TModel>
    {
        Services.TryAddScoped<THandler>();

        return RegisterRowDetails(partialName, new HandlerRowDetailLoader<TModel, THandler>());
    }

    /// <summary>
    ///     Shows a partial view with a model that a handler loads when a row is expanded, and
    ///     configures the rows.
    /// </summary>
    /// <typeparam name="TModel">The type of the partial view's model.</typeparam>
    /// <typeparam name="THandler">The type of the handler that loads the model.</typeparam>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="configure">A delegate that configures the rows.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler is registered as a scoped service if it isn't registered already.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="partialName" /> or <paramref name="configure" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="partialName" /> is empty or whitespace.
    /// </exception>
    public TBuilder EnableRowDetails<TModel, THandler>(
        string partialName,
        Action<ResourceRowDetailsBuilder<TResource>> configure
    )
        where THandler : class, IResourceRowDetailHandler<TModel>
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableRowDetails<TModel, THandler>(partialName));

        return (TBuilder)this;
    }

    /// <summary>
    ///     Shows one or more panels when a row is expanded.
    /// </summary>
    /// <param name="panels">A delegate that adds the panels.</param>
    /// <returns>A builder that configures the rows.</returns>
    /// <remarks>
    ///     Two or more panels are shown as tabs. No panels, a panel without a name, or two panels with the same name are configuration
    ///     problems. A panel without a name, and the later panel with a repeated name, are left
    ///     out, and without panels rows don't expand.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="panels" /> is <see langword="null" />.
    /// </exception>
    public ResourceRowDetailsBuilder<TResource> EnableRowDetails(
        Action<ResourceRowDetailPanelsBuilder<TResource>> panels
    )
    {
        ArgumentNullException.ThrowIfNull(panels);
        var builder = new DefaultRowDetailPanelsBuilder<TResource>(Services);
        panels(builder);

        return RegisterRowDetails(builder);
    }

    /// <summary>
    ///     Shows one or more panels when a row is expanded, and configures the rows.
    /// </summary>
    /// <param name="panels">A delegate that adds the panels.</param>
    /// <param name="configure">A delegate that configures the rows.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Two or more panels are shown as tabs. No panels, a panel without a name, or two panels with the same name are configuration
    ///     problems. A panel without a name, and the later panel with a repeated name, are left
    ///     out, and without panels rows don't expand.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="panels" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public TBuilder EnableRowDetails(
        Action<ResourceRowDetailPanelsBuilder<TResource>> panels,
        Action<ResourceRowDetailsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnableRowDetails(panels));

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

    /// <summary>
    ///     Shows a partial view when a row is expanded.
    /// </summary>
    /// <param name="partialName">The partial view to render.</param>
    /// <param name="loader">
    ///     The object that loads the partial's model from the row's key when the row is expanded,
    ///     or <see langword="null" /> to use the row itself as the model.
    /// </param>
    /// <returns>A builder that configures the rows.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="partialName" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     An empty or white space partial name is a configuration problem, and rows don't
    ///     expand.
    /// </remarks>
    protected ResourceRowDetailsBuilder<TResource> RegisterRowDetails(
        string partialName,
        ResourceRowDetailLoader? loader
    )
    {
        ArgumentNullException.ThrowIfNull(partialName);
        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            if (string.IsNullOrWhiteSpace(partialName))
            {
                options.Index.RowDetails = null;
                options.Report(
                    ConfigurationProblemKind.RowDetailsWithoutPanels,
                    ConfigurationProblemEffect.Ignored,
                    "The row details name a blank partial view.",
                    "Rows don't expand."
                );

                return;
            }

            options.Index.RowDetails = new() { MutablePanels = { new(partialName, loader) } };
        });

        return new(Services);
    }

    /// <summary>
    ///     Shows the panels that a panels builder added when a row is expanded.
    /// </summary>
    /// <param name="panels">The builder that added the panels.</param>
    /// <returns>A builder that configures the rows.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="panels" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     No panels, a panel without a name, or two panels with the same name are configuration
    ///     problems. A panel without a name, and the later panel with a repeated name, are left
    ///     out, and without panels rows don't expand.
    /// </remarks>
    protected ResourceRowDetailsBuilder<TResource> RegisterRowDetails(
        ResourceRowDetailPanelsBuilder<TResource> panels
    )
    {
        ArgumentNullException.ThrowIfNull(panels);

        var registered = new List<ResourceRowDetailPanelOptions>();
        var problems = new List<ConfigurationProblem>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var panel in panels.Panels)
        {
            if (string.IsNullOrWhiteSpace(panel.PartialName))
            {
                problems.Add(
                    new(
                        ConfigurationProblemKind.RowDetailPanelWithoutName,
                        ConfigurationProblemEffect.TurnedOff,
                        $"The row detail panel '{panel.Title}' names a blank partial view.",
                        "The panel is left out."
                    )
                );
            }
            else if (string.IsNullOrWhiteSpace(panel.Name))
            {
                problems.Add(
                    new(
                        ConfigurationProblemKind.RowDetailPanelWithoutName,
                        ConfigurationProblemEffect.TurnedOff,
                        $"The row detail panel '{panel.Title}' needs a name. Set Name on the panel.",
                        "The panel is left out."
                    )
                );
            }
            else if (!names.Add(panel.Name))
            {
                problems.Add(
                    new(
                        ConfigurationProblemKind.RepeatedRowDetailPanel,
                        ConfigurationProblemEffect.TurnedOff,
                        $"Row detail panel names must be unique, and '{panel.Name}' is used more than once.",
                        $"The later panel '{panel.Title}' is left out."
                    )
                );
            }
            else
            {
                registered.Add(panel);
            }
        }

        if (registered.Count == 0)
        {
            problems.Add(
                new(
                    ConfigurationProblemKind.RowDetailsWithoutPanels,
                    ConfigurationProblemEffect.Ignored,
                    "The row details have no panels. Add at least one row detail panel.",
                    "Rows don't expand."
                )
            );
        }

        Services.Configure<ResourceOptions<TResource>>(options =>
        {
            options.Problems.AddRange(problems);
            if (registered.Count == 0)
            {
                options.Index.RowDetails = null;
                return;
            }

            options.Index.RowDetails = new();
            foreach (var panel in registered)
            {
                options.Index.RowDetails.MutablePanels.Add(panel);
            }
        });

        return new(Services);
    }

    private TBuilder ConfigureDefaultSort(LambdaExpression field, ResourceSortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (ResourcePropertyPath.GetProperties(field) is not { } properties)
        {
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Report(
                    ConfigurationProblemKind.InvalidSelector,
                    ConfigurationProblemEffect.Ignored,
                    $"The default sort selects '{field}', which isn't a resource property path.",
                    "The default sort is left unchanged."
                )
            );

            return (TBuilder)this;
        }

        Services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.DefaultSort = new(ResourcePropertyPath.GetName(properties), direction)
        );

        return (TBuilder)this;
    }
}
