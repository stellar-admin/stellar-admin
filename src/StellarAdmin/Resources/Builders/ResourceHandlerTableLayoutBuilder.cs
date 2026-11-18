using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a table layout of a resource's index page whose rows a handler loads.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
/// <typeparam name="TItem">The type of the rows the handler loads.</typeparam>
/// <remarks>
///     By default the rows don't link anywhere. <see cref="HasRecordKey" /> makes each row a record
///     of the resource, and <see cref="LinkTo{TOther}" /> links each row to another resource.
/// </remarks>
public sealed class ResourceHandlerTableLayoutBuilder<TResource, TItem>
    : ResourceLayoutBuilderBase<TResource, ResourceHandlerTableLayoutBuilder<TResource, TItem>>
{
    internal ResourceHandlerTableLayoutBuilder(IServiceCollection services, string key)
        : base(services, key) { }

    /// <summary>
    ///     Configures the table's columns.
    /// </summary>
    /// <param name="configure">A delegate that adds and configures the columns.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The handler sorts the rows: <see cref="ResourceListRequest.Sort" /> names the column the
    ///     user sorted by. Columns don't edit inline rows.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerTableLayoutBuilder<TResource, TItem> Columns(
        Action<ResourceTableColumnsBuilder<TItem>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(ConfigureTable));

        return this;
    }

    /// <summary>
    ///     Sorts the table by a column in ascending order by default.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property of the column to sort by.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The property must belong to a sortable column. The handler receives this order until the
    ///     user sorts by a column.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerTableLayoutBuilder<TResource, TItem> DefaultSortBy<TProperty>(
        Expression<Func<TItem, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Ascending);

    /// <summary>
    ///     Sorts the table by a column in descending order by default.
    /// </summary>
    /// <typeparam name="TProperty">The type of the selected property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property of the column to sort by.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The property must belong to a sortable column. The handler receives this order until the
    ///     user sorts by a column.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerTableLayoutBuilder<TResource, TItem> DefaultSortByDescending<TProperty>(
        Expression<Func<TItem, TProperty>> field
    ) => ConfigureDefaultSort(field, ResourceSortDirection.Descending);

    /// <summary>
    ///     Enables paging on the table.
    /// </summary>
    /// <returns>A builder that configures paging.</returns>
    /// <remarks>
    ///     The handler pages the rows: <see cref="ResourceListRequest.Paging" /> holds the page,
    ///     and the total it returns counts the pages.
    /// </remarks>
    public ResourceTablePagingBuilder<TResource> EnablePaging()
    {
        ConfigureTable(table => table.Paging = new());

        return new(Services, LayoutKey);
    }

    /// <summary>
    ///     Enables and configures paging on the table.
    /// </summary>
    /// <param name="configure">A delegate that configures paging.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerTableLayoutBuilder<TResource, TItem> EnablePaging(
        Action<ResourceTablePagingBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(EnablePaging());

        return this;
    }

    /// <summary>
    ///     Makes each row a record of the resource, with its view and edit links and the delete
    ///     button.
    /// </summary>
    /// <param name="key">
    ///     A delegate that returns the key of the record a row is, or <see langword="null" /> for a
    ///     row that isn't a record.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The rows get the row actions the resource allows, which <see cref="RowActions" /> can
    ///     turn off.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="LinkTo{TOther}" /> was called on the table.
    /// </exception>
    public ResourceHandlerTableLayoutBuilder<TResource, TItem> HasRecordKey(
        Func<TItem, object?> key
    )
    {
        ConfigureRecordKey(key);

        return this;
    }

    /// <summary>
    ///     Links each row to the view page of another resource's record.
    /// </summary>
    /// <typeparam name="TOther">The type of the other resource.</typeparam>
    /// <param name="key">
    ///     A delegate that returns the key of the record a row links to, or
    ///     <see langword="null" /> for a row without a link.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The rows have no other row actions. They don't link when the other resource has no view
    ///     page or the user isn't allowed to open it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="key" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     <see cref="HasRecordKey" /> was called on the table.
    /// </exception>
    public ResourceHandlerTableLayoutBuilder<TResource, TItem> LinkTo<TOther>(
        Func<TItem, object?> key
    )
    {
        ConfigureLink<TOther, TItem>(key);

        return this;
    }

    /// <summary>
    ///     Configures which of the resource's row actions the table shows.
    /// </summary>
    /// <param name="configure">A delegate that turns row actions off.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Only rows that <see cref="HasRecordKey" /> makes records have row actions, and a table
    ///     shows an action only when the resource also allows it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceHandlerTableLayoutBuilder<TResource, TItem> RowActions(
        Action<ResourceRowActionsBuilder<TResource>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(Services, LayoutKey));

        return this;
    }

    private ResourceHandlerTableLayoutBuilder<TResource, TItem> ConfigureDefaultSort(
        LambdaExpression field,
        ResourceSortDirection direction
    )
    {
        ArgumentNullException.ThrowIfNull(field);
        var source = ConfigurationSource.Capture();
        if (ResourcePropertyPath.GetProperties(field) is not { } properties)
        {
            Services.Configure<ResourceOptions<TResource>>(options =>
                options.Report(
                    ConfigurationProblemKind.InvalidSelector,
                    $"The list isn't sorted by `{field}` because it isn't a property path of the row. The default sort is left unchanged.",
                    "Sort by a property of the row, or a path of properties.",
                    source
                )
            );

            return this;
        }

        ConfigureTable(table =>
        {
            table.DefaultSort = new(ResourcePropertyPath.GetName(properties), direction);
            table.DefaultSortSource = source;
        });

        return this;
    }
}
