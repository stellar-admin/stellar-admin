using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A title for a field or field set that is styled like a label but is not associated with a control.
/// </summary>
[HtmlTargetElement("sa-field-title")]
public class FieldTitleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-label");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
