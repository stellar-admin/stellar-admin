using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

// Renders the box shared by the list and dictionary editors: a row per item, an empty message, Add
// as the last row, and a template row that sel-collection-editor clones for Add
internal sealed class CollectionEditorRenderer(
    TagHelperContext context,
    IHtmlGenerator generator,
    IOptions<IconOptions> icons,
    ViewContext viewContext,
    CollectionEditorModel model
)
{
    public async Task RenderAsync(TagHelperOutput output, string? classes)
    {
        output.TagName = "sel-collection-editor";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.RemoveAll("name");
        output.Attributes.SetAttribute("id", model.Id);
        output.Attributes.SetAttribute(
            "data-slot",
            model.Dictionary ? "dictionary-editor" : "list-editor"
        );
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses("sa-collection-editor", classes)
        );
        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-kind", model.Dictionary ? "dictionary" : "list");
        output.Attributes.SetAttribute("data-name", model.Name);
        SetFlag(output, "data-numbered", model.Numbered);
        SetFlag(output, "data-sortable", model.Sortable);
        SetFlag(output, "data-unique", model.Unique);
        SetFlag(output, "data-readonly", model.ReadOnly);
        if (model.MinItems is { } minItems)
        {
            output.Attributes.SetAttribute("data-min-items", Number(minItems));
        }

        if (model.MaxItems is { } maxItems)
        {
            output.Attributes.SetAttribute("data-max-items", Number(maxItems));
        }

        if (!model.ReadOnly)
        {
            output.Attributes.SetAttribute("data-texts", JsonSerializer.Serialize(model.Texts));
        }

        var content = output.Content.Clear();
        var empty = model.Rows.Count == 0;
        if (model.Dictionary)
        {
            content.AppendHtml(Head(empty));
        }

        var items = Element("ol", "sa-collection-editor-items");
        items.Attributes["data-slot"] = "collection-editor-items";
        if (empty)
        {
            items.Attributes["hidden"] = "hidden";
        }

        for (var index = 0; index < model.Rows.Count; index++)
        {
            items.InnerHtml.AppendHtml(await RowAsync(model.Rows[index], index));
        }

        content.AppendHtml(items);

        var emptyMessage = Element("div", "sa-collection-editor-empty");
        emptyMessage.Attributes["data-slot"] = "collection-editor-empty";
        if (!empty)
        {
            emptyMessage.Attributes["hidden"] = "hidden";
        }

        emptyMessage.InnerHtml.Append(model.ReadOnly ? model.Texts["none"] : model.EmptyText);
        content.AppendHtml(emptyMessage);
        if (model.ReadOnly)
        {
            return;
        }

        content.AppendHtml(await AddAsync());

        var template = new TagBuilder("template");
        template.InnerHtml.AppendHtml(
            await RowAsync(
                new CollectionEditorRow(
                    model.Parts.Select(_ => "").ToList(),
                    model.Parts.Select(_ => (string?)null).ToList()
                ),
                null
            )
        );
        content.AppendHtml(template);

        // Posted with the rows, so that removing every row binds an empty collection
        var marker = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        marker.Attributes["type"] = "hidden";
        marker.Attributes["name"] = CheckboxGroupModelBinderProvider.MarkerPrefix + model.Name;
        marker.Attributes["value"] = "true";
        content.AppendHtml(marker);
    }

    private static TagBuilder Element(string tag, string css)
    {
        var element = new TagBuilder(tag);
        element.Attributes["class"] = css;
        return element;
    }

    private static string Format(string text, int number) =>
        text.Replace("{n}", Number(number), StringComparison.Ordinal);

    private static string Number(int number) => number.ToString(CultureInfo.InvariantCulture);

    private static void SetFlag(TagHelperOutput output, string name, bool value)
    {
        if (value)
        {
            output.Attributes.SetAttribute(name, "");
        }
    }

    private async Task<IHtmlContent> AddAsync()
    {
        var full = model.MaxItems is { } max && model.Rows.Count >= max;
        var add = new TagHelperOutput(
            "button",
            [
                new TagHelperAttribute("type", "button"),
                new TagHelperAttribute("class", "sa-collection-editor-add"),
                new TagHelperAttribute("data-slot", "collection-editor-add"),
                new TagHelperAttribute("command", "--add"),
                new TagHelperAttribute("commandfor", model.Id),
            ],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        if (full)
        {
            add.Attributes.Add("disabled", "disabled");
            add.Attributes.Add("title", model.Texts["full"]);
        }

        var text = new TagBuilder("span");
        text.InnerHtml.Append(model.AddText);
        add.Content.AppendHtml(await IconAsync(SemanticIconRole.CollectionEditorAdd, null));
        add.Content.AppendHtml(text);

        // The count shows only when there is a maximum
        if (model.MaxItems != null)
        {
            var count = Element("span", "sa-collection-editor-count");
            count.Attributes["data-slot"] = "collection-editor-count";
            if (model.Rows.Count == 0)
            {
                count.Attributes["hidden"] = "hidden";
            }

            count.InnerHtml.Append(
                model
                    .Texts["count"]
                    .Replace("{count}", Number(model.Rows.Count), StringComparison.Ordinal)
            );
            add.Content.AppendHtml(count);
        }

        return add;
    }

    private IHtmlContent Button(
        string css,
        string label,
        IHtmlContent content,
        ButtonSize size,
        string? command
    )
    {
        var button = new TagHelperOutput(
            "button",
            [new TagHelperAttribute("type", "button"), new TagHelperAttribute("class", css)],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        ButtonRenderingHelper.RenderAttributes(button, ButtonVariant.Ghost, size);
        button.Attributes.SetAttribute("aria-label", label);
        if (command != null)
        {
            button.Attributes.SetAttribute("command", command);
            button.Attributes.SetAttribute("commandfor", model.Id);
        }

        button.Content.SetHtmlContent(content);
        return button;
    }

    // A text or number input, or a select of the part's items
    private async Task<IHtmlContent> ControlAsync(
        CollectionEditorPart part,
        string value,
        int? index,
        string? errorId
    )
    {
        var number = (index ?? 0) + 1;
        var name =
            index == null ? ""
            : model.Dictionary
                ? $"{model.Name}[{Number(index.Value)}].{(part.Name == "key" ? "Key" : "Value")}"
            : $"{model.Name}[{Number(index.Value)}]";
        var attributes = new TagHelperAttributeList
        {
            { "aria-label", Format(model.Texts[part.Name], number) },
            { "data-part", part.Name },
        };
        if (name.Length > 0)
        {
            attributes.Insert(0, new TagHelperAttribute("name", name));
        }

        if (errorId != null)
        {
            attributes.Add("aria-invalid", "true");
            attributes.Add("aria-describedby", errorId);
        }

        if (part.Items != null)
        {
            var options = new HtmlContentBuilder();
            var placeholder = new TagBuilder("option");
            placeholder.Attributes["value"] = "";
            placeholder.InnerHtml.Append(part.Placeholder ?? "");
            options.AppendHtml(placeholder);

            // A key can be used once, so the keys other rows use are disabled
            var used =
                part.Name == "key" && index != null
                    ? model
                        .Rows.Where((_, other) => other != index)
                        .Select(row => row.Values[0])
                        .ToHashSet(StringComparer.Ordinal)
                    : [];
            foreach (var item in part.Items)
            {
                var option = new TagBuilder("option");
                var optionValue = item.Value ?? item.Text;
                option.Attributes["value"] = optionValue;
                if (optionValue == value)
                {
                    option.Attributes["selected"] = "selected";
                }

                if (item.Disabled || used.Contains(optionValue))
                {
                    option.Attributes["disabled"] = "disabled";
                }

                option.InnerHtml.Append(item.Text);
                options.AppendHtml(option);
            }

            var select = new TagHelperOutput(
                "sa-select",
                attributes,
                (_, _) =>
                    Task.FromResult<TagHelperContent>(
                        new DefaultTagHelperContent().SetHtmlContent(options)
                    )
            );
            await new SelectTagHelper(generator, icons)
            {
                ViewContext = viewContext,
                WrapInField = false,
            }.ProcessAsync(context, select);
            return select;
        }

        attributes.Insert(0, new TagHelperAttribute("type", part.Type ?? "text"));
        attributes.Add("value", value);
        if (part.Type == "number")
        {
            attributes.Add("inputmode", part.Step == "1" ? "numeric" : "decimal");
            attributes.Add("step", part.Step ?? "any");
        }

        AddIfSet(attributes, "min", part.Min);
        AddIfSet(attributes, "max", part.Max);
        AddIfSet(
            attributes,
            "maxlength",
            part.MaxLength is { } maxLength ? Number(maxLength) : null
        );
        AddIfSet(attributes, "pattern", part.Pattern);
        AddIfSet(attributes, "data-pattern-message", part.PatternMessage);
        AddIfSet(attributes, "data-casing", part.Casing);
        AddIfSet(attributes, "placeholder", part.Placeholder);

        var input = new TagHelperOutput(
            "sa-input",
            attributes,
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        await new InputTagHelper(generator, icons.Value)
        {
            ViewContext = viewContext,
            WrapInField = false,
        }.ProcessAsync(context, input);
        return input;
    }

    private static void AddIfSet(TagHelperAttributeList attributes, string name, string? value)
    {
        if (value != null)
        {
            attributes.Add(name, value);
        }
    }

    private async Task<IHtmlContent> CellAsync(
        CollectionEditorPart part,
        string value,
        int? index,
        string? errorId
    )
    {
        var cell = Element("div", "sa-collection-editor-cell");
        cell.Attributes["data-col"] = part.Name;
        if (part.Align != null)
        {
            cell.Attributes["data-align"] = part.Align;
        }

        if (part.Prefix != null)
        {
            cell.InnerHtml.AppendHtml(Affix(part.Prefix, "start"));
        }

        if (model.ReadOnly)
        {
            var text = Element("span", "sa-collection-editor-text");
            text.InnerHtml.Append(
                part.Items?.FirstOrDefault(item => (item.Value ?? item.Text) == value)?.Text
                    ?? value
            );
            cell.InnerHtml.AppendHtml(text);
        }
        else
        {
            cell.InnerHtml.AppendHtml(await ControlAsync(part, value, index, errorId));
        }

        if (part.Suffix != null)
        {
            cell.InnerHtml.AppendHtml(Affix(part.Suffix, "end"));
        }

        return cell;
    }

    private static TagBuilder Affix(string text, string side)
    {
        var affix = Element("span", "sa-collection-editor-affix");
        affix.Attributes["data-side"] = side;
        affix.Attributes["aria-hidden"] = "true";
        affix.InnerHtml.Append(text);
        return affix;
    }

    // The column heads of a dictionary: the key's and the value's labels
    private TagBuilder Head(bool empty)
    {
        var head = Element("div", "sa-collection-editor-head");
        head.Attributes["aria-hidden"] = "true";
        head.Attributes["data-slot"] = "collection-editor-head";
        if (empty)
        {
            head.Attributes["hidden"] = "hidden";
        }

        if (model.Numbered || model.Sortable)
        {
            var number = Element("span", "sa-collection-editor-head-gutter");
            if (model.Numbered)
            {
                number.InnerHtml.Append("#");
            }

            head.InnerHtml.AppendHtml(number);
        }

        foreach (var part in model.Parts)
        {
            var label = new TagBuilder("span");
            if (part.Align != null)
            {
                label.Attributes["data-align"] = part.Align;
            }

            label.InnerHtml.Append(part.Label);
            head.InnerHtml.AppendHtml(label);
        }

        if (!model.ReadOnly)
        {
            head.InnerHtml.AppendHtml(new TagBuilder("span"));
        }

        return head;
    }

    private async Task<IHtmlContent> IconAsync(SemanticIconRole role, string? css)
    {
        var icon = new TagHelperOutput(
            string.Empty,
            css == null
                ? [new TagHelperAttribute("aria-hidden", "true")]
                :
                [
                    new TagHelperAttribute("class", css),
                    new TagHelperAttribute("aria-hidden", "true"),
                ],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        await new IconTagHelper(icons.Value)
        {
            Name = icons.Value.GetSemanticIconName(role),
        }.ProcessAsync(context, icon);
        return icon;
    }

    // A row, or the template row when the index is null
    private async Task<IHtmlContent> RowAsync(CollectionEditorRow row, int? index)
    {
        var number = (index ?? 0) + 1;
        var item = Element("li", "sa-collection-editor-item");
        item.Attributes["data-slot"] = "collection-editor-item";
        var error = row.Errors.FirstOrDefault(message => message != null);
        var errorId = index == null ? null : $"{model.Id}-{Number(index.Value)}-error";
        if (error != null)
        {
            item.Attributes["data-invalid"] = "true";
        }

        if (model.Numbered || model.Sortable)
        {
            var gutter = Element("span", "sa-collection-editor-gutter");
            var numberText = Element("span", "sa-collection-editor-number");
            numberText.InnerHtml.Append(Number(number));
            if (model.Sortable)
            {
                var content = new HtmlContentBuilder();
                if (model.Numbered)
                {
                    content.AppendHtml(numberText);
                }

                content.AppendHtml(await IconAsync(SemanticIconRole.CollectionEditorHandle, null));
                gutter.InnerHtml.AppendHtml(
                    Button(
                        "sa-collection-editor-handle",
                        Format(model.Texts["move"], number),
                        content,
                        ButtonSize.Icon,
                        null
                    )
                );
            }
            else
            {
                gutter.Attributes["aria-hidden"] = "true";
                gutter.InnerHtml.AppendHtml(numberText);
            }

            item.InnerHtml.AppendHtml(gutter);
        }

        for (var part = 0; part < model.Parts.Count; part++)
        {
            item.InnerHtml.AppendHtml(
                await CellAsync(
                    model.Parts[part],
                    row.Values[part],
                    index,
                    row.Errors[part] == null ? null : errorId
                )
            );
        }

        if (!model.ReadOnly)
        {
            var actions = Element("span", "sa-collection-editor-actions");
            actions.InnerHtml.AppendHtml(
                Button(
                    "sa-collection-editor-remove",
                    Format(model.Texts["remove"], number),
                    await IconAsync(SemanticIconRole.CollectionEditorRemove, null),
                    ButtonSize.IconSmall,
                    "--remove"
                )
            );
            item.InnerHtml.AppendHtml(actions);
        }

        if (error != null)
        {
            var message = Element("p", "sa-collection-editor-item-error");
            message.Attributes["id"] = errorId!;
            message.Attributes["data-slot"] = "collection-editor-item-error";
            message.InnerHtml.Append(error);
            item.InnerHtml.AppendHtml(message);
        }

        return item;
    }
}
