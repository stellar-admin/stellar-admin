using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a label for a progress bar.
/// </summary>
[HtmlTargetElement("sa-progress-label")]
public sealed class ProgressLabelTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "progress-label");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-progress-label", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
