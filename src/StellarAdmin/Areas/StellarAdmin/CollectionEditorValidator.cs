using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Editors;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// Runs the list and dictionary editors' checks again on the posted rows, since the browser's can be bypassed: the
// number of items, duplicates, a dictionary row's missing key or value, and each text's pattern and length and each
// number's range. A row's error goes under the row, such as Entity.Highlights[2] or Entity.LocalNames[1].Key, and the
// number of items under the field. A field that posted nothing, such as one an editing row only shows, isn't checked,
// and a value that didn't bind keeps its binding error.
internal static class CollectionEditorValidator
{
    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(1);

    public static void Validate(
        ModelStateDictionary modelState,
        IValueProvider source,
        IEnumerable<FormFieldOptions> fields,
        string prefix,
        IServiceProvider services,
        IModelMetadataProvider metadataProvider
    )
    {
        var labels = services.GetRequiredService<IOptions<ResourceLabelOptions>>().Value;
        foreach (var field in fields)
        {
            var key = ModelNames.CreatePropertyModelName(prefix, field.FieldName);
            if (!source.ContainsPrefix(key))
            {
                continue;
            }

            var property = field.PropertyPath[^1];
            var metadata = metadataProvider.GetMetadataForProperty(
                property.DeclaringType!,
                property.Name
            );
            var fieldLabel = FieldLabels.Get(field, services, metadataProvider);
            switch (field.Editor)
            {
                case ListEditor list:
                    ValidateList(modelState, source, key, list, metadata, fieldLabel, labels);
                    break;
                case DictionaryEditor dictionary:
                    ValidateDictionary(
                        modelState,
                        source,
                        key,
                        dictionary,
                        metadata,
                        fieldLabel,
                        labels
                    );
                    break;
            }
        }
    }

    // The fields whose editors show each row's error under the row
    public static bool ShowsRowErrors(FormFieldOptions field) =>
        field.Editor is ListEditor or DictionaryEditor;

    private static void AddError(ModelStateDictionary modelState, string key, string message)
    {
        if (modelState.GetFieldValidationState(key) != ModelValidationState.Invalid)
        {
            modelState.AddModelError(key, message);
        }
    }

    private static void CheckCount(
        ModelStateDictionary modelState,
        string key,
        int count,
        int? minItems,
        int? maxItems,
        Func<string> tooFew,
        Func<string> full
    )
    {
        if (count < minItems)
        {
            AddError(modelState, key, tooFew());
        }
        else if (count > maxItems)
        {
            AddError(modelState, key, full());
        }
    }

    // A text's pattern and length, and a number's range. An HTML pattern matches the whole value.
    private static string? CheckValue(
        string value,
        bool number,
        string? pattern,
        string? patternMessage,
        int? maxLength,
        double? min,
        double? max,
        Func<string> patternText,
        Func<string> maxLengthText,
        Func<string> rangeText
    )
    {
        if (number)
        {
            return
                double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var parsed
                ) && (parsed < min || parsed > max)
                ? rangeText()
                : null;
        }

        if (maxLength is { } length && value.Length > length)
        {
            return maxLengthText();
        }

