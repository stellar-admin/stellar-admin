using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Resources.Options;

// A badge display's text delegate casts each item, so its item type must match the field's. Otherwise the
// column or field is left out.
internal static class BadgeDisplayCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        foreach (var column in options.Index.Columns.ToArray())
        {
            if (
                FindProblem(
                    $"The `{column.FieldName ?? column.FieldExpression.ToString()}` column isn't shown",
                    column.Display,
                    column.FieldExpression.ReturnType
                ) is
                var (message, fix)
            )
            {
                options.Index.MutableColumns.Remove(column);
                options.Report(
                    ConfigurationProblemKind.MismatchedBadgeText,
                    message,
                    fix,
                    column.Source
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
                    $"The `{field.FieldName ?? field.Title ?? field.FieldExpression.ToString()}` field isn't on the {page}",
                    field.Display,
                    field.FieldExpression.ReturnType
                ) is
                var (message, fix)
            )
            {
                FormItems.Remove(scope!, field);
                options.Report(
                    ConfigurationProblemKind.MismatchedBadgeText,
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
        Type valueType
    )
    {
        if (display is not BadgeDisplay { TextItemType: { } textItemType })
        {
            return null;
        }

        var itemType = GetItemType(valueType);

        return textItemType.IsAssignableFrom(itemType)
            ? null
            : (
                $"{notice} because it shows `{itemType.Name}` badges, and its `UseText` reads `{textItemType.Name}`.",
                $"Call `UseText<{itemType.Name}>`."
            );
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
