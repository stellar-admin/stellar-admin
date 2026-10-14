using System.Linq.Expressions;

namespace StellarAdmin.EntityFrameworkCore;

internal sealed class ReplaceParameterVisitor(
    ParameterExpression source,
    ParameterExpression target
) : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node) =>
        node == source ? target : base.VisitParameter(node);
}
