using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.Resources.Options;

// An icon field offers registered icons, and its inline layout lists them all, so it needs a list
internal static class IconEditorCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options, IconOptions icons)
    {
        CheckFields(options, "create", options.Create, icons);
        CheckFields(options, "edit", options.Edit, icons);
    }

    private static void CheckFields<TResource>(
        ResourceOptions<TResource> options,
        string form,
        ResourceFormOptions? formOptions,
        IconOptions icons
    )
    {
        foreach (var field in FormItems.Fields(formOptions))
        {
            if (field.Editor is not IconEditor editor)
            {
                continue;
            }

            var unknown = (editor.Icons ?? [])
                .Where(icon => !icons.TryGetIcon(icon, out _))
                .ToArray();
            if (unknown.Length > 0)
            {
                var known = editor.Icons!.Except(unknown).ToArray();
                if (known.Length == 0)
                {
                    FormItems.Remove(formOptions!, field);
                    options.Report(
                        ConfigurationProblemKind.UnknownIcons,
                        $"The {form} form's icon field '{field.FieldName}' lists icons that aren't registered: {string.Join(", ", unknown)}.",
                        "None of its icons are registered, so the field is left out.",
                        field.Source
                    );
                    continue;
                }

                editor.Icons = known;
                options.Report(
                    ConfigurationProblemKind.UnknownIcons,
                    $"The {form} form's icon field '{field.FieldName}' lists icons that aren't registered: {string.Join(", ", unknown)}.",
                    "The field offers only its registered icons.",
                    field.Source
                );
            }

            if (editor.Icons is { Count: 0 })
            {
                editor.Icons = null;
                options.Report(
                    ConfigurationProblemKind.EmptyIconList,
                    $"The {form} form's icon field '{field.FieldName}' has an empty list of icons. Set Icons to null to offer every registered icon.",
                    "The field offers every registered icon.",
                    field.Source
                );
            }

            if (editor is { Layout: IconEditorLayout.Inline, Icons: null })
            {
                editor.Layout = IconEditorLayout.Auto;
                options.Report(
                    ConfigurationProblemKind.InlineIconsWithoutList,
                    $"The {form} form's icon field '{field.FieldName}' uses the inline layout, which requires a list of icons.",
                    "The field uses the automatic layout.",
                    field.Source
                );
            }
        }
    }
}
