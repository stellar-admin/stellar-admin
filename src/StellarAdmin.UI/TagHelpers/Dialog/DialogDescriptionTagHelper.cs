using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of a dialog, below its title.
/// </summary>
[HtmlTargetElement("sa-dialog-description")]
public class DialogDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
