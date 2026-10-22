using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Resources.Options;

// The built-in displays show a value, or each item of a collection, as its ToString(), which is the
// type name for a class that doesn't override it, such as a related entity. A record of a registered
// resource shows its title, and a collection of them shows it in badges. A badge display's text, a
// custom display or a [UIHint] template can show other values. Without one, the column or field is
// left out.
internal static class ValueTextCheck
{
    public static void Run<TResource>(
        ResourceOptions<TResource> options,
        IReadOnlySet<Type> resourceTypes
    )
    {
        foreach (var column in options.Index.Columns.ToArray())
        {
            if (
                FindProblem(
                    $"The `{column.FieldName ?? column.FieldExpression.ToString()}` column isn't shown",
                    column.Display,
                    column.FieldExpression,
                    resourceTypes
                )
                is var (message, fix)
            )
            {
                options.Index.MutableColumns.Remove(column);
                options.Report(
                    ConfigurationProblemKind.ValueWithoutText,
                    message,
                    fix,
                    column.Source
                );
            }
        }

        CheckFields(options, "view page", options.View, resourceTypes);
        CheckFields(options, "create form", options.Create, resourceTypes);
        CheckFields(options, "edit form", options.Edit, resourceTypes);
    }

    private static void CheckFields<TResource>(
        ResourceOptions<TResource> options,
        string page,
        IFormScope? scope,
        IReadOnlySet<Type> resourceTypes
    )
    {
        foreach (var field in FormItems.DisplayFields(scope))
        {
            if (
                FindProblem(
                    $"The `{field.FieldName ?? field.Title ?? field.FieldExpression.ToString()}` field isn't on the {page}",
                    field.Display,
                    field.FieldExpression,
                    resourceTypes
                )
                is var (message, fix)
            )
            {
                FormItems.Remove(scope!, field);
                options.Report(
                    ConfigurationProblemKind.ValueWithoutText,
                    message,
                    fix,
                    field.Source
                );
            }
        }
    }

    // The message, which starts with what the developer notices, and the fix
    private static (string Message, string Fix)? FindProblem(
        string notice,
        FieldDisplay? display,
        LambdaExpression expression,
        IReadOnlySet<Type> resourceTypes
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
            || resourceTypes.Contains(type) && (itemType is null || display is BadgeDisplay)
            || display is not null && display.GetType().Assembly != typeof(FieldDisplay).Assembly
            || display is null
                && ResourcePropertyPath.GetProperties(expression) is [.., var property]
                && property.IsDefined(typeof(UIHintAttribute))
        )
        {
            return null;
        }

        var badge = $"`UseDisplay<BadgeDisplay>(badge => badge.UseText<{type.Name}>(item => ...))`";

        return itemType is null
                ? (
                    $"{notice} because its value is a `{type.Name}`, which has no text of its own.",
                    $"Show one of the `{type.Name}`'s properties instead, or use {badge}."
                )
            : resourceTypes.Contains(type)
                ? (
                    $"{notice} because its value is a collection of `{type.Name}`, whose titles show only in badges.",
                    "Call `UseDisplay<BadgeDisplay>()`."
                )
            : (
                $"{notice} because its value is a collection of `{type.Name}`, which has no text of its own.",
                $"Show it with {badge}."
            );
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
