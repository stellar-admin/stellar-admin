using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Shared binding and field attributes for radio and checkbox groups.
/// </summary>
public abstract class ChoiceGroupTagHelper : FieldInputBaseTagHelper
{
    private static readonly object SequenceKey = new();
    private readonly IHtmlGenerator _generator;
    private readonly IOptions<IconOptions> _icons;

    /// <summary>
    ///     Whether all options are disabled.
    /// </summary>
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Options to render instead of child item tags.
    /// </summary>
    [HtmlAttributeName("asp-items")]
    public IEnumerable<SelectListItem>? Items { get; set; }

    protected ChoiceGroupTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator)
    {
        _generator = generator;
        _icons = icons;
    }

    protected async Task<AutoFieldConfiguration> RenderAsync(
        TagHelperContext context,
        TagHelperOutput output,
        bool multiple,
        bool card,
        IEnumerable<string>? values
    )
    {
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
        var describedBy = output.Attributes["aria-describedby"]?.Value?.ToString() ?? string.Empty;
        var itemIndex = 0;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var group = new ChoiceGroupContext
        {
            Multiple = multiple,
            RenderItem = async (value, label, itemDescription, disabled, css) =>
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
                    itemDescription,
                    multiple,
                    card,
                    Disabled == true || disabled,
                    invalid,
                    selected.Contains(formatted),
                    describedBy,
                    css
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
                "label",
                "description",
                "error",
                "render-field",
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
            JoinCssClasses(
                "sa-field-group group/field-group",
                multiple && !card ? "[&_[data-slot=field-label]]:font-normal" : null,
                output.GetUserSuppliedClass()
            )
        );
        if (Disabled == true)
        {
            output.Attributes.SetAttribute("aria-disabled", "true");
        }

        if (invalid)
        {
            output.Attributes.SetAttribute("aria-invalid", "true");
        }

        if (
            !output.Attributes.ContainsName("aria-label")
            && !output.Attributes.ContainsName("aria-labelledby")
            && (Label ?? For?.Metadata.DisplayName ?? For?.Name) is { } groupLabel
        )
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

        return new AutoFieldConfiguration(AutoFieldLayout.VerticalDescriptionAfterLabel);
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
        string? description,
        bool multiple,
        bool card,
        bool disabled,
        bool invalid,
        bool selected,
        string describedBy,
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

        if (invalid)
        {
            attributes.Add("aria-invalid", "true");
        }

        var itemDescribedBy = JoinCssClasses(
            describedBy,
            description == null ? null : $"{id}-description"
        );
        if (itemDescribedBy.Length > 0)
        {
            attributes.Add("aria-describedby", itemDescribedBy);
        }

        var input = new TagHelperOutput(
            "input",
            attributes,
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        await new InputTagHelper(_generator, _icons)
        {
            ViewContext = ViewContext,
            ShouldRenderField = false,
        }.ProcessAsync(context, input);

        var field = new FieldTagBuilder(FieldOrientation.Horizontal, card ? null : css);
        if (invalid)
        {
            field.Attributes["data-invalid"] = "true";
        }

        var label = Element(
            card ? "div" : "label",
            "field-label",
            card ? "sa-field-title" : "sa-field-label group/field-label peer/field-label"
        );
        if (!card)
        {
            label.Attributes["for"] = id;
            field.InnerHtml.AppendHtml(input);
        }

        label.InnerHtml.AppendHtml(labelContent);
        if (card || description != null)
        {
            var content = Element("div", "field-content", "sa-field-content group/field-content");
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
        var cardLabel = Element(
            "label",
            "field-label",
            JoinCssClasses("sa-field-label group/field-label peer/field-label", css)
        );
        cardLabel.Attributes["for"] = id;
        cardLabel.InnerHtml.AppendHtml(field);
        return cardLabel;
    }
}
