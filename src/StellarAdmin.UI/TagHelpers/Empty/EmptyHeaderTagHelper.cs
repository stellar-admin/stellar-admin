using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the header of an empty state, which holds the media, title and description.
/// </summary>
[HtmlTargetElement("sa-empty-header")]
public class EmptyHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-empty-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
