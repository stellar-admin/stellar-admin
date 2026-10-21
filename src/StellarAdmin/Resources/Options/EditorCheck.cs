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
                RepairRadioClassNames(options, form, field.FieldName, radio);
            }

            if (FindProblem(field) is not { } problem)
            {
                continue;
            }

            var message =
                $"The {form} form's field '{field.FieldName}' uses {field.Editor!.GetType().Name}, {problem}";
            if (field.Binder is not null)
            {
                FormItems.Remove(formOptions!, field);
                options.Report(
                    ConfigurationProblemKind.UnsuitableEditor,
                    ConfigurationProblemEffect.TurnedOff,
                    message,
                    "The field is left out."
                );
                continue;
            }

            field.Editor = null;
            field.HandlerType = null;
            options.Report(
                ConfigurationProblemKind.UnsuitableEditor,
                ConfigurationProblemEffect.Ignored,
                message,
                "The field uses the default editor for its property."
            );
        }
    }

    private static string? FindProblem(FormFieldOptions field)
    {
        var propertyType = field.PropertyPath[^1].PropertyType;
        switch (field.Editor)
        {
            case CheckboxEditor or ToggleEditor when propertyType != typeof(bool):
                return $"which requires a non-nullable Boolean property, but the property is {propertyType.Name}. Use SelectEditor, RadioGroupEditor or SegmentedControlEditor for a nullable Boolean.";
            case ChoiceEditor { ItemsLoader: null } when !HasPropertyChoices(propertyType):
                return "which requires UseItems unless the property is a non-flags enum or a Boolean.";
            case ILookupSheetEditor { Items: null }:
                return "which requires UseItems.";
            default:
                return null;
        }
    }

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
        string fieldName,
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
            ConfigurationProblemEffect.Ignored,
            $"The {form} form's radio field '{fieldName}' sets ClassNames.Content or ClassNames.Option.Error, but radio groups have no outer Content or per-option Error element. Use Option.Content or Error instead.",
            "Those class names aren't used."
        );
    }
}
