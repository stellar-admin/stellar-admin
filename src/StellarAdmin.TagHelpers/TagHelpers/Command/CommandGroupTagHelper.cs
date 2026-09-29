using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A set of related items in a command menu, with an optional heading. The group is hidden
///     when none of its items match the search.
/// </summary>
[HtmlTargetElement("sa-command-group")]
public class CommandGroupTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The heading shown above the group's items.
    /// </summary>
    [HtmlAttributeName("heading")]
    public string? Heading { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "presentation");
        output.Attributes.SetAttribute("data-slot", "command-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-command-group", output.GetUserSuppliedClass())
        );

        var items = new TagBuilder("div");
        items.Attributes["role"] = "group";

        if (!string.IsNullOrEmpty(Heading))
        {
            // Groups rendered in a loop share a unique id, so number them within their command
            var commandContext = GetContext<CommandContext>(context);
            var headingId =
                commandContext == null
                    ? $"sa-command-group-{GetUniqueId(context)}-heading"
                    : $"{commandContext.ListId}-group-{++commandContext.GroupCount}-heading";

            // Themes style the heading through its data-slot, so it must be kept.
            var heading = new TagBuilder("div");
            heading.Attributes["id"] = headingId;
            heading.Attributes["aria-hidden"] = "true";
            heading.Attributes["data-slot"] = "command-group-heading";
            heading.InnerHtml.Append(Heading);
            output.PreContent.AppendHtml(heading);

            items.Attributes["aria-labelledby"] = headingId;
        }

        output.PreContent.AppendHtml(items.RenderStartTag());
        output.PostContent.AppendHtml(items.RenderEndTag());

        return Task.CompletedTask;
    }
}
