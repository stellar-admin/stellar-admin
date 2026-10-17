using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of a sheet as a heading.
/// </summary>
[HtmlTargetElement("sa-sheet-title")]
public class SheetTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "sheet-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-sheet-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
