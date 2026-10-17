using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the list that holds the pagination items.
/// </summary>
[HtmlTargetElement("sa-pagination-content")]
public sealed class PaginationContentTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "ul";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "pagination-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-pagination-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
