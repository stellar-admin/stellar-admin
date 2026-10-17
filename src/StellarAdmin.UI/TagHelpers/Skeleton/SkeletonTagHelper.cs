using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a pulsing placeholder shape to show while content loads.
/// </summary>
/// <remarks>
///     Set the size and shape with the <c>class</c> attribute.
/// </remarks>
[HtmlTargetElement("sa-skeleton")]
public class SkeletonTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "skeleton");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-skeleton", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
