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
        int? maxItems,
        DictionaryEditorTexts? texts
    ) =>
        new Dictionary<string, string>(
            Shared(minItems, maxItems, texts?.None, texts?.Count, texts?.Full, texts?.TooFew)
        )
        {
            ["key"] = $"{keyLabel} {{n}}",
            ["value"] = $"{valueLabel} {{n}}",
            ["move"] = "Move row {n}",
            ["remove"] = texts?.Remove ?? "Remove row {n}",
            ["duplicate"] = texts?.Duplicate ?? "{value} is already used.",
            ["keyRequired"] = texts?.KeyRequired ?? $"{keyLabel} is required.",
            ["valueRequired"] = texts?.ValueRequired ?? $"{valueLabel} is required.",
        };

    public static IReadOnlyDictionary<string, string> ForList(
        int? minItems,
        int? maxItems,
        ListEditorTexts? texts
    ) =>
        new Dictionary<string, string>(
            Shared(minItems, maxItems, texts?.None, texts?.Count, texts?.Full, texts?.TooFew)
        )
        {
            ["value"] = texts?.Item ?? "Item {n}",
            ["move"] = texts?.Move ?? "Move item {n}",
            ["remove"] = texts?.Remove ?? "Remove item {n}",
            ["duplicate"] = texts?.Duplicate ?? "{value} is already in the list.",
        };

    private static string Number(int number) => number.ToString(CultureInfo.InvariantCulture);

    private static Dictionary<string, string> Shared(
        int? minItems,
        int? maxItems,
        string? none,
        string? count,
        string? full,
        string? tooFew
    )
    {
        var texts = new Dictionary<string, string> { ["none"] = none ?? "None" };
        if (minItems is > 0 and var min)
        {
            texts["minItems"] =
                tooFew
                ?? (min == 1 ? "Add at least one item." : $"Add at least {Number(min)} items.");
        }

        if (maxItems is { } max)
        {
            texts["count"] = count ?? $"{{count}} of {Number(max)}";
            texts["full"] =
                full
                ?? (
                    max == 1
                        ? "You can add only one item."
                        : $"You can add up to {Number(max)} items."
                );
        }

        return texts;
    }
}
