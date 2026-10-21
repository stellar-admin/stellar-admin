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
                        $"The `{field.FieldName}` field isn't on the {form} form because none of its icons are registered: {Format(unknown)}.",
                        $"Register {Format(unknown)}, or list registered icons in the field's `Icons`.",
                        field.Source
                    );
                    continue;
                }

                editor.Icons = known;
                options.Report(
                    ConfigurationProblemKind.UnknownIcons,
                    $"The `{field.FieldName}` field on the {form} form doesn't offer {Format(unknown)} because they aren't registered. It offers its other icons.",
                    $"Register {Format(unknown)}, or remove them from the field's `Icons`.",
                    field.Source
                );
            }

            if (editor.Icons is { Count: 0 })
            {
                editor.Icons = null;
                options.Report(
                    ConfigurationProblemKind.EmptyIconList,
                    $"The `{field.FieldName}` field on the {form} form offers every registered icon because its `Icons` list is empty.",
                    "List the icons the field offers, or set `Icons` to `null` to offer every registered icon.",
                    field.Source
                );
            }

            if (editor is { Layout: IconEditorLayout.Inline, Icons: null })
            {
                editor.Layout = IconEditorLayout.Auto;
                options.Report(
                    ConfigurationProblemKind.InlineIconsWithoutList,
                    $"The `{field.FieldName}` field on the {form} form doesn't use the inline layout because it has no list of icons. It uses the automatic layout instead.",
                    "List the field's icons in `Icons`, or use another layout.",
                    field.Source
                );
            }
        }
    }

    private static string Format(IEnumerable<string> icons) =>
        string.Join(", ", icons.Select(icon => $"`{icon}`"));
}
