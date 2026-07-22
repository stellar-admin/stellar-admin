using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     The content region of an empty state; typically contains actions or supplementary
///     elements below the header.
/// </summary>
[HtmlTargetElement("sa-empty-content")]
public class EmptyContentTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-empty-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
