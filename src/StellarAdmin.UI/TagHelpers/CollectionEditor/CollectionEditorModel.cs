using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.UI;

internal sealed class CollectionEditorModel
{
    public required string AddText { get; init; }
    public required bool Dictionary { get; init; }
    public bool Disabled { get; init; }
    public required string EmptyText { get; init; }
    public required string Id { get; init; }
    public int? MaxItems { get; init; }
    public int? MinItems { get; init; }
    public required string Name { get; init; }
    public bool Numbered { get; init; }
    public required IReadOnlyList<CollectionEditorPart> Parts { get; init; }
    public bool ReadOnly { get; init; }
    public required IReadOnlyList<CollectionEditorRow> Rows { get; init; }
    public bool Sortable { get; init; }
    public required IReadOnlyDictionary<string, string> Texts { get; init; }
    public bool Unique { get; init; }
}

// A list's item, or a dictionary's key or value
internal sealed class CollectionEditorPart
{
    public string? Align { get; init; }
    public string? Casing { get; init; }
    public IReadOnlyList<SelectListItem>? Items { get; init; }
    public required string Label { get; init; }
    public string? Max { get; init; }
    public int? MaxLength { get; init; }
    public string? Min { get; init; }
    public required string Name { get; init; }
    public string? Pattern { get; init; }
    public string? PatternMessage { get; init; }
    public string? Placeholder { get; init; }
    public string? Prefix { get; init; }
    public string? Step { get; init; }
    public string? Suffix { get; init; }
    public string? Type { get; init; }
}

// The values of a row's parts, in order, and each part's error from ModelState
internal sealed record CollectionEditorRow(
    IReadOnlyList<string> Values,
    IReadOnlyList<string?> Errors
);