        return
            pattern is not null
            && !Regex.IsMatch(value, $"^(?:{pattern})$", RegexOptions.None, PatternTimeout)
            ? patternMessage ?? patternText()
            : null;
    }

    // How the browser compares values for duplicates: numbers by value, and text ignoring case
    private static string Comparable(string value, bool number) =>
        number
        && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed.ToString(CultureInfo.InvariantCulture)
            : value.ToLower(CultureInfo.CurrentCulture);

    // The posted rows, in order, until an index has no value: each row's values, trimmed
    private static List<string[]> ReadRows(IValueProvider source, string key, string[] parts)
    {
        var rows = new List<string[]>();
        for (var index = 0; ; index++)
        {
            var rowKey = ModelNames.CreateIndexModelName(key, index);
            if (!source.ContainsPrefix(rowKey))
            {
                return rows;
            }

            rows.Add(
                parts
                    .Select(part =>
                        source
                            .GetValue(
                                part.Length == 0
                                    ? rowKey
                                    : ModelNames.CreatePropertyModelName(rowKey, part)
                            )
                            .FirstValue?.Trim()
                        ?? ""
                    )
                    .ToArray()
            );
        }
    }

    private static void ValidateDictionary(
        ModelStateDictionary modelState,
        IValueProvider source,
        string key,
        DictionaryEditor editor,
        ModelMetadata metadata,
        string fieldLabel,
        ResourceLabelOptions labels
    )
    {
        var texts = labels.DictionaryEditor;
        var (keyType, valueType) = CollectionEditorTypes.DictionaryTypes(metadata.ModelType)!.Value;
        var context = CollectionEditorSettings.DictionaryContext(
            editor,
            metadata,
            fieldLabel,
            CollectionEditorSettings.KeyCount(keyType, null)
        );
        var keyPart = editor.KeyPart;
        var valuePart = editor.ValuePart;
        var keyContext = CollectionEditorSettings.DictionaryKeyContext(context, texts, keyPart);
        var valueContext = CollectionEditorSettings.DictionaryValueContext(
            context,
            texts,
            valuePart
        );

        // A select's value is one of its items, so only text and numbers have rules of their own
        var textKey = keyType == typeof(string) && keyPart.Source.ItemsLoader is null;
        var plainValue =
            !CollectionEditorTypes.Underlying(valueType).IsEnum
            && valuePart.Source.ItemsLoader is null;
        var numberValue = plainValue && CollectionEditorTypes.IsNumber(valueType);

        var rows = ReadRows(source, key, ["Key", "Value"]);
        var used = new HashSet<string>(StringComparer.Ordinal);
        var count = 0;
        for (var index = 0; index < rows.Count; index++)
        {
            var (rowKey, rowValue) = (rows[index][0], rows[index][1]);
            if (rowKey.Length == 0 && rowValue.Length == 0)
            {
                continue;
            }

            count++;
            var rowName = ModelNames.CreateIndexModelName(key, index);
            var keyName = ModelNames.CreatePropertyModelName(rowName, "Key");
            var valueName = ModelNames.CreatePropertyModelName(rowName, "Value");
            if (rowKey.Length == 0)
            {
                AddError(modelState, keyName, texts.RequiredText(keyContext));
            }
            else if (!used.Add(Comparable(rowKey, number: false)))
            {
                AddError(
                    modelState,
                    keyName,
                    texts.DuplicateText(context).Replace("{value}", rowKey)
                );
            }
            else if (
                textKey
                && CheckValue(
                    rowKey,
                    number: false,
                    keyPart.Pattern,
                    keyPart.PatternMessage,
                    keyPart.MaxLength,
                    null,
                    null,
                    () => texts.PatternText(keyContext),
                    () => texts.MaxLengthText(keyContext),
                    () => texts.RangeText(keyContext)
                )
                    is { } keyError
            )
            {
                AddError(modelState, keyName, keyError);
            }

            if (rowValue.Length == 0)
            {
                AddError(modelState, valueName, texts.RequiredText(valueContext));
            }
            else if (
                plainValue
                && CheckValue(
                    rowValue,
                    numberValue,
                    valuePart.Pattern,
                    valuePart.PatternMessage,
                    valuePart.MaxLength,
                    valuePart.Min,
                    valuePart.Max,
                    () => texts.PatternText(valueContext),
                    () => texts.MaxLengthText(valueContext),
                    () => texts.RangeText(valueContext)
                )
                    is { } valueError
            )
            {
                AddError(modelState, valueName, valueError);
            }
        }

        CheckCount(
            modelState,
            key,
            count,
            context.MinItems,
            context.MaxItems,
            () => texts.TooFewText(context),
            () => texts.FullText(context)
        );
    }

    private static void ValidateList(
        ModelStateDictionary modelState,
        IValueProvider source,
        string key,
        ListEditor editor,
        ModelMetadata metadata,
        string fieldLabel,
        ResourceLabelOptions labels
    )
    {
        var texts = labels.ListEditor;
        var context = CollectionEditorSettings.ListContext(editor, metadata, fieldLabel);
        var number = CollectionEditorTypes.IsNumber(
            CollectionEditorTypes.ListElementType(metadata.ModelType)!
        );

        var rows = ReadRows(source, key, [""]);
        var used = new HashSet<string>(StringComparer.Ordinal);
        var count = 0;
        for (var index = 0; index < rows.Count; index++)
        {
            var value = rows[index][0];
            if (value.Length == 0)
            {
                continue;
            }

            count++;
            var name = ModelNames.CreateIndexModelName(key, index);
            if (editor.Unique && !used.Add(Comparable(value, number)))
            {
                AddError(modelState, name, texts.DuplicateText(context).Replace("{value}", value));
            }
            else if (
                CheckValue(
                    value,
                    number,
                    editor.Pattern,
                    editor.PatternMessage,
                    editor.MaxLength,
                    editor.Min,
                    editor.Max,
                    () => texts.PatternText(context),
                    () => texts.MaxLengthText(context),
                    () => texts.RangeText(context)
                ) is
                { } error
            )
            {
                AddError(modelState, name, error);
            }
        }

        CheckCount(
            modelState,
            key,
            count,
            context.MinItems,
            context.MaxItems,
            () => texts.TooFewText(context),
            () => texts.FullText(context)
        );
    }
}
