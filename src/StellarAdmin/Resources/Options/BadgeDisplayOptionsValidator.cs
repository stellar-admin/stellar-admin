using Microsoft.Extensions.Options;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Resources.Options;

// A badge display's text delegate casts each item, so its item type must match the field's
internal sealed class BadgeDisplayOptionsValidator<TResource>
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
                column.FieldExpression.ReturnType
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
        Type valueType
    )
    {
        if (display is not BadgeDisplay { TextItemType: { } textItemType })
        {
            return;
        }

        var itemType = GetItemType(valueType);
        if (!textItemType.IsAssignableFrom(itemType))
        {
            failures.Add(
                $"The {field} shows {itemType.Name} badges, but its UseText reads {textItemType.Name}. Call UseText<{itemType.Name}>."
            );
        }
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
                field.FieldExpression.ReturnType
            );
        }
    }

    // The type of the value, or of each item when the value is a collection, as the badge
    // template shows them
    private static Type GetItemType(Type valueType)
    {
        if (valueType != typeof(string))
        {
            var enumerable =
                valueType.IsGenericType
                && valueType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    ? valueType
                    : valueType
                        .GetInterfaces()
                        .FirstOrDefault(type =>
                            type.IsGenericType
                            && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        );
            if (enumerable is not null)
            {
                valueType = enumerable.GetGenericArguments()[0];
            }
        }

        return Nullable.GetUnderlyingType(valueType) ?? valueType;
    }
}
