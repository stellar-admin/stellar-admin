using System.Linq.Expressions;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal sealed class ReplaceParameterVisitor(
    ParameterExpression source,
    ParameterExpression target
) : ExpressionVisitor
{
    protected override Expression VisitParameter(ParameterExpression node) =>
        node == source ? target : base.VisitParameter(node);
}
