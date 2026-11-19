using System.Linq.Expressions;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures a group of columns of a table on a resource's index page.
/// </summary>
/// <typeparam name="TItem">
///     The type of the table's rows: the resource, or the items a handler loads.
/// </typeparam>
public sealed class ResourceTableColumnGroupBuilder<TItem>
{
    // Configures the table, once the resource's options are built
    private readonly Action<Action<ResourceTableDataLayoutOptions>> _configureTable;
    private readonly DataGridColumnGroupOptions _group;

    /// <summary>
    ///     Sets how the group's head aligns over its columns.
    /// </summary>
    /// <value>
    ///     The default is <see cref="DataGridColumnGroupAlign.Center" />.
    /// </value>
    public DataGridColumnGroupAlign Align
    {
        set => _group.Align = value;
    }

    internal ResourceTableColumnGroupBuilder(
        Action<Action<ResourceTableDataLayoutOptions>> configureTable,
        DataGridColumnGroupOptions group
    )
    {
        _configureTable = configureTable;
        _group = group;
    }

    /// <summary>
    ///     Adds a column to the group.
    /// </summary>
    /// <typeparam name="TProperty">The type of the column's property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property the column shows, or computes the value it
    ///     shows, such as <c>product =&gt; product.Price * product.Quantity</c>.
    /// </param>
    /// <returns>A builder that configures the column.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     A column that computes its value requires a title, and sorts only by an expression.
    /// </remarks>
    public ResourceTableColumnBuilder<TItem> Add<TProperty>(
        Expression<Func<TItem, TProperty>> field
    )
    {
        ArgumentNullException.ThrowIfNull(field);

        var column = new ResourceTableColumnBuilder<TItem>(field, _group);
        _configureTable(table => table.MutableColumns.Add(column.Build()));

        return column;
    }

    /// <summary>
    ///     Adds a column to the group and configures it.
    /// </summary>
    /// <typeparam name="TProperty">The type of the column's property.</typeparam>
    /// <param name="field">
    ///     An expression that selects the property the column shows, or computes the value it
    ///     shows, such as <c>product =&gt; product.Price * product.Quantity</c>.
    /// </param>
    /// <param name="configure">A delegate that configures the column.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="field" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceTableColumnGroupBuilder<TItem> Add<TProperty>(
        Expression<Func<TItem, TProperty>> field,
        Action<ResourceTableColumnBuilder<TItem>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(Add(field));

        return this;
    }

    /// <summary>
    ///     Adds a group of columns to the group.
    /// </summary>
    /// <param name="title">The nested group's head text.</param>
    /// <param name="configure">
    ///     A delegate that adds the nested group's columns and configures it.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="title" /> or <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public ResourceTableColumnGroupBuilder<TItem> AddGroup(
        string title,
        Action<ResourceTableColumnGroupBuilder<TItem>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_configureTable, new(title, _group)));

        return this;
    }
}
