using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Resources.Options;

// The built-in displays show each item of a collection as its ToString(), which is the type name for
// an item type that doesn't override it, such as a related entity. A badge display's text, a custom
// display or a [UIHint] template can show such items.
internal sealed class CollectionDisplayOptionsValidator<TResource>
    : IValidateOptions<ResourceOptions<TResource>>
{
    public ValidateOptionsResult Validate(string? name, ResourceOptions<TResource> options)
    {
        var failures = new List<string>();
        foreach (var column in options.Index.Columns)
        {
            AddFailure(
                failures,
                $"index column '{column.FieldName ?? column.FieldExpression.ToString()}'",
                column.Display,
                column.FieldExpression
            );
        }

        AddFailures(failures, "view page", options.View?.Fields);
        AddFailures(failures, "create form", options.Create?.DisplayFields);
        AddFailures(failures, "edit form", options.Edit?.DisplayFields);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void AddFailure(
        List<string> failures,
        string field,
        FieldDisplay? display,
        LambdaExpression expression
    )
    {
        if (
            GetItemType(expression.ReturnType) is not { } itemType
            || HasText(itemType)
            || display is BadgeDisplay { Text: not null }
            || display is not null && display.GetType().Assembly != typeof(FieldDisplay).Assembly
            || display is null
                && ResourcePropertyPath.GetProperties(expression) is [.., var property]
                && property.IsDefined(typeof(UIHintAttribute))
        )
        {
            return;
        }

        failures.Add(
            $"The {field} shows a collection of {itemType.Name}, which has no text of its own. Show it with UseDisplay<BadgeDisplay>(badge => badge.UseText<{itemType.Name}>(item => ...))."
        );
    }

    private static void AddFailures(
        List<string> failures,
        string page,
        IEnumerable<FormDisplayFieldOptions>? fields
    )
    {
        foreach (var field in fields ?? [])
        {
            AddFailure(
                failures,
                $"{page}'s field '{field.FieldName ?? field.Title ?? field.FieldExpression.ToString()}'",
                field.Display,
                field.FieldExpression
            );
        }
    }

    // The item type of a collection of reference types other than strings, or null for any other value
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
        var itemType = enumerable?.GetGenericArguments()[0];

        return itemType is { IsClass: true } && itemType != typeof(string) ? itemType : null;
    }

    private static bool HasText(Type itemType) =>
        itemType
            .GetMethod(nameof(ToString), BindingFlags.Public | BindingFlags.Instance, [])
            ?.DeclaringType != typeof(object);
}
