using System.Linq.Expressions;

namespace StellarAdmin.EntityFrameworkCore;

// Item value expressions return object, so the value's own type is the operand of the boxing conversion
internal static class EfCoreItemValue
{
    public static LambdaExpression Unbox<TEntity>(Expression<Func<TEntity, object?>> value)
    {
        var body =
            value.Body
                is UnaryExpression
                {
                    NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
                } convert
            && value.Body.Type == typeof(object)
                ? convert.Operand
                : value.Body;

        return Expression.Lambda(
            typeof(Func<,>).MakeGenericType(typeof(TEntity), body.Type),
            body,
            value.Parameters
        );
    }
}
