using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Provides the binding and field attributes shared by radio and checkbox groups.
/// </summary>
public abstract class ChoiceGroupTagHelper : FieldInputBaseTagHelper<ChoiceGroupClassNames>
{
    private static readonly object SequenceKey = new();
    private readonly IHtmlGenerator _generator;
    private readonly IOptions<IconOptions> _icons;

    /// <summary>
    ///     Gets or sets the number of columns of options at the smallest width.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is <c>1</c>.
    /// </value>
    /// <remarks>
    ///     Wider breakpoints use this count unless they set their own.
    /// </remarks>
    [HtmlAttributeName("columns")]
    public int? Columns { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns of options when the field is at least 56rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at the next smaller width.
    /// </value>
    [HtmlAttributeName("columns-lg")]
    public int? ColumnsLarge { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns of options when the field is at least 40rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at the next smaller width.
    /// </value>
    [HtmlAttributeName("columns-md")]
    public int? ColumnsMedium { get; set; }

    /// <summary>
    ///     Gets or sets the number of columns of options when the field is at least 30rem wide.
    /// </summary>
    /// <value>
    ///     A number from 1 to 12. The default is the number of columns at the smallest width.
    /// </value>
    [HtmlAttributeName("columns-sm")]
    public int? ColumnsSmall { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether every option is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if every option is disabled; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the options to render in place of child option elements.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with child option elements. Option groups aren't supported.
    /// </remarks>
    [HtmlAttributeName("asp-items")]
    public IEnumerable<SelectListItem>? Items { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the selection can't be changed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the selection can't be changed; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     A read-only group looks like an editable one and can still be focused, and its selection
    ///     is posted.
    /// </remarks>
    [HtmlAttributeName("readonly")]
    public bool? ReadOnly { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChoiceGroupTagHelper" /> class.
    /// </summary>
    /// <param name="generator">The generator that renders the field and its inputs.</param>
    /// <param name="icons">The icon options that supply the mark of a selected checkbox.</param>
    private protected ChoiceGroupTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator)
    {
        _generator = generator;
        _icons = icons;
    }

    /// <summary>
    ///     Renders the group and its options.
    /// </summary>
    /// <param name="context">The context of the element being rendered.</param>
    /// <param name="output">The output of the element being rendered.</param>
    /// <param name="multiple">
    ///     <see langword="true" /> to render checkboxes; otherwise, <see langword="false" /> to render
    ///     radio buttons.
    /// </param>
    /// <param name="card">
    ///     <see langword="true" /> to show the options as cards; otherwise, <see langword="false" />.
    /// </param>
    /// <param name="down">
    ///     <see langword="true" /> to fill each column before the next; otherwise,
    ///     <see langword="false" /> to fill each row first.
    /// </param>
    /// <param name="values">
    ///     The selected values when the group isn't bound, or <see langword="null" /> for none.
    /// </param>
    /// <returns>The layout of the field around the group.</returns>
    /// <exception cref="InvalidOperationException">
    ///     The group has neither <c>asp-for</c> nor <c>name</c>, combines <c>asp-for</c> with
    ///     <c>name</c>, <c>value</c> or <c>values</c>, or is bound to an unsupported type.
    ///     -or-
    ///     The options include an option group, repeat a value, or come from both
    ///     <see cref="Items" /> and child option elements.
    ///     -or-
    ///     A radio group has more than one selected value, or a column count is out of range.
    /// </exception>
    private protected async Task<FieldLayout> RenderAsync(
        TagHelperContext context,
        TagHelperOutput output,
        bool multiple,
        bool card,
        bool down,
        IEnumerable<string>? values
    )
    {
        var columns = GridLayout.ResolveColumns(
            multiple ? "sa-checkbox-group" : "sa-radio-group",
            Columns,
            ColumnsSmall,
            ColumnsMedium,
            ColumnsLarge
        );
        var name =
            For == null ? Name : ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("A choice group requires asp-for or name.");
        }

        if (
            For != null
            && (
                Name != null
                || values != null
                || context.AllAttributes.ContainsName("value")
                || context.AllAttributes.ContainsName("values")
            )
        )
        {
            throw new InvalidOperationException(
                "asp-for cannot be combined with name, value, or values."
            );
        }

        var valueType =
            For == null ? null
            : multiple ? ChoiceGroupValue.ElementType(For.ModelExplorer.ModelType)
            : For.ModelExplorer.ModelType;
        if (
            For != null
            && (
                valueType == null
                || !ChoiceGroupValue.IsSupported(valueType)
                || (multiple && Nullable.GetUnderlyingType(valueType) != null)
            )
        )
        {
            throw new InvalidOperationException(
                "Radio groups require a supported scalar; checkbox groups require an array or list of non-nullable scalar values (excluding byte[])."
            );
        }

        var items = Items?.ToList();
        if (items?.Any(item => item.Group != null) == true)
        {
            throw new InvalidOperationException(
                "SelectListGroup is not supported; use separate choice groups."
            );
        }

        var hasState =
            ViewContext.ViewData.ModelState.TryGetValue(name, out var entry)
            && entry.RawValue != null;
        var selection = (
            hasState ? ChoiceGroupValue.Values(entry!.RawValue)
            : For != null ? ChoiceGroupValue.Values(For.Model)
            : values
                ?? items
                    ?.Where(item => item.Selected)
                    .Select(item => (object?)(item.Value ?? item.Text))
                ?? []
        ).Where(value => value != null).Select(value => ChoiceGroupValue.Normalize(value!, valueType));
        var selected = (hasState && !multiple ? selection.Take(1) : selection).ToHashSet(
            StringComparer.Ordinal
        );
        if (!multiple && selected.Count > 1)
        {
            throw new InvalidOperationException("A radio group can have only one selected value.");
        }

        var sequence = (ViewContext.HttpContext.Items[SequenceKey] as int? ?? 0) + 1;
        ViewContext.HttpContext.Items[SequenceKey] = sequence;
        var id =
            output.Attributes["id"]?.Value?.ToString()
            ?? $"sa-choice-{GetUniqueId(context)}-{sequence}";
        var invalid = Error != null || entry?.Errors.Count > 0;
        var readOnly = ReadOnly == true && Disabled != true;
        var itemIndex = 0;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var group = new ChoiceGroupContext
        {
            Multiple = multiple,
            RenderItem = async (value, label, media, itemDescription, disabled, css) =>
            {
                var formatted = ChoiceGroupValue.Normalize(value, valueType);
                if (!seen.Add(formatted))
                {
                    throw new InvalidOperationException(
                        "Choice group option values must be unique."
                    );
                }

                return await RenderItemAsync(
                    context,
                    name,
                    $"{id}-{++itemIndex}",
                    formatted,
                    label,
                    media,
                    itemDescription,
                    multiple,
                    card,
                    Disabled == true || disabled,
                    readOnly,
                    invalid,
                    selected.Contains(formatted),
                    JoinCssClasses(ClassNames?.Item, css)
                );
            },
        };
        SetContext(context, group);
        var children = await output.GetChildContentAsync();
        if (items != null && (group.ItemCount > 0 || !children.IsEmptyOrWhiteSpace))
        {
            throw new InvalidOperationException("Use asp-items or child items, not both.");
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        foreach (
            var attribute in new[]
            {
                "name",
                "value",
                "values",
                "variant",
                "disabled",
                "readonly",
                "label",
                "description",
                "error",
                "wrap-in-field",
                "asp-for",
                "asp-items",
            }
        )
        {
            output.Attributes.RemoveAll(attribute);
        }

        output.Attributes.SetAttribute("id", id);
        output.Attributes.SetAttribute("data-slot", multiple ? "checkbox-group" : "radio-group");
        output.Attributes.SetAttribute("role", multiple ? "group" : "radiogroup");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-group", ClassNames?.Control, output.GetUserSuppliedClass())
        );
        // The options fill a grid measured against the field around the group. Flowing down, each
        // breakpoint also gets the row count that fills a column before the next one starts.
        if (ResponsiveGrid.ColumnsStyle(columns) is { } columnsStyle)
        {
            output.Attributes.SetAttribute("data-columns", "multiple");
            if (down)
            {
                var rows = ResponsiveGrid.ResolveRows(columns, items?.Count ?? group.ItemCount);
                output.Attributes.SetAttribute("data-flow", "down");
                if (ResponsiveGrid.RowsStyle(rows) is { } rowsStyle)
                {
                    columnsStyle = $"{columnsStyle};{rowsStyle}";
                }
            }

            output.AppendStyle(columnsStyle);
        }

        if (Disabled == true)
        {
            output.Attributes.SetAttribute("aria-disabled", "true");
        }

        // A checkbox group's role has no read-only state, so its checkboxes carry it
        if (readOnly && !multiple)
        {
            output.Attributes.SetAttribute("aria-readonly", "true");
        }

        if (invalid)
        {
            output.Attributes.SetAttribute("aria-invalid", "true");
        }

        // The group carries the field's descriptions, so each option announces only its own
        ApplyFieldAttributes(context, output.Attributes, FieldLayout.StackedDescriptionFirst);

        // A label cannot target a div, so a rendered field label is referenced by id instead
        var labelled =
            output.Attributes.ContainsName("aria-label")
            || output.Attributes.ContainsName("aria-labelledby");
        if (WillRenderFieldLabel())
        {
            LabelId = $"{id}-label";
            if (!labelled)
            {
                output.Attributes.SetAttribute("aria-labelledby", LabelId);
            }
        }
        else if (!labelled && (Label ?? For?.Metadata.DisplayName ?? For?.Name) is { } groupLabel)
        {
            output.Attributes.SetAttribute("aria-label", groupLabel);
        }

        output.Content.Clear();
        if (items == null)
        {
            output.Content.AppendHtml(children);
        }
        else
        {
            foreach (var item in items)
            {
                output.Content.AppendHtml(
                    await group.RenderItem(
                        item.Value ?? item.Text,
                        new HtmlContentBuilder().Append(item.Text),
                        null,
                        null,
                        item.Disabled,
                        null
                    )
                );
            }
        }

        if (multiple && Disabled != true)
        {
            var marker = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            marker.Attributes["type"] = "hidden";
            marker.Attributes["name"] = CheckboxGroupModelBinderProvider.MarkerPrefix + name;
            marker.Attributes["value"] = "true";
            output.Content.AppendHtml(marker);
        }

        return FieldLayout.StackedDescriptionFirst;
    }

    private static TagBuilder Element(string tag, string slot, string css)
    {
        var element = new TagBuilder(tag);
        element.Attributes["data-slot"] = slot;
        element.Attributes["class"] = css;
        return element;
    }

    private async Task<IHtmlContent> RenderItemAsync(
        TagHelperContext context,
        string name,
        string id,
        string value,
        IHtmlContent labelContent,
        IHtmlContent? media,
        string? description,
        bool multiple,
        bool card,
        bool disabled,
        bool readOnly,
        bool invalid,
        bool selected,
        string? css
    )
    {
        var attributes = new TagHelperAttributeList
        {
            { "type", multiple ? "checkbox" : "radio" },
            { "name", name },
            { "id", id },
            { "value", value },
        };
        if (selected)
        {
            attributes.Add("checked", "checked");
        }

        if (disabled)
        {
            attributes.Add("disabled", "disabled");
        }
        else if (readOnly)
        {
            attributes.Add("readonly", "readonly");
        }

        if (invalid)
        {
            attributes.Add("aria-invalid", "true");
        }

        if (description != null)
        {
            attributes.Add("aria-describedby", $"{id}-description");
        }

        var input = new TagHelperOutput(
            "input",
            attributes,
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        await new InputTagHelper(_generator, _icons)
        {
            ViewContext = ViewContext,
            WrapInField = false,
        }.ProcessAsync(context, input);

        var field = new FieldTagBuilder(FieldOrientation.Horizontal, card ? null : css);
        if (invalid)
        {
            field.Attributes["data-invalid"] = "true";
        }

        var label = Element(
            card ? "div" : "label",
            "field-label",
            card ? "sa-field-title" : "sa-field-label"
        );
        if (!card)
        {
            label.Attributes["for"] = id;
            field.InnerHtml.AppendHtml(input);
        }

        // Media leads a card, and sits before the text of a plain option
        if (media != null && !card)
        {
            label.InnerHtml.AppendHtml(media);
        }

        label.InnerHtml.AppendHtml(labelContent);
        if (media != null && card)
        {
            field.InnerHtml.AppendHtml(media);
        }

        if (card || description != null)
        {
            var content = Element("div", "field-content", "sa-field-content");
            content.InnerHtml.AppendHtml(label);
            if (description != null)
            {
                var text = Element("p", "field-description", "sa-field-description");
                text.Attributes["id"] = $"{id}-description";
                text.InnerHtml.Append(description);
                content.InnerHtml.AppendHtml(text);
            }

            field.InnerHtml.AppendHtml(content);
        }
        else
        {
            field.InnerHtml.AppendHtml(label);
        }

        if (!card)
        {
            return field;
        }

        field.InnerHtml.AppendHtml(input);
        var cardLabel = Element("label", "field-label", JoinCssClasses("sa-field-label", css));
        cardLabel.Attributes["for"] = id;
        cardLabel.InnerHtml.AppendHtml(field);
        return cardLabel;
    }
}
