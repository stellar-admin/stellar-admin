using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the header of a sheet.
/// </summary>
/// <remarks>
///     The header usually holds the sheet's title and description.
/// </remarks>
[HtmlTargetElement("sa-sheet-header")]
public sealed class SheetHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sheet-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
