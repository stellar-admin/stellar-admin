using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the footer of a sheet.
/// </summary>
/// <remarks>
///     The footer usually holds the sheet's action buttons.
/// </remarks>
[HtmlTargetElement("sa-sheet-footer")]
public sealed class SheetFooterTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-footer");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sheet-footer", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
