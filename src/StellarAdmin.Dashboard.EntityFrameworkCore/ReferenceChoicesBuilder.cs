using System.Linq.Expressions;
using StellarAdmin.Dashboard.Resources.Infrastructure.Query;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>Configures the selectable records for a reference.</summary>
public sealed class ReferenceChoicesBuilder<TTarget>
    where TTarget : class
{
    internal Func<IQueryable<TTarget>, IQueryable<TTarget>>? QueryTransform { get; private set; }

    internal LambdaExpression? Sort { get; private set; }

    internal ReferenceChoicesBuilder() { }

    /// <summary>Orders choices by the selected property.</summary>
    public ReferenceChoicesBuilder<TTarget> DefaultSortBy<TProperty>(
        Expression<Func<TTarget, TProperty>> field
    )
    {
        ArgumentNullException.ThrowIfNull(field);
        Sort = field;

        return this;
    }

    /// <summary>Transforms the query for selectable records.</summary>
    public ReferenceChoicesBuilder<TTarget> TransformQuery(
        Func<IQueryable<TTarget>, IQueryable<TTarget>> transform
    )
    {
        ArgumentNullException.ThrowIfNull(transform);
        var previous = QueryTransform;
        QueryTransform = previous is null ? transform : query => transform(previous(query));

        return this;
    }

    internal IQueryable<TTarget> Apply(IQueryable<TTarget> query, LambdaExpression display)
    {
        query = QueryTransform?.Invoke(query) ?? query;

        return query.OrderByField(Sort ?? display, DataGridSortDirection.Ascending);
    }
}
