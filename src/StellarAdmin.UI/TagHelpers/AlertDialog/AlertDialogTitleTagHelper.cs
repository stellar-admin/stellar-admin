using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of an alert dialog.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-title")]
public class AlertDialogTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-dialog-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-dialog-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
