using System.Linq.Expressions;
using System.Reflection;

namespace StellarAdmin.Dashboard.Resources;

internal static class ResourcePropertyPath
{
    public static PropertyInfo[]? GetProperties(LambdaExpression expression)
    {
        var properties = new List<PropertyInfo>();
        Expression body = expression.Body;
        while (body is MemberExpression { Member: PropertyInfo property, Expression: { } owner })
        {
            properties.Add(property);
            body = owner;
        }

        if (body != expression.Parameters[0] || properties.Count == 0)
        {
            return null;
        }

        properties.Reverse();
        return [.. properties];
    }

    public static string GetName(IEnumerable<PropertyInfo> properties) =>
        string.Join('.', properties.Select(property => property.Name));
}
