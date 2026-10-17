using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of related fields in a column with consistent spacing.
/// </summary>
[HtmlTargetElement("sa-field-group")]
public class FieldGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("data-slot"))
        {
            output.Attributes.SetAttribute("data-slot", "field-group");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
