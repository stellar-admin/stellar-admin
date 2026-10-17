using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the footer of a dialog, which holds its action buttons.
/// </summary>
[HtmlTargetElement("sa-dialog-footer")]
public sealed class DialogFooterTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dialog-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dialog-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
