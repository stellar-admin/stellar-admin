using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group that combines an input with add-ons, buttons or text into one field.
/// </summary>
[HtmlTargetElement("sa-input-group")]
public sealed class InputGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "group");
        output.Attributes.SetAttribute("data-slot", "input-group");

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
