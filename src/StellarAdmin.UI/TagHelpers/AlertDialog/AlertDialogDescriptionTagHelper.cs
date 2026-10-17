using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of an alert dialog, shown below the title.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-description")]
public class AlertDialogDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-dialog-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-dialog-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
