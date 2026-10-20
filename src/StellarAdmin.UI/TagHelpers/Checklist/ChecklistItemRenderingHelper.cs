using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

// Plain and link items share their content: the status, then the title and description from
// their attributes, then the children.
internal static class ChecklistItemRenderingHelper
{
    public static IHtmlContent RenderContent(
        TagHelperContext context,
        TagHelperOutput output,
        ChecklistItemContext item,
        TagHelperContent childContent,
        IconOptions iconOptions,
        string tagName,
        ChecklistStatus? status,
        string? title,
        string? description
    )
    {
        ThrowIfSetTwice(tagName, "status", status is not null, item.HasStatus);
        ThrowIfSetTwice(tagName, "title", !string.IsNullOrEmpty(title), item.HasTitle);
        ThrowIfSetTwice(
            tagName,
            "description",
            !string.IsNullOrEmpty(description),
            item.HasDescription
        );

        var content = new HtmlContentBuilder();

        if (!item.HasStatus)
        {
            var effectiveStatus = status ?? ChecklistStatus.Todo;

            output.Attributes.SetAttribute("data-status", effectiveStatus.GetDataAttributeText());
            content.AppendHtml(RenderStatus(context, iconOptions, effectiveStatus));
        }

        if (!string.IsNullOrEmpty(title))
        {
            content.AppendHtml(RenderPart("div", "checklist-item-title", title));
        }

        if (!string.IsNullOrEmpty(description))
        {
            content.AppendHtml(RenderPart("p", "checklist-item-description", description));
        }

        content.AppendHtml(childContent);

        return content;
    }

    // The status reads as text to assistive technology, ahead of the title.
    public static TagBuilder RenderStatusText(string label)
    {
        var text = new TagBuilder("span");
        text.AddCssClass("sr-only");
        text.InnerHtml.Append($"{label}: ");

        return text;
    }

    private static TagBuilder RenderStatus(
        TagHelperContext context,
        IconOptions iconOptions,
        ChecklistStatus status
    )
    {
        var iconOutput = new TagHelperOutput(
            "svg",
            [new TagHelperAttribute("aria-hidden", "true")],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
        var iconTagHelper = new IconTagHelper(iconOptions)
        {
            Name = iconOptions.GetSemanticIconName(status.GetIconRole()),
        };
        iconTagHelper.Process(context, iconOutput);

        var statusElement = new TagBuilder("span");
        statusElement.AddCssClass("sa-checklist-item-status");
        statusElement.Attributes["data-slot"] = "checklist-item-status";
        statusElement.InnerHtml.AppendHtml(iconOutput);
        statusElement.InnerHtml.AppendHtml(RenderStatusText(status.GetLabel()));

        return statusElement;
    }

    private static TagBuilder RenderPart(string tagName, string slot, string text)
    {
        var part = new TagBuilder(tagName);
        part.AddCssClass($"sa-{slot}");
        part.Attributes["data-slot"] = slot;
        part.InnerHtml.Append(text);

        return part;
    }

    private static void ThrowIfSetTwice(
        string tagName,
        string attributeName,
        bool attributeIsSet,
        bool childIsSet
    )
    {
        if (attributeIsSet && childIsSet)
        {
            throw new InvalidOperationException(
                $"<{tagName}> can't have both the '{attributeName}' attribute and a <sa-checklist-item-{attributeName}> child."
            );
        }
    }
}
