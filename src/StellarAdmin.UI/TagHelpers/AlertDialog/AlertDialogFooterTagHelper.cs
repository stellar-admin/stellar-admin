using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the footer of an alert dialog, which holds the cancel and confirm buttons.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-footer")]
public sealed class AlertDialogFooterTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-dialog-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-dialog-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
