using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Arranges form content in equal-width columns that stack in narrow containers.
/// </summary>
[HtmlTargetElement("sa-form-row")]
public class FormRowTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "form-row");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-form-row", output.GetUserSuppliedClass())
        );

        var content = new TagBuilder("div");
        content.Attributes["data-slot"] = "form-row-content";
        content.AddCssClass("sa-form-row-content");
        content.InnerHtml.AppendHtml(await output.GetChildContentAsync());
        output.Content.SetHtmlContent(content);
    }
}
