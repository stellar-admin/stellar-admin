using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders text or an icon inside an input group, usually in an add-on.
/// </summary>
[HtmlTargetElement("sa-input-group-text")]
public sealed class InputGroupTextTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input-group-text", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
