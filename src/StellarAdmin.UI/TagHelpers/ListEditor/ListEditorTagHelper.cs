using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an editor for a list of text or number values, with a row per item.
/// </summary>
/// <remarks>
///     Adding, removing and reordering items requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-list-editor", TagStructure = TagStructure.WithoutEndTag)]
public sealed class ListEditorTagHelper : FieldInputBaseTagHelper<ListEditorClassNames>
{
    private readonly IHtmlGenerator _generator;
    private readonly IOptions<IconOptions> _icons;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ListEditorTagHelper" /> class.
    /// </summary>
    /// <param name="generator">The generator that renders the field and the inputs.</param>
    /// <param name="icons">The icon options that supply the add, remove and drag icons.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="generator" /> is <see langword="null" />.
    /// </exception>
    public ListEditorTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _icons = icons;
    }

    /// <summary>
    ///     Gets or sets the text of the button that adds an item.
    /// </summary>
    /// <value>
    ///     The default is "Add item".
    /// </value>
    [HtmlAttributeName("add-text")]
    public string? AddText { get; set; }

    /// <summary>
    ///     Gets or sets how each item is aligned within its row.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the alignment. The default is
    ///     <see cref="ListEditorAlign.End" /> for numbers and <see cref="ListEditorAlign.Start" />
    ///     for text.
    /// </value>
    [HtmlAttributeName("align")]
    public ListEditorAlign? Align { get; set; }

    /// <summary>
    ///     Gets or sets how an item's case changes as it is entered.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the case. The default is
    ///     <see cref="ListEditorCasing.None" />.
    /// </value>
    [HtmlAttributeName("casing")]
    public ListEditorCasing? Casing { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the list is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the list is disabled; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A disabled list can't be focused, and its values aren't posted.
    /// </remarks>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the text shown when the list has no items.
    /// </summary>
    /// <value>
    ///     The default is "No items yet".
    /// </value>
    [HtmlAttributeName("empty-text")]
    public string? EmptyText { get; set; }

    /// <summary>
    ///     Gets or sets the largest number a number item can be.
    /// </summary>
    [HtmlAttributeName("max")]
    public double? Max { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of items.
    /// </summary>
    /// <value>
    ///     The default is the length from a <see cref="MaxLengthAttribute" /> or
    ///     <see cref="LengthAttribute" /> on the bound property, or no limit.
    /// </value>
    [HtmlAttributeName("max-items")]
    public int? MaxItems { get; set; }

    /// <summary>
    ///     Gets or sets the largest number of characters a text item can have.
    /// </summary>
    [HtmlAttributeName("max-length")]
    public int? MaxLength { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number a number item can be.
    /// </summary>
    [HtmlAttributeName("min")]
    public double? Min { get; set; }

    /// <summary>
    ///     Gets or sets the smallest number of items.
    /// </summary>
    /// <value>
    ///     The default is the length from a <see cref="MinLengthAttribute" /> or
    ///     <see cref="LengthAttribute" /> on the bound property, or none.
    /// </value>
    [HtmlAttributeName("min-items")]
    public int? MinItems { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether each item shows its number.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to number the items; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("numbered")]
    public bool? Numbered { get; set; }

    /// <summary>
    ///     Gets or sets a regular expression that each text item must match.
    /// </summary>
    [HtmlAttributeName("pattern")]
    public string? Pattern { get; set; }

    /// <summary>
    ///     Gets or sets the message shown when an item doesn't match <see cref="Pattern" />.
    /// </summary>
    /// <value>
    ///     The default is the browser's message.
    /// </value>
    [HtmlAttributeName("pattern-message")]
    public string? PatternMessage { get; set; }

    /// <summary>
    ///     Gets or sets the placeholder of an empty item.
    /// </summary>
    [HtmlAttributeName("placeholder")]
    public string? Placeholder { get; set; }

    /// <summary>
    ///     Gets or sets the text shown before each item, such as a currency symbol.
    /// </summary>
    [HtmlAttributeName("prefix")]
    public string? Prefix { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the items can't be changed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the items can't be changed, added, removed or reordered;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A read-only list shows its items in read-only inputs, which can still be focused, and its
    ///     values are posted.
    /// </remarks>
    [HtmlAttributeName("readonly")]
    public bool? ReadOnly { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the items can be reordered.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to reorder items by dragging their handle or with Alt+Up and
    ///     Alt+Down; otherwise, <see langword="false" />. The default is
    ///     <see langword="false" />.
    /// </value>
    [HtmlAttributeName("sortable")]
    public bool? Sortable { get; set; }

    /// <summary>
    ///     Gets or sets the step of a number item.
    /// </summary>
    /// <value>
    ///     The default is <c>1</c> for a whole-number type and any step for other numbers.
    /// </value>
    [HtmlAttributeName("step")]
    public double? Step { get; set; }

    /// <summary>
    ///     Gets or sets the text shown after each item, such as a unit.
    /// </summary>
    [HtmlAttributeName("suffix")]
    public string? Suffix { get; set; }

    /// <summary>
    ///     Gets or sets the texts the editor shows as its items change, such as its errors.
    /// </summary>
    /// <value>
    ///     The texts, or <see langword="null" /> for the default texts.
    /// </value>
    [HtmlAttributeName("texts")]
    public ListEditorTexts? Texts { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether each item must be different.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to reject an item that is already in the list, ignoring case;
    ///     otherwise, <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("unique")]
    public bool? Unique { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The editor has no <c>asp-for</c>, or is bound to a type other than an array or list of
    ///     text or numbers.
    /// </exception>
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var elementType =
            For == null ? null : ChoiceGroupValue.ElementType(For.ModelExplorer.ModelType);
        if (elementType == null || !CollectionEditorValue.IsListElement(elementType))
        {
            throw new InvalidOperationException(
                "A list editor requires asp-for bound to an array or list of strings or numbers."
            );
        }

        var effectiveDisabled = Disabled ?? false;
        var effectiveReadOnly = !effectiveDisabled && (ReadOnly ?? false);
        var name = Name ?? ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For!.Name);
        var validators = For!.Metadata.ValidatorMetadata;
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

        var number = CollectionEditorValue.IsNumber(elementType);
        var model = new CollectionEditorModel
        {
            Id = output.Attributes["id"]?.Value?.ToString() ?? $"sa-{GetUniqueId(context)}",
            Name = name,
            Dictionary = false,
            Parts =
            [
                new CollectionEditorPart
                {
                    Name = "value",
                    Label = "Item",
                    Type = number ? "number" : "text",
                    Step =
                        !number ? null
                        : Step is { } step ? Format(step)
                        : CollectionEditorValue.IsInteger(elementType) ? "1"
                        : "any",
                    Min = number && Min is { } min ? Format(min) : null,
                    Max = number && Max is { } max ? Format(max) : null,
                    MaxLength = number ? null : MaxLength,
                    Pattern = number ? null : Pattern,
                    PatternMessage = number ? null : PatternMessage,
                    Casing = number ? null : Casing?.GetDataAttributeText(),
                    Align = (
                        Align ?? (number ? ListEditorAlign.End : ListEditorAlign.Start)
                    ).GetDataAttributeText(),
                    Placeholder = Placeholder,
                    Prefix = Prefix,
                    Suffix = Suffix,
                },
            ],
            Rows = CollectionEditorRows.Read(
                ViewContext.ViewData.ModelState,
                name,
                For.Model,
                [elementType]
            ),
            MinItems = minItems,
            MaxItems = maxItems,
            Numbered = Numbered ?? false,
            Sortable = !effectiveReadOnly && (Sortable ?? false),
            Unique = Unique ?? false,
            Disabled = effectiveDisabled,
            ReadOnly = effectiveReadOnly,
            AddText = AddText ?? "Add item",
            EmptyText = EmptyText ?? "No items yet",
            Texts = CollectionEditorTexts.ForList(minItems, maxItems, Texts),
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
}
