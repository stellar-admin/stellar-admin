using System.Collections;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Options;

// A field an integration inferred a resource for, such as a foreign key, gets a lookup sheet for it, or a multi-lookup
// sheet for a collection, when no editor was configured. Integrations run before the registered resources are known,
// so the editor is chosen here. A field without a registered resource keeps its property's default editor, or is left
// out when a binder binds it, since its value needs an editor.
internal static class InferredEditorCheck
{
    public static void Run<TResource>(
        ResourceOptions<TResource> options,
        IReadOnlySet<Type> resourceTypes
    )
    {
        CheckFields(options, "create", options.Create, resourceTypes);
        CheckFields(options, "edit", options.Edit, resourceTypes);
    }

    private static void CheckFields<TResource>(
        ResourceOptions<TResource> options,
        string form,
        ResourceFormOptions? formOptions,
        IReadOnlySet<Type> resourceTypes
    )
    {
        foreach (var field in FormItems.Fields(formOptions))
        {
            if (field is not { Editor: null, InferredResource: { } resource })
            {
                continue;
            }

            var propertyType = field.PropertyPath[^1].PropertyType;
            var isCollection =
                propertyType != typeof(string)
                && typeof(IEnumerable).IsAssignableFrom(propertyType);
            var isRegistered = resourceTypes.Contains(resource.ResourceType);
            if (isRegistered && !isCollection)
            {
                var lookup = new LookupSheetEditor<TResource>();
                lookup.UseResource(resource);
                Use(field, lookup);
                continue;
            }

            // The grid has no editor that selects several items
            if (
                isRegistered
                && formOptions
                    is not (
                        ResourceCreateOptions { InlineRow: not null }
                        or ResourceEditOptions { InlineRow: not null }
                    )
            )
            {
                var lookup = new MultiLookupSheetEditor<TResource>();
                lookup.UseResource(resource);
                Use(field, lookup);
                continue;
            }

            var name = resource.ResourceType.Name;
            if (field.Binder is not null)
            {
                FormItems.Remove(formOptions!, field);
                options.Report(
                    ConfigurationProblemKind.InferredEditorUnavailable,
                    isRegistered
                        ? $"The `{field.FieldName}` field isn't on the {form} form because it has no editor, and the inline row has no editor that selects several `{name}` records."
                        : $"The `{field.FieldName}` field isn't on the {form} form because it has no editor, and no resource is registered for `{name}`.",
                    isRegistered
                        ? "Remove the field from the inline row."
                        : $"Add a resource for `{name}`, or configure the field's editor with its items.",
                    field.Source
                );
                continue;
            }

            if (isRegistered)
            {
                continue;
            }

            options.Problems.Add(
                new ConfigurationProblem(
                    ConfigurationProblemKind.InferredEditorUnavailable,
                    $"The `{field.FieldName}` field on the {form} form uses its property's default editor because no resource is registered for `{name}`.",
                    $"Add a resource for `{name}` to pick it with a lookup, or configure the field's editor with its items."
                )
                {
                    Level = ConfigurationProblemLevel.Info,
                    Source = field.Source,
                }
            );
        }
    }

    private static void Use(FormFieldOptions field, FieldEditor editor)
    {
        field.Editor = editor;
        field.HandlerType = (editor as IFieldEditor)?.HandlerType;
    }
}
