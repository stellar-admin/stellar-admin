using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;
using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin;

// The settings the list and dictionary editors resolve from the field, its property and the label options, shared by
// their templates and the checks that run again on the server
internal static class CollectionEditorSettings
{
    // A dictionary's key column: its label, and its rules for text keys
    public static DictionaryEditorLabelContext DictionaryKeyContext(
        DictionaryEditorLabelContext context,
        DictionaryEditorLabelOptions labels,
        DictionaryEditorKey key
    ) =>
        new(context.FieldLabel)
        {
            ColumnLabel = key.Label ?? labels.KeyLabel(context),
            MaxItems = context.MaxItems,
            MaxLength = key.MaxLength,
            MinItems = context.MinItems,
        };

    public static DictionaryEditorLabelContext DictionaryContext(
        DictionaryEditor editor,
        ModelMetadata metadata,
        string fieldLabel,
        int? keyCount
    )
    {
        var (minItems, maxItems) = ItemCounts(editor.MinItems, editor.MaxItems, metadata);

        // Each key can be used once, so the keys there are to choose from are the most entries there can be
        if (keyCount is { } count && !(maxItems <= count))
        {
            maxItems = count;
        }

        return new(fieldLabel) { MaxItems = maxItems, MinItems = minItems };
    }

    public static DictionaryEditorTexts DictionaryTexts(
        DictionaryEditorLabelOptions labels,
        DictionaryEditorLabelContext context,
        string keyLabel,
        string valueLabel
    ) =>
        new()
        {
            Count = context.MaxItems is null ? null : labels.CountText(context),
            Duplicate = labels.DuplicateText(context),
            Full = context.MaxItems is null ? null : labels.FullText(context),
            KeyRequired = labels.RequiredText(WithColumn(context, keyLabel)),
            None = labels.NoneText(context),
            Remove = labels.RemoveLabel(context),
            TooFew = context.MinItems is > 0 ? labels.TooFewText(context) : null,
            ValueRequired = labels.RequiredText(WithColumn(context, valueLabel)),
        };

    // A dictionary's value column: its label, and its rules for text and number values
    public static DictionaryEditorLabelContext DictionaryValueContext(
        DictionaryEditorLabelContext context,
        DictionaryEditorLabelOptions labels,
        DictionaryEditorValue value
    ) =>
        new(context.FieldLabel)
        {
            ColumnLabel = value.Label ?? labels.ValueLabel(context),
            Max = value.Max,
            MaxItems = context.MaxItems,
            MaxLength = value.MaxLength,
            Min = value.Min,
            MinItems = context.MinItems,
        };

    // The number of keys a dictionary's keys can be: their items, or an enum's members
    public static int? KeyCount(Type keyType, IReadOnlyList<ChoiceItem>? keyItems) =>
        keyItems is not null ? keyItems.Count(item => !item.Disabled)
        : keyType.IsEnum ? Enum.GetNames(keyType).Length
        : null;

    public static ListEditorLabelContext ListContext(
        ListEditor editor,
        ModelMetadata metadata,
        string fieldLabel
    )
    {
        var (minItems, maxItems) = ItemCounts(editor.MinItems, editor.MaxItems, metadata);

        return new(fieldLabel)
        {
            Max = editor.Max,
            MaxItems = maxItems,
            MaxLength = editor.MaxLength,
            Min = editor.Min,
            MinItems = minItems,
        };
    }

    public static ListEditorTexts ListTexts(
        ListEditorLabelOptions labels,
        ListEditorLabelContext context
    ) =>
        new()
        {
            Count = context.MaxItems is null ? null : labels.CountText(context),
            Duplicate = labels.DuplicateText(context),
            Full = context.MaxItems is null ? null : labels.FullText(context),
            Item = labels.ItemLabel(context),
            Move = labels.MoveLabel(context),
            None = labels.NoneText(context),
            Remove = labels.RemoveLabel(context),
            TooFew = context.MinItems is > 0 ? labels.TooFewText(context) : null,
        };

    // The editor's numbers of items, or else the lengths of the property's attributes
    private static (int? Min, int? Max) ItemCounts(int? min, int? max, ModelMetadata metadata)
    {
        var validators = metadata.ValidatorMetadata;
        min ??=
            validators.OfType<MinLengthAttribute>().FirstOrDefault()?.Length
            ?? validators.OfType<LengthAttribute>().FirstOrDefault()?.MinimumLength;
        max ??=
            validators.OfType<MaxLengthAttribute>().FirstOrDefault()?.Length
            ?? validators.OfType<LengthAttribute>().FirstOrDefault()?.MaximumLength;

        // MaxLengthAttribute without a length allows any number
        return (min, max < 0 ? null : max);
    }

    private static DictionaryEditorLabelContext WithColumn(
        DictionaryEditorLabelContext context,
        string columnLabel
    ) =>
        new(context.FieldLabel)
        {
            ColumnLabel = columnLabel,
            MaxItems = context.MaxItems,
            MinItems = context.MinItems,
        };
}
