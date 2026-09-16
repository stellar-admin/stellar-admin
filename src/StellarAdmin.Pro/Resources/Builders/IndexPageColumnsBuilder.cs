using System.Linq.Expressions;
using StellarAdmin.Pro.Resources.Infrastructure.Expressions;
using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures the columns of the data grid.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class IndexPageColumnsBuilder<TEntity>
    where TEntity : class
{
    private readonly IndexPageOptions<TEntity> _options;

    internal IndexPageColumnsBuilder(IndexPageOptions<TEntity> options)
    {
        _options = options;
    }

    /// <summary>
    ///     Adds a column bound to the property selected by <paramref name="field" />, after
    ///     the columns already configured.
    /// </summary>
    /// <param name="field">
    ///     An expression selecting a property on <typeparamref name="TEntity" />, either
    ///     direct (<c>x => x.Email</c>) or a nested chain (<c>x => x.Department!.Name</c>).
    /// </param>
    public IndexPageColumnBuilder Add<TProp>(Expression<Func<TEntity, TProp>> field)
    {
        ArgumentNullException.ThrowIfNull(field);

        var column = new DataGridColumnOptions(
            field,
            FieldExpressionHelper.ExtractFieldName(field)
        );
        _options.AddColumn(column);

        return new IndexPageColumnBuilder(column);
    }

    /// <summary>Removes all columns, including the default ones.</summary>
    /// <remarks>The default sort is not affected.</remarks>
    public IndexPageColumnsBuilder<TEntity> Clear()
    {
        _options.ClearColumns();

        return this;
    }
}
