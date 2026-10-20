using System.Linq.Expressions;

namespace StellarAdmin;

// Compiles an expression that isn't a property chain, such as
// (Booking b) => b.Customer.Name + " " + b.Reference, into a getter for a grid column or display
// field. Expression trees can't use ?., so reading a property or calling an instance method on a
// null value gives the member type's default instead of throwing, as a property chain reads a null
// along the chain as a null value. Linked into StellarAdmin.UI and StellarAdmin.
internal static class ComputedValue
{
    public static Func<object, object?> Compile(LambdaExpression expression)
    {
        var item = Expression.Parameter(typeof(object), "item");
        var parameter = expression.Parameters[0];
        var body = Expression.Block(
            [parameter],
            Expression.Assign(parameter, Expression.Convert(item, parameter.Type)),
            Expression.Convert(new NullSafeVisitor().Visit(expression.Body), typeof(object))
        );

        return Expression.Lambda<Func<object, object?>>(body, item).Compile();
    }

    private sealed class NullSafeVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node) =>
            node.Expression is null
                ? node
                : GuardNull(Visit(node.Expression), instance => node.Update(instance), node.Type);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var arguments = Visit(node.Arguments);

            return node.Object is null
                ? node.Update(null, arguments)
                : GuardNull(
                    Visit(node.Object),
                    instance => node.Update(instance, arguments),
                    node.Type
                );
        }

        // instance == null ? default : access, evaluating the instance once
        private static Expression GuardNull(
            Expression instance,
            Func<Expression, Expression> access,
            Type type
        )
        {
            if (instance.Type.IsValueType && Nullable.GetUnderlyingType(instance.Type) is null)
            {
                return access(instance);
            }

            var variable = Expression.Variable(instance.Type);
            return Expression.Block(
                type,
                [variable],
                Expression.Assign(variable, instance),
                Expression.Condition(
                    instance.Type.IsValueType
                        ? Expression.Equal(variable, Expression.Constant(null, instance.Type))
                        : Expression.ReferenceEqual(variable, Expression.Constant(null)),
                    Expression.Default(type),
                    access(variable),
                    type
                )
            );
        }
    }
}
