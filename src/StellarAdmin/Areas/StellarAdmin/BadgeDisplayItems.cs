using System.Collections;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StellarAdmin.Resources.Displays;

namespace StellarAdmin.Areas.StellarAdmin;

// The texts of a badge display's badges: one for the value, for each item of a collection, or for each entry of a
// dictionary as "key: value"
internal static class BadgeDisplayItems
{
    public static List<string> GetTexts(FieldDisplayView<BadgeDisplay> view, object? model)
    {
        var metadata = view.ViewData.ModelMetadata;
        if (model is not IEnumerable values || model is string)
        {
            return [Text(view, model, metadata)];
        }

        var itemMetadata = metadata.ElementMetadata ?? metadata;
        var keyMetadata = model is IDictionary ? itemMetadata.Properties["Key"] : null;
        var valueMetadata = model is IDictionary ? itemMetadata.Properties["Value"] : null;

        return values
            .Cast<object?>()
            .Select(item =>
                keyMetadata is not null
                && valueMetadata is not null
                && item is not null
                && view.Display.Text is null
                    ? $"{view.FormatValue(Read(item, "Key"), keyMetadata)}: {view.FormatValue(Read(item, "Value"), valueMetadata)}"
                    : Text(view, item, itemMetadata)
            )
            .ToList();
    }

    private static object? Read(object entry, string property) =>
        entry.GetType().GetProperty(property)?.GetValue(entry);

    private static string Text(
        FieldDisplayView<BadgeDisplay> view,
        object? item,
        ModelMetadata metadata
    ) =>
        view.Display.Text is { } text && item is not null
            ? text(item) ?? ""
            : view.FormatValue(item, metadata);
}
