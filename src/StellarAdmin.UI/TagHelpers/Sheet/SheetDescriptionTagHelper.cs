using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of a sheet, shown below its title.
/// </summary>
[HtmlTargetElement("sa-sheet-description")]
public class SheetDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sheet-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
