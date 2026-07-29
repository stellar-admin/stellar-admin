using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The accessible title of a sheet, rendered as a heading in the sheet header.
/// </summary>
[HtmlTargetElement("sa-sheet-title")]
public class SheetTitleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "sheet-title");
        output.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-sheet-title",
                "sa-font-heading",
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
