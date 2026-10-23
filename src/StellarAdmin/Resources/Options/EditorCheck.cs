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

            var editor = field.Editor!.TypeName;
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
        var editor = field.Editor?.TypeName;
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
            case ILookupSheetEditor { Items: null, Resource: null }:
                return (
                    $"`{editor}` has no items",
                    "Pass the editor's items to `UseItems` or `UseSource`, or take them from a resource with `UseResource`."
                );
            case ILookupSheetEditor { Items: SourceLookupItems items }
                when FindSourceProblem(items) is { } problem:
                return problem;
            default:
                return FindValueProblem(field, propertyType);
        }
    }

    // A source that searches with a value from the form needs one of its type, and a source that doesn't takes none
    private static (string Reason, string Fix)? FindSourceProblem(SourceLookupItems items)
    {
        var source = items.SourceType.Name;
        var shape = items.Shape;
        if (shape.MediaMethods.Count > 1)
        {
            return (
                $"`{source}` overrides more than one media method: {string.Join(", ", shape.MediaMethods.Select(method => $"`{method}`"))}",
                "Override only one of them."
            );
        }

        switch (shape.ArgumentType, items.ArgumentType)
        {
            case (null, { } argument):
                return (
                    $"`{source}` doesn't search with a value from the form, and the field passes it a `{TypeName(argument)}`",
                    $"Call `UseSource<{source}>()` without a value, or derive `{source}` from `LookupSource<TItem, TValue, TArgument>`."
                );
            case ({ } expected, null):
                return (
                    $"`{source}` searches with a `{TypeName(expected)}` from the form, and the field passes it none",
                    $"Pass the value with `UseSource<{source}>(form => ...)`."
                );
            case ({ } expected, { } argument)
                when argument != expected && Nullable.GetUnderlyingType(argument) != expected:
                return (
                    $"`{source}` searches with a `{TypeName(expected)}` from the form, and the field passes it a `{TypeName(argument)}`",
                    $"Pass a `{TypeName(expected)}` to `UseSource<{source}>(form => ...)`."
                );
            default:
                return null;
        }
    }

    // Items whose values aren't the property's type can't select its value. A field a binder binds, such as a
    // many-to-many navigation, holds entities and binds the items' keys itself
    private static (string Reason, string Fix)? FindValueProblem(
        FormFieldOptions field,
        Type propertyType
    )
    {
        var valueType = field.Editor switch
        {
            ChoiceEditor choice => choice.ItemsValueType,
            ILookupSheetEditor lookup => lookup.Items?.ValueType,
            _ => null,
        };
        if (valueType is null || field.Binder is not null)
        {
            return null;
        }

        var fieldValueType = ElementType(propertyType) ?? propertyType;
        if (Unwrap(valueType) == Unwrap(fieldValueType))
        {
            return null;
        }

        return (
            $"its items' values are `{TypeName(valueType)}`, and `{field.FieldName}` holds `{TypeName(fieldValueType)}`",
            $"Give the editor items whose values are `{TypeName(Unwrap(fieldValueType))}`, or bind the field to a property of the items' type."
        );

        static Type Unwrap(Type type) => Nullable.GetUnderlyingType(type) ?? type;
    }

    private static string TypeName(Type type) =>
        Nullable.GetUnderlyingType(type) is { } underlying ? $"{underlying.Name}?" : type.Name;

    // The element type of a collection property, or null for any other property
    private static Type? ElementType(Type propertyType) =>
        propertyType != typeof(string)
        && propertyType
            .GetInterfaces()
            .Append(propertyType)
            .FirstOrDefault(type =>
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            )
            is { } enumerable
            ? enumerable.GetGenericArguments()[0]
            : null;

    // A collection property takes its choices from the element type
    private static bool HasPropertyChoices(Type propertyType)
    {
        var valueType = ElementType(propertyType) ?? propertyType;
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
