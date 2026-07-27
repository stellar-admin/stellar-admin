using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A container that holds a field's label and description, keeping them stacked together
///     when the field is laid out horizontally alongside its control.
/// </summary>
[HtmlTargetElement("sa-field-content")]
public class FieldContentTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-content");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-content", "group/field-content", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
