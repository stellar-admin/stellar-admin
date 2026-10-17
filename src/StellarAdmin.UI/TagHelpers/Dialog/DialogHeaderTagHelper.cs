using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the header of a dialog, which holds the title and description.
/// </summary>
[HtmlTargetElement("sa-dialog-header")]
public class DialogHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
