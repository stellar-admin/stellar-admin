namespace StellarAdmin.TagHelpers;

/// <summary>
///     The resolved configuration a <c>sa-toggle-group</c> publishes for its
///     <c>sa-toggle-group-item</c> children to consume. The group determines every value here;
///     an item only contributes its own <c>value</c> (and any per-item variant/size override).
/// </summary>
internal sealed class ToggleGroupContext
{
    public ToggleGroupClassNames? ClassNames { get; init; }

    public required ToggleGroupType Type { get; init; }

    public required ToggleVariant Variant { get; init; }

    public required ToggleSize Size { get; init; }

    public required int Spacing { get; init; }

    /// <summary>
    ///     The shared form-field name applied to every item's input, or <c>null</c> when the group
    ///     is not model-bound.
    /// </summary>
    public required string? FieldName { get; init; }

    /// <summary>
    ///     The selected values, normalized to the bound value type: the posted values when the
    ///     field has model state, otherwise the bound model.
    /// </summary>
    public required IReadOnlySet<string> SelectedValues { get; init; }

    /// <summary>
    ///     The scalar type the values are normalized to, or <c>null</c> when not model-bound.
    /// </summary>
    public required Type? ValueType { get; init; }

    /// <summary>
    ///     Whether an item that can be selected has rendered. Multiple-select groups render their
    ///     empty-selection marker only then, so a fully disabled group never clears its value.
    /// </summary>
    public bool HasEnabledItem { get; set; }

    /// <summary>
    ///     Determines whether an item with the given value is selected.
    /// </summary>
    public bool IsItemSelected(string itemValue) =>
        SelectedValues.Contains(ChoiceGroupValue.Normalize(itemValue, ValueType));
}
