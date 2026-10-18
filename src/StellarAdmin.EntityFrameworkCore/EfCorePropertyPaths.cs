using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace StellarAdmin.EntityFrameworkCore;

internal static class EfCorePropertyPaths
{
    // Whether reading a property path in memory sees what the database has, once the include path of
    // its references is loaded. A collection or skip navigation is never included with it.
    public static bool CanInclude(IEntityType entityType, IReadOnlyList<PropertyInfo> propertyPath)
    {
        IEntityType current = entityType;
        foreach (var property in propertyPath)
        {
            if (current.FindNavigation(property.Name) is not { } navigation)
            {
                return current.FindSkipNavigation(property.Name) is null;
            }

            if (navigation.IsCollection)
            {
                return false;
            }

            current = navigation.TargetEntityType;
        }

        return true;
    }

    // The properties a lambda such as p => p.Category.Name reads, or null when it is not a property path
    public static PropertyInfo[]? GetPropertyPath(LambdaExpression expression) =>
        ReadPropertyPath(expression.Body, expression.Parameters[0]);

    // Every property path a lambda such as p => p.Category.Name + " " + p.Name reads, or null when it
    // uses its parameter other than through properties, such as passing it to a method
    public static IReadOnlyList<PropertyInfo[]>? GetPropertyPaths(LambdaExpression expression)
    {
        var visitor = new PropertyPathVisitor(expression.Parameters[0]);
        visitor.Visit(expression.Body);

        return visitor.ReadsParameterOtherwise ? null : visitor.Paths;
    }

    // The include path of the reference navigations a property path reads through, such as
    // Supplier.Country for Supplier.Country.Name. Owned types load with their owner, so they extend
    // the path without being included themselves; a collection or plain property ends it.
    public static string? FindReferencePath(
        IEntityType entityType,
        IReadOnlyList<PropertyInfo> propertyPath
    )
    {
        var segments = new List<string>();
        string? path = null;
        IEntityType current = entityType;
        foreach (var property in propertyPath)
        {
            if (current.FindNavigation(property.Name) is not { IsCollection: false } navigation)
            {
                break;
            }

            segments.Add(navigation.Name);
            current = navigation.TargetEntityType;
            if (!current.IsOwned())
            {
                path = string.Join('.', segments);
            }
        }

        return path;
    }

    private static PropertyInfo[]? ReadPropertyPath(
        Expression expression,
        ParameterExpression parameter
    )
    {
        var properties = new List<PropertyInfo>();
        var body = expression;
        while (body is MemberExpression { Member: PropertyInfo property, Expression: { } owner })
        {
            properties.Add(property);
            body = owner;
        }

        if (body != parameter)
        {
            return null;
        }

        properties.Reverse();
        return [.. properties];
    }

    private sealed class PropertyPathVisitor(ParameterExpression parameter) : ExpressionVisitor
    {
        public List<PropertyInfo[]> Paths { get; } = [];

        public bool ReadsParameterOtherwise { get; private set; }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (ReadPropertyPath(node, parameter) is { } path)
            {
                Paths.Add(path);
                return node;
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == parameter)
            {
                ReadsParameterOtherwise = true;
            }

            return node;
        }
    }
}
