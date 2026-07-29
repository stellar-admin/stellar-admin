using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The title heading of a dialog.
/// </summary>
[HtmlTargetElement("sa-dialog-title")]
public class DialogTitleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.Add("data-slot", "dialog-title");
        output.Attributes.Add(
            "class",
            JoinCssClasses(
                "sa-dialog-title",
                "sa-font-heading",
                "font-heading",
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
