using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Groups related fields under a common legend, rendered as a <c>&lt;fieldset&gt;</c> element.
/// </summary>
[HtmlTargetElement("sa-field-set")]
public class FieldSetTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "fieldset";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "field-set");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-field-set", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
