using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A responsive data table, rendered as a <c>&lt;table&gt;</c> inside a scrollable
///     container. Compose it with the header, body, footer, row, head, cell, and caption
///     subcomponents.
/// </summary>
[HtmlTargetElement("sa-table")]
public class TableTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-container");
        output.Attributes.SetAttribute("class", "sa-table-container");

        var tableTagBuilder = new TagBuilder("table");
        tableTagBuilder.Attributes.Add("data-slot", "table");
        tableTagBuilder.Attributes.Add(
            "class",
            JoinCssClasses("sa-table", output.GetUserSuppliedClass())
        );
        tableTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.Content.AppendHtml(tableTagBuilder);
    }
}
