using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Options;

// Editors that can't work with their property would fail when the form renders. A field the panel binds falls back
// to the default editor for its property; a field a binder binds is left out, since its value needs the editor.
internal static class EditorCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        CheckFields(options, "create", options.Create);
        CheckFields(options, "edit", options.Edit);
    }

    private static void CheckFields<TResource>(
        ResourceOptions<TResource> options,
        string form,
        ResourceFormOptions? formOptions
    )
    {
        foreach (var field in FormItems.Fields(formOptions))
        {
            if (field.Editor is RadioGroupEditor radio)
            {
                RepairRadioClassNames(options, form, field, radio);
            }

            if (FindProblem(field) is not var (reason, fix))
            {
                continue;
            }

            if (field.Binder is not null)
            {
                FormItems.Remove(formOptions!, field);
                options.Report(
                    ConfigurationProblemKind.UnsuitableEditor,
                    $"The `{field.FieldName}` field isn't on the {form} form because {reason}.",
                    fix,
                    field.Source
                );
                continue;
            }

            var editor = field.Editor!.GetType().Name;
            field.Editor = null;
            field.HandlerType = null;
            options.Report(
                ConfigurationProblemKind.UnsuitableEditor,
                $"The `{field.FieldName}` field on the {form} form uses its property's default editor instead of `{editor}` because {reason}.",
                fix,
                field.Source
            );
        }
    }

    // Why the editor can't work with the field's property, and how to fix it
    private static (string Reason, string Fix)? FindProblem(FormFieldOptions field)
    {
        var propertyType = field.PropertyPath[^1].PropertyType;
        var editor = field.Editor?.GetType().Name;
        switch (field.Editor)
        {
            case CheckboxEditor or ToggleEditor when propertyType != typeof(bool):
                return (
                    $"`{editor}` needs a non-nullable Boolean property, and `{field.FieldName}` is `{TypeName(propertyType)}`",
                    propertyType == typeof(bool?)
                        ? "Use `SelectEditor`, `RadioGroupEditor` or `SegmentedControlEditor` for a nullable Boolean."
                        : $"Use an editor that suits a `{TypeName(propertyType)}` property."
                );
            case ChoiceEditor { ItemsLoader: null } when !HasPropertyChoices(propertyType):
                return (
                    $"`{editor}` has no items, and only takes them from a non-flags enum or Boolean property",
                    "Pass the editor's items to `UseItems`."
                );
            case ILookupSheetEditor { Items: null }:
                return ($"`{editor}` has no items", "Pass the editor's items to `UseItems`.");
            default:
                return null;
        }
    }

    private static string TypeName(Type type) =>
        Nullable.GetUnderlyingType(type) is { } underlying ? $"{underlying.Name}?" : type.Name;

    // A collection property takes its choices from the element type
    private static bool HasPropertyChoices(Type propertyType)
    {
        var valueType =
            propertyType != typeof(string)
            && propertyType
                .GetInterfaces()
                .Append(propertyType)
                .FirstOrDefault(type =>
                    type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                )
                is { } enumerable
                ? enumerable.GetGenericArguments()[0]
                : propertyType;
        valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

        return valueType == typeof(bool)
            || valueType.IsEnum && !valueType.IsDefined(typeof(FlagsAttribute), inherit: false);
    }

    // A radio group has no outer content element and no error element per option
    private static void RepairRadioClassNames<TResource>(
        ResourceOptions<TResource> options,
        string form,
        FormFieldOptions field,
        RadioGroupEditor radio
    )
    {
        var classNames = radio.ClassNames;
        if (classNames.Content is null && classNames.Option.Error is null)
        {
            return;
        }

        classNames.Content = null;
        classNames.Option.Error = null;
        options.Report(
            ConfigurationProblemKind.UnsuitableEditor,
            $"The `{field.FieldName}` field on the {form} form doesn't use its `ClassNames.Content` or `ClassNames.Option.Error` because a radio group has no outer content element and no error element per option.",
            "Use `ClassNames.Option.Content` or `ClassNames.Error` instead.",
            field.Source
        );
    }
}
