using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an editor for a dictionary of keys and values, with a row per entry.
/// </summary>
/// <remarks>
///     A key or value is a select when it is an enum or has items, and otherwise a text or number
///     input. Each key can be used once. Adding and removing entries requires
///     <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-dictionary-editor", TagStructure = TagStructure.WithoutEndTag)]
public sealed class DictionaryEditorTagHelper : FieldInputBaseTagHelper<DictionaryEditorClassNames>
{
    private readonly IHtmlGenerator _generator;
    private readonly IOptions<IconOptions> _icons;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryEditorTagHelper" /> class.
    /// </summary>
    /// <param name="generator">The generator that renders the field and the inputs.</param>
    /// <param name="icons">The icon options that supply the add and remove icons.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="generator" /> is <see langword="null" />.
    /// </exception>
    public DictionaryEditorTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _icons = icons;
    }

    /// <summary>
    ///     Gets or sets the text of the button that adds an entry.
    /// </summary>
    /// <value>
    ///     The default is "Add item".
    /// </value>
    [HtmlAttributeName("add-text")]
    public string? AddText { get; set; }

    /// <summary>
    ///     Gets or sets the text shown when the dictionary has no entries.
    /// </summary>
    /// <value>
    ///     The default is "No items yet".
    /// </value>
    [HtmlAttributeName("empty-text")]
    public string? EmptyText { get; set; }

    /// <summary>
    ///     Gets or sets how a text key's case changes as it is entered.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the case. The default is
    ///     <see cref="DictionaryEditorCasing.None" />.
    /// </value>
    [HtmlAttributeName("key-casing")]
    public DictionaryEditorCasing? KeyCasing { get; set; }

    /// <summary>
    ///     Gets or sets the keys to choose from.
    /// </summary>
    /// <value>
    ///     The default is the members of an enum key, or none for a text key.
    /// </value>
    /// <remarks>
    ///     With items, the key is a select and the dictionary can have one entry for each item.
    /// </remarks>
    [HtmlAttributeName("asp-key-items")]
    public IEnumerable<SelectListItem>? KeyItems { get; set; }

    /// <summary>
    ///     Gets or sets the label of the key column.
    /// </summary>
    /// <value>
    ///     The default is "Key".
    /// </value>
    [HtmlAttributeName("key-label")]
    public string? KeyLabel { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of characters a text key can have.
    /// </summary>
    [HtmlAttributeName("key-max-length")]
    public int? KeyMaxLength { get; set; }

    /// <summary>
    ///     Gets or sets a regular expression that each text key must match.
    /// </summary>
    [HtmlAttributeName("key-pattern")]
    public string? KeyPattern { get; set; }

    /// <summary>
    ///     Gets or sets the message shown when a key doesn't match <see cref="KeyPattern" />.
    /// </summary>
    /// <value>
    ///     The default is the browser's message.
    /// </value>
    [HtmlAttributeName("key-pattern-message")]
    public string? KeyPatternMessage { get; set; }

    /// <summary>
    ///     Gets or sets the placeholder of an empty key.
    /// </summary>
    [HtmlAttributeName("key-placeholder")]
    public string? KeyPlaceholder { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of entries.
    /// </summary>
    /// <value>
    ///     The default is the length from a <see cref="MaxLengthAttribute" /> or
    ///     <see cref="LengthAttribute" /> on the bound property, or no limit. Keys from items
    ///     limit it to the number of items.
    /// </value>
    [HtmlAttributeName("max-items")]
    public int? MaxItems { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number of entries.
    /// </summary>
    /// <value>
    ///     The default is the length from a <see cref="MinLengthAttribute" /> or
    ///     <see cref="LengthAttribute" /> on the bound property, or none.
    /// </value>
    [HtmlAttributeName("min-items")]
    public int? MinItems { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether each entry shows its number.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to number the entries; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("numbered")]
    public bool? Numbered { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the dictionary shows its entries without
    ///     editing.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to show the entries as text, without adding or removing;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A read-only dictionary posts no values.
    /// </remarks>
    [HtmlAttributeName("readonly")]
    public bool? ReadOnly { get; set; }

    /// <summary>
    ///     Gets or sets the texts the editor shows as its entries change, such as its errors.
    /// </summary>
    /// <value>
    ///     The texts, or <see langword="null" /> for the default texts.
    /// </value>
    [HtmlAttributeName("texts")]
    public DictionaryEditorTexts? Texts { get; set; }

    /// <summary>
    ///     Gets or sets how each value is aligned within its column.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="DictionaryEditorAlign.End" /> for numbers and
    ///     <see cref="DictionaryEditorAlign.Start" /> otherwise.
    /// </value>
    [HtmlAttributeName("value-align")]
    public DictionaryEditorAlign? ValueAlign { get; set; }

    /// <summary>
    ///     Gets or sets how a text value's case changes as it is entered.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the case. The default is
    ///     <see cref="DictionaryEditorCasing.None" />.
    /// </value>
    [HtmlAttributeName("value-casing")]
    public DictionaryEditorCasing? ValueCasing { get; set; }

    /// <summary>
    ///     Gets or sets the values to choose from.
    /// </summary>
    /// <value>
    ///     The default is the members of an enum value, or none for a text or number value.
    /// </value>
    /// <remarks>
    ///     With items, the value is a select.
    /// </remarks>
    [HtmlAttributeName("asp-value-items")]
    public IEnumerable<SelectListItem>? ValueItems { get; set; }

    /// <summary>
    ///     Gets or sets the label of the value column.
    /// </summary>
    /// <value>
    ///     The default is "Value".
    /// </value>
    [HtmlAttributeName("value-label")]
    public string? ValueLabel { get; set; }

    /// <summary>
    ///     Gets or sets the largest number a number value can be.
    /// </summary>
    [HtmlAttributeName("value-max")]
    public double? ValueMax { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of characters a text value can have.
    /// </summary>
    [HtmlAttributeName("value-max-length")]
    public int? ValueMaxLength { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number a number value can be.
    /// </summary>
    [HtmlAttributeName("value-min")]
    public double? ValueMin { get; set; }

    /// <summary>
    ///     Gets or sets a regular expression that each text value must match.
    /// </summary>
    [HtmlAttributeName("value-pattern")]
    public string? ValuePattern { get; set; }

    /// <summary>
    ///     Gets or sets the message shown when a value doesn't match <see cref="ValuePattern" />.
    /// </summary>
    /// <value>
    ///     The default is the browser's message.
    /// </value>
    [HtmlAttributeName("value-pattern-message")]
    public string? ValuePatternMessage { get; set; }

    /// <summary>
    ///     Gets or sets the placeholder of an empty value.
    /// </summary>
    [HtmlAttributeName("value-placeholder")]
    public string? ValuePlaceholder { get; set; }

    /// <summary>
    ///     Gets or sets the text shown before each value, such as a currency symbol.
    /// </summary>
    [HtmlAttributeName("value-prefix")]
    public string? ValuePrefix { get; set; }

    /// <summary>
    ///     Gets or sets the step of a number value.
    /// </summary>
    /// <value>
    ///     The default is <c>1</c> for a whole-number type and any step for other numbers.
    /// </value>
    [HtmlAttributeName("value-step")]
    public double? ValueStep { get; set; }

    /// <summary>
    ///     Gets or sets the text shown after each value, such as a unit.
    /// </summary>
    [HtmlAttributeName("value-suffix")]
    public string? ValueSuffix { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The editor has no <c>asp-for</c>, or is bound to a type other than a dictionary with
    ///     a string or enum key and a string, number or enum value.
    /// </exception>
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        if (
            For == null
            || CollectionEditorValue.DictionaryTypes(For.ModelExplorer.ModelType)
                is not var (keyType, valueType)
        )
        {
            throw new InvalidOperationException(
                "A dictionary editor requires asp-for bound to a Dictionary<TKey, TValue> or IDictionary<TKey, TValue> with a string or enum key and a string, number or enum value."
            );
        }

        var effectiveReadOnly = ReadOnly ?? false;
        var effectiveKeyLabel = KeyLabel ?? "Key";
        var effectiveValueLabel = ValueLabel ?? "Value";
        var name = Name ?? ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        var keyItems = KeyItems?.ToList() ?? EnumItems(keyType);
        var valueItems = ValueItems?.ToList() ?? EnumItems(valueType);
        var validators = For.Metadata.ValidatorMetadata;
        var minItems =
            MinItems
            ?? validators.OfType<MinLengthAttribute>().FirstOrDefault()?.Length
            ?? validators.OfType<LengthAttribute>().FirstOrDefault()?.MinimumLength;
        var maxItems =
            MaxItems
            ?? validators.OfType<MaxLengthAttribute>().FirstOrDefault()?.Length
            ?? validators.OfType<LengthAttribute>().FirstOrDefault()?.MaximumLength;
        if (maxItems < 0)
        {
            // MaxLengthAttribute without a length allows any number
            maxItems = null;
        }

        // Each key can be used once, so keys from items are the most entries there can be
        if (keyItems?.Count(item => !item.Disabled) is { } keyCount && !(maxItems <= keyCount))
        {
            maxItems = keyCount;
        }

        var number = valueItems == null && CollectionEditorValue.IsNumber(valueType);
        var text = valueItems == null && !number;
        var model = new CollectionEditorModel
        {
            Id = output.Attributes["id"]?.Value?.ToString() ?? $"sa-{GetUniqueId(context)}",
            Name = name,
            Dictionary = true,
            Parts =
            [
                new CollectionEditorPart
                {
                    Name = "key",
                    Label = effectiveKeyLabel,
                    Type = keyItems == null ? "text" : null,
                    Items = keyItems,
                    MaxLength = keyItems == null ? KeyMaxLength : null,
                    Pattern = keyItems == null ? KeyPattern : null,
                    PatternMessage = keyItems == null ? KeyPatternMessage : null,
                    Casing = keyItems == null ? KeyCasing?.GetDataAttributeText() : null,
                    Placeholder = KeyPlaceholder,
                },
                new CollectionEditorPart
                {
                    Name = "value",
                    Label = effectiveValueLabel,
                    Type =
                        valueItems != null ? null
                        : number ? "number"
                        : "text",
                    Items = valueItems,
                    Step =
                        !number ? null
                        : ValueStep is { } step ? Format(step)
                        : CollectionEditorValue.IsInteger(valueType) ? "1"
                        : "any",
                    Min = number && ValueMin is { } min ? Format(min) : null,
                    Max = number && ValueMax is { } max ? Format(max) : null,
                    MaxLength = text ? ValueMaxLength : null,
                    Pattern = text ? ValuePattern : null,
                    PatternMessage = text ? ValuePatternMessage : null,
                    Casing = text ? ValueCasing?.GetDataAttributeText() : null,
                    Align = (
                        ValueAlign
                        ?? (number ? DictionaryEditorAlign.End : DictionaryEditorAlign.Start)
                    ).GetDataAttributeText(),
                    Placeholder = ValuePlaceholder,
                    Prefix = ValuePrefix,
                    Suffix = ValueSuffix,
                },
            ],
            Rows = CollectionEditorRows.Read(
                ViewContext.ViewData.ModelState,
                name,
                For.Model,
                [keyType, valueType]
            ),
            MinItems = minItems,
            MaxItems = maxItems,
            Numbered = Numbered ?? false,
            Unique = true,
            ReadOnly = effectiveReadOnly,
            AddText = AddText ?? "Add item",
            EmptyText = EmptyText ?? "No items yet",
            Texts = CollectionEditorTexts.ForDictionary(
                effectiveKeyLabel,
                effectiveValueLabel,
                minItems,
                maxItems,
                Texts
            ),
        };
        await new CollectionEditorRenderer(
            context,
            _generator,
            _icons,
            ViewContext,
            model
        ).RenderAsync(output, JoinCssClasses(ClassNames?.Control, output.GetUserSuppliedClass()));

        ApplyFieldAttributes(context, output.Attributes, FieldLayout.Stacked);
        LabelGroup(output.Attributes, model.Id);
        return FieldLayout.Stacked;
    }

    private static string Format(double value) => value.ToString(CultureInfo.InvariantCulture);

    // An enum's members, by name, with their display names
    private List<SelectListItem>? EnumItems(Type type)
    {
        type = CollectionEditorValue.Underlying(type);
        if (!type.IsEnum)
        {
            return null;
        }

        var metadata = ViewContext
            .HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>()
            .GetMetadataForType(type);
        var names = metadata.EnumNamesAndValues!;
        return metadata
            .EnumGroupedDisplayNamesAndValues!.Select(member => new SelectListItem(
                member.Key.Name,
                names.First(pair => pair.Value == member.Value).Key
            ))
            .ToList();
    }
}
