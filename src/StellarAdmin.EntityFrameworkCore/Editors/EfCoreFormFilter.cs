using System.Linq.Expressions;

namespace StellarAdmin.EntityFrameworkCore;

// A filter on the items that reads the form's values. The form is captured as a closure, so EF Core sends the values
// the filter reads as query parameters rather than inlining them.
internal static class EfCoreFormFilter
{
    public static Func<object?, Expression<Func<TEntity, bool>>> Create<TEntity, TModel>(
        Expression<Func<TEntity, TModel, bool>> filter
    ) =>
        model =>
        {
            var form = model switch
            {
                TModel typed => typed,
                null => throw new InvalidOperationException(
                    "The items' filter reads the form, but the items were loaded without it."
                ),
                _ => throw new InvalidOperationException(
                    $"The items' filter reads a {typeof(TModel).Name}, but the form edits a {model.GetType().Name}."
                ),
            };
            Expression<Func<TModel>> captured = () => form;

            return Expression.Lambda<Func<TEntity, bool>>(
                new ReplaceParameterVisitor(filter.Parameters[1], captured.Body).Visit(filter.Body),
                filter.Parameters[0]
            );
        };
}
