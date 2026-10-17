using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of a dialog.
/// </summary>
[HtmlTargetElement("sa-dialog-title")]
public class DialogTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "dialog-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-dialog-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
