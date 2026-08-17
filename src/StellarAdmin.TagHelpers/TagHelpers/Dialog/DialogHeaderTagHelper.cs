using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The header region of a dialog; typically contains the title and description.
/// </summary>
[HtmlTargetElement("sa-dialog-header")]
public class DialogHeaderTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dialog-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dialog-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
