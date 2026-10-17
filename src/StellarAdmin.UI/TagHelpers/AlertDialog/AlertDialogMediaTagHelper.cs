using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an area in an alert dialog for media, such as an icon or illustration.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-media")]
public sealed class AlertDialogMediaTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "alert-dialog-media");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-dialog-media", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
