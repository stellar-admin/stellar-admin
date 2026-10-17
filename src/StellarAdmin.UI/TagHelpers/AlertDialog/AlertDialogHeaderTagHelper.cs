using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the header of an alert dialog, which holds the title and description.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-header")]
public sealed class AlertDialogHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-dialog-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-dialog-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
