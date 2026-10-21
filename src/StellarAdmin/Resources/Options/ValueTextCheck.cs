using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Resources.Options;

// The built-in displays show a value, or each item of a collection, as its ToString(), which is the
// type name for a class that doesn't override it, such as a related entity. A badge display's text,
// a custom display or a [UIHint] template can show such values. Without one, the column or field is
// left out.
internal static class ValueTextCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        foreach (var column in options.Index.Columns.ToArray())
        {
            if (
                FindProblem(
                    $"index column '{column.FieldName ?? column.FieldExpression.ToString()}'",
                    column.Display,
                    column.FieldExpression
                ) is
                { } message
            )
            {
                options.Index.MutableColumns.Remove(column);
                options.Report(
                    ConfigurationProblemKind.ValueWithoutText,
                    message,
                    "The column is left out."
                );
            }
        }

        CheckFields(options, "view page", options.View);
        CheckFields(options, "create form", options.Create);
        CheckFields(options, "edit form", options.Edit);
    }

    private static void CheckFields<TResource>(
        ResourceOptions<TResource> options,
        string page,
        IFormScope? scope
    )
    {
        foreach (var field in FormItems.DisplayFields(scope))
        {
            if (
                FindProblem(
                    $"{page}'s field '{field.FieldName ?? field.Title ?? field.FieldExpression.ToString()}'",
                    field.Display,
                    field.FieldExpression
                ) is
                { } message
            )
            {
                FormItems.Remove(scope!, field);
                options.Report(
                    ConfigurationProblemKind.ValueWithoutText,
                    message,
                    "The field is left out."
                );
            }
        }
    }

    private static string? FindProblem(
        string field,
        FieldDisplay? display,
        LambdaExpression expression
    )
    {
        var valueType = expression.ReturnType;
        var itemType = GetItemType(valueType);
        var type = itemType ?? valueType;
        if (
            !type.IsClass
            || itemType is null && typeof(IEnumerable).IsAssignableFrom(valueType)
            || type == typeof(string)
            || type == typeof(object)
            || HasText(type)
            || display is BadgeDisplay { Text: not null }
            || display is not null && display.GetType().Assembly != typeof(FieldDisplay).Assembly
            || display is null
                && ResourcePropertyPath.GetProperties(expression) is [.., var property]
                && property.IsDefined(typeof(UIHintAttribute))
        )
        {
            return null;
        }

        return itemType is null
            ? $"The {field} shows a {type.Name}, which has no text of its own. Show one of its properties instead, or use UseDisplay<BadgeDisplay>(badge => badge.UseText<{type.Name}>(item => ...))."
            : $"The {field} shows a collection of {type.Name}, which has no text of its own. Show it with UseDisplay<BadgeDisplay>(badge => badge.UseText<{type.Name}>(item => ...)).";
    }

    // The item type of a collection other than a string, or null for any other value
    private static Type? GetItemType(Type valueType)
    {
        if (valueType == typeof(string))
        {
            return null;
        }

        var enumerable =
            valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? valueType
                : valueType
                    .GetInterfaces()
                    .FirstOrDefault(type =>
                        type.IsGenericType
                        && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    );

        return enumerable?.GetGenericArguments()[0];
    }

    private static bool HasText(Type type) =>
        type.GetMethod(
            nameof(ToString),
            BindingFlags.Public | BindingFlags.Instance,
            []
        )?.DeclaringType != typeof(object);
}
