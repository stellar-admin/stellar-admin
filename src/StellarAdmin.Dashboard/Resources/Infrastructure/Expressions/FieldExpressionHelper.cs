using System.Linq.Expressions;
using System.Reflection;

namespace StellarAdmin.Dashboard.Resources.Infrastructure.Expressions;

internal static class FieldExpressionHelper
{
    // Builds an x => x.PropertyName expression for a seeded column or field definition,
    // or null when TEntity has no such property.
    internal static LambdaExpression? BuildFieldExpression<TEntity>(string propertyName)
    {
        var property = typeof(TEntity).GetProperty(propertyName);
        if (property is null)
        {
            return null;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        return Expression.Lambda(Expression.Property(parameter, property), parameter);
    }

    // The property the expression selects directly on the lambda parameter, or null for
    // any other shape — nested chains, method calls, indexers, non-property members.
    internal static string? ExtractDirectPropertyName(LambdaExpression field)
    {
        var body = UnwrapConvert(field.Body);

        return
            body is MemberExpression { Member: PropertyInfo property } member
            && member.Expression == field.Parameters[0]
            ? property.Name
            : null;
    }

    // Best-effort name: the final member of the body, or null when the body is not a
    // member access at all.
    internal static string? ExtractFieldName(LambdaExpression field)
    {
        return UnwrapConvert(field.Body) is MemberExpression member ? member.Member.Name : null;
    }

    private static Expression UnwrapConvert(Expression body)
    {
        return
            body
                is UnaryExpression
                {
                    NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
                } convert
            ? convert.Operand
            : body;
    }
}
