using System.Globalization;

namespace StellarAdmin.UI;

// The texts sel-collection-editor shows as rows change: {n} is a row's number, {count} the number
// of items and {value} an item's text
internal static class CollectionEditorTexts
{
    public static IReadOnlyDictionary<string, string> ForDictionary(
        string keyLabel,
        string valueLabel,
        int? minItems,
        int? maxItems
    ) =>
        new Dictionary<string, string>(Shared(minItems, maxItems))
        {
            ["key"] = $"{keyLabel} {{n}}",
            ["value"] = $"{valueLabel} {{n}}",
            ["move"] = "Move row {n}",
            ["remove"] = "Remove row {n}",
            ["duplicate"] = "{value} is already used.",
            ["keyRequired"] = $"{keyLabel} is required.",
            ["valueRequired"] = $"{valueLabel} is required.",
        };

    public static IReadOnlyDictionary<string, string> ForList(int? minItems, int? maxItems) =>
        new Dictionary<string, string>(Shared(minItems, maxItems))
        {
            ["value"] = "Item {n}",
            ["move"] = "Move item {n}",
            ["remove"] = "Remove item {n}",
            ["duplicate"] = "{value} is already in the list.",
        };

    private static string Number(int number) => number.ToString(CultureInfo.InvariantCulture);

    private static Dictionary<string, string> Shared(int? minItems, int? maxItems)
    {
        var texts = new Dictionary<string, string> { ["none"] = "None" };
        if (minItems is > 0 and var min)
        {
            texts["minItems"] =
                min == 1 ? "Add at least one item." : $"Add at least {Number(min)} items.";
        }

        if (maxItems is { } max)
        {
            texts["count"] = $"{{count}} of {Number(max)}";
            texts["full"] =
                max == 1 ? "You can add only one item." : $"You can add up to {Number(max)} items.";
        }

        return texts;
    }
}
