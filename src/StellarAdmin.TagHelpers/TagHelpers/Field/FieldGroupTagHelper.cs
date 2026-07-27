using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Groups a set of related fields together, arranging them in a column with consistent spacing.
/// </summary>
[HtmlTargetElement("sa-field-group")]
public class FieldGroupTagHelper : StellarAdminTagHelperBase
{
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
            JoinCssClasses("sa-field-group", "group/field-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
