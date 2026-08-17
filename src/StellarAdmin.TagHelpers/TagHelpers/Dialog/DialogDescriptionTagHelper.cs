using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The descriptive body text of a dialog, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-dialog-description")]
public class DialogDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dialog-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dialog-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
