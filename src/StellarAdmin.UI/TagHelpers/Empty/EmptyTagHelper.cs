using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an empty state, which tells the user there is no content to show.
/// </summary>
[HtmlTargetElement("sa-empty")]
public sealed class EmptyTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-empty", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
