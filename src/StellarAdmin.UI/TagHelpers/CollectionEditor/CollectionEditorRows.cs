using System.Collections;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StellarAdmin.UI;

internal static class CollectionEditorRows
{
    // The rows a redisplayed form posted, from ModelState, or otherwise the model's items. An empty
    // submission leaves an entry for the name itself, so it shows no rows.
    public static IReadOnlyList<CollectionEditorRow> Read(
        ModelStateDictionary state,
        string name,
        object? model,
        IReadOnlyList<Type> partTypes
    )
    {
        var dictionary = partTypes.Count == 2;

        // An error added for an item has no value, so only binding marks the rows as posted
        var posted = state.Any(entry =>
            entry.Key.StartsWith(name + "[", StringComparison.Ordinal)
            && entry.Value?.RawValue != null
        );
        if (posted)
        {
            var rows = new List<CollectionEditorRow>();
            for (var index = 0; ; index++)
            {
                var keys = dictionary
                    ? new[] { $"{name}[{index}].Key", $"{name}[{index}].Value" }
                    : [$"{name}[{index}]"];
                if (
                    !keys.Any(key =>
                        state.TryGetValue(key, out var entry) && entry.RawValue != null
                    )
                )
                {
                    break;
                }

                rows.Add(
                    new CollectionEditorRow(
                        keys.Select(
                                (key, part) =>
                                    Normalize(
                                        state.TryGetValue(key, out var entry)
                                            ? entry.AttemptedValue
                                            : null,
                                        partTypes[part]
                                    )
                            )
                            .ToList(),
                        Errors(state, name, index, keys)
                    )
                );
            }

            return rows;
        }

        if (state.TryGetValue(name, out var empty) && empty.RawValue != null)
        {
            return [];
        }

        var values = dictionary
            ? CollectionEditorValue
                .Entries(model)
                .Select(entry =>
                    (IReadOnlyList<string>)
                        [
                            CollectionEditorValue.Format(entry.Key),
                            CollectionEditorValue.Format(entry.Value),
                        ]
                )
            : (model as IEnumerable)
                ?.Cast<object?>()
                .Select(item => (IReadOnlyList<string>)[CollectionEditorValue.Format(item)])
                ?? [];
        return values
            .Select(
                (row, index) =>
                    new CollectionEditorRow(
                        row,
                        Errors(
                            state,
                            name,
                            index,
                            dictionary
                                ? [$"{name}[{index}].Key", $"{name}[{index}].Value"]
                                : [$"{name}[{index}]"]
                        )
                    )
            )
            .ToList();
    }

    // Each part's first error. An error for a dictionary's whole entry goes with its key.
    private static IReadOnlyList<string?> Errors(
        ModelStateDictionary state,
        string name,
        int index,
        string[] keys
    )
    {
        var errors = keys.Select(key => Message(state, key)).ToArray();
        if (keys.Length == 2 && errors[0] == null)
        {
            errors[0] = Message(state, $"{name}[{index}]");
        }

        return errors;
    }

    private static string? Message(ModelStateDictionary state, string key) =>
        state.TryGetValue(key, out var entry) && entry.Errors.Count > 0
            ? entry.Errors[0].ErrorMessage is { Length: > 0 } message
                ? message
                : entry.Errors[0].Exception?.Message
            : null;

    private static string Normalize(string? value, Type type) =>
        value != null && CollectionEditorValue.Underlying(type).IsEnum
            ? ChoiceGroupValue.Normalize(value, type)
            : value ?? "";
}
