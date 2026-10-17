using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the content of an empty state below its header, such as actions.
/// </summary>
[HtmlTargetElement("sa-empty-content")]
public sealed class EmptyContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-empty-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
